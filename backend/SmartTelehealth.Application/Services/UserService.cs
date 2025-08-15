using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IStripeService _stripeService;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IDocumentService _documentService;
    private readonly IDocumentTypeService _documentTypeService;
    private readonly IUserRoleRepository _userRoleRepository;

    public UserService(
        IUserRepository userRepository,
        INotificationService notificationService,
        IStripeService stripeService,
        ILogger<UserService> logger,
        UserManager<User> userManager,
        IMapper mapper,
        IDocumentService documentService,
        IDocumentTypeService documentTypeService,
        IUserRoleRepository userRoleRepository)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _stripeService = stripeService;
        _logger = logger;
        _userManager = userManager;
        _mapper = mapper;
        _documentService = documentService;
        _documentTypeService = documentTypeService;
        _userRoleRepository = userRoleRepository;
    }

    // --- AUTHENTICATION METHODS ---
    public async Task<UserDto?> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            // Use the repository to find user by email instead of UserManager
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            // Use custom password verification instead of Identity
            var isValidPassword = VerifyPassword(password, user.PasswordHash);
            if (!isValidPassword)
                return null;

            return MapToUserDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            return null;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    // User profile operations
    public async Task<JsonModel> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            var userDto = MapToUserDto(user);
            return new JsonModel { data = userDto, Message = "User retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get user: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserAsync(int userId, UpdateUserDto updateDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // Update user properties
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;
            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;
            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;
            if (updateDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;
            if (!string.IsNullOrEmpty(updateDto.City))
                user.City = updateDto.City;
            if (!string.IsNullOrEmpty(updateDto.State))
                user.State = updateDto.State;
            if (!string.IsNullOrEmpty(updateDto.ZipCode))
                user.ZipCode = updateDto.ZipCode;
            if (!string.IsNullOrEmpty(updateDto.Country))
                user.Country = updateDto.Country;

            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            return new JsonModel { data = userDto, Message = "User updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update user: {ex.Message}", StatusCode = 500 };
        }
    }

    // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
    
    public async Task<JsonModel> UploadProfilePictureAsync(int userId, IFormFile file)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Get profile picture document type
            var profilePictureType = await _documentTypeService.GetByNameAsync("Profile Picture");
            if (profilePictureType == null)
                return new JsonModel { data = new object(), Message = "Profile picture document type not found", StatusCode = 404 };

            // Read file content
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var uploadRequest = new UploadUserDocumentRequest
            {
                FileData = fileBytes,
                FileName = file.FileName,
                ContentType = file.ContentType,
                UserId = userId,
                ReferenceType = "profile_picture",
                Description = $"Profile picture for user {user.FirstName} {user.LastName}",
                IsPublic = true, // Profile pictures are typically public
                IsEncrypted = false,
                DocumentTypeId = profilePictureType.Data?.DocumentTypeId ?? Guid.Empty,
                CreatedById = userId
            };

            // Upload using centralized document service
            var result = await _documentService.UploadUserDocumentAsync(uploadRequest);
            
            if (result.Success)
            {
                // Update user profile picture URL
                user.ProfilePicture = result.Data?.DownloadUrl ?? result.Data?.FilePath;
                await _userRepository.UpdateAsync(user);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to upload profile picture: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType = null)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Get documents using centralized document service
            var documentsResult = await _documentService.GetUserDocumentsAsync(userId, referenceType);
            return new JsonModel { data = .SuccessResponse(documentsResult.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get user documents: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UploadUserDocumentAsync(int userId, UploadUserDocumentRequest request)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Override user ID to ensure it's linked to the correct user
            request.UserId = userId;
            request.CreatedById = userId;

            // Upload using centralized document service
            var result = await _documentService.UploadUserDocumentAsync(request);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to upload user document: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 };

            // Delete using centralized document service
            var result = await _documentService.DeleteDocumentAsync(documentId, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} for user {UserId}", documentId, userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete user document: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            await _userRepository.DeleteAsync(userId);
            return new JsonModel { data = true, Message = "User deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete user: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUsersByRoleAsync(string role)
    {
        try
        {
            var users = await _userRepository.GetByRoleAsync(role);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return new JsonModel { data = userDtos, Message = $"Users with role {role} retrieved successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role {Role}", role);
            return new JsonModel { data = new object(), Message = $"Failed to get users by role: {ex.Message}", StatusCode = 500 };
        }
    }



    public async Task<JsonModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // Verify current password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                return new JsonModel { data = new object(), Message = "Current password is incorrect", StatusCode = 500 };

            // Hash new password
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Password changed successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to change password: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResetPasswordAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Send reset email
            await _notificationService.SendPasswordResetEmailAsync(email, resetToken);

            return new JsonModel { data = true, Message = "Password reset email sent successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to reset password: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ConfirmPasswordResetAsync(string email, string resetToken, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            if (user.PasswordResetToken != resetToken)
                return new JsonModel { data = new object(), Message = "Invalid reset token", StatusCode = 500 };

            if (user.ResetTokenExpires < DateTime.UtcNow)
                return new JsonModel { data = new object(), Message = "Reset token has expired", StatusCode = 500 };

            // Update password
            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Password reset successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming password reset for email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to confirm password reset: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserProfileAsync(int userId, UpdateUserProfileDto profileDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // Update profile properties
            if (!string.IsNullOrEmpty(profileDto.FirstName))
                user.FirstName = profileDto.FirstName;
            if (!string.IsNullOrEmpty(profileDto.LastName))
                user.LastName = profileDto.LastName;
            if (!string.IsNullOrEmpty(profileDto.PhoneNumber))
                user.PhoneNumber = profileDto.PhoneNumber;
            if (profileDto.DateOfBirth.HasValue)
                user.DateOfBirth = profileDto.DateOfBirth.Value;
            if (!string.IsNullOrEmpty(profileDto.Address))
                user.Address = profileDto.Address;
            if (!string.IsNullOrEmpty(profileDto.City))
                user.City = profileDto.City;
            if (!string.IsNullOrEmpty(profileDto.State))
                user.State = profileDto.State;
            if (!string.IsNullOrEmpty(profileDto.ZipCode))
                user.ZipCode = profileDto.ZipCode;
            if (!string.IsNullOrEmpty(profileDto.Country))
                user.Country = profileDto.Country;

            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Profile updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update profile: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserPreferencesAsync(int userId, UpdateUserPreferencesDto preferencesDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // Update preferences
            if (!string.IsNullOrEmpty(preferencesDto.NotificationPreferences))
                user.NotificationPreferences = preferencesDto.NotificationPreferences;
            if (!string.IsNullOrEmpty(preferencesDto.LanguagePreference))
                user.LanguagePreference = preferencesDto.LanguagePreference;
            if (!string.IsNullOrEmpty(preferencesDto.TimeZonePreference))
                user.TimeZonePreference = preferencesDto.TimeZonePreference;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Preferences updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    // Patient operations
    public async Task<JsonModel> GetPatientByIdAsync(int patientId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(patientId);
            if (user == null || user.UserType != "Patient")
                return new JsonModel { data = new object(), Message = "Patient not found", StatusCode = 500 };

            var patientDto = MapToPatientDto(user);
            
            // Get additional patient data
            var stats = await GetPatientStatsAsync(patientId);
            var statsObj = await GetPatientStatsAsync(patientId);
            if (statsObj is not null)
            {
                var statsData = (dynamic)statsObj;
                patientDto.TotalAppointments = statsData.TotalAppointments ?? 0;
                patientDto.CompletedAppointments = statsData.CompletedAppointments ?? 0;
                patientDto.CancelledAppointments = statsData.CancelledAppointments ?? 0;
                patientDto.TotalSpent = statsData.TotalSpent ?? 0m;
            }

            return new JsonModel { data = patientDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by ID: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to get patient: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllPatientsAsync()
    {
        try
        {
            var patients = await _userRepository.GetByUserTypeAsync("Patient");
            var patientDtos = patients.Select(MapToPatientDto).ToList();
            return new JsonModel { data = .SuccessResponse(patientDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return new JsonModel { data = new object(), Message = $"Failed to get patients: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetPatientMedicalHistoryAsync(int patientId)
    {
        try
        {
            // This would typically fetch from a medical history repository
            var medicalHistory = new
            {
                Allergies = "None known",
                CurrentMedications = "None",
                MedicalHistory = "No significant medical history",
                FamilyHistory = "No significant family history",
                Lifestyle = "Non-smoker, occasional alcohol"
            };

            return new JsonModel { data = medicalHistory, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient medical history: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to get medical history: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdatePatientMedicalHistoryAsync(int patientId, UpdateMedicalHistoryDto medicalHistoryDto)
    {
        try
        {
            // This would typically update a medical history repository
            var updatedHistory = new
            {
                Allergies = medicalHistoryDto.Allergies,
                CurrentMedications = medicalHistoryDto.CurrentMedications,
                MedicalHistory = medicalHistoryDto.MedicalHistory,
                FamilyHistory = medicalHistoryDto.FamilyHistory,
                Lifestyle = medicalHistoryDto.Lifestyle,
                UpdatedAt = DateTime.UtcNow
            };

            return new JsonModel { data = updatedHistory, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient medical history: {PatientId}", patientId);
            return new JsonModel { data = new object(), Message = $"Failed to update medical history: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider operations
    public async Task<JsonModel> GetProviderAsync(string id)
    {
        if (!int.TryParse(id, out var providerId))
            return new JsonModel { data = new object(), Message = "Invalid provider ID", StatusCode = 500 };
        var provider = await _userRepository.GetByIdAsync(providerId);
        if (provider == null)
            return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };
        var providerDto = _mapper.Map<ProviderDto>(provider);
        return new JsonModel { data = providerDto, Message = "Success", StatusCode = 200 };
    }

    public async Task<JsonModel> GetProviderByEmailAsync(string email)
    {
        try
        {
            var provider = await _userRepository.GetByEmailAsync(email);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };
            }

            var providerDto = MapToProviderDto(provider);
            return new JsonModel { data = providerDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by email {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetProviderByLicenseAsync(string licenseNumber)
    {
        try
        {
            var provider = await _userRepository.GetByLicenseNumberAsync(licenseNumber);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };
            }

            var providerDto = MapToProviderDto(provider);
            return new JsonModel { data = providerDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by license {LicenseNumber}", licenseNumber);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetProviderByIdAsync(int providerId)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };
            }

            var providerDto = _mapper.Map<ProviderDto>(provider);
            return new JsonModel { data = providerDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider by ID {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to get provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllProvidersAsync()
    {
        try
        {
            var providers = await _userRepository.GetByUserTypeAsync("Provider");
            var userDtos = providers.Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                // ... map other properties as needed ...
            }).ToList();
            return new JsonModel { data = .SuccessResponse(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all providers");
            return new JsonModel { data = new object(), Message = $"Failed to get providers: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateProviderAsync(CreateProviderDto createDto)
    {
        try
        {
            var provider = new User
            {
                Id = 0, // Will be set by the database
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                Phone = createDto.PhoneNumber,
                UserType = "Provider",
                IsActive = true,
                IsEmailVerified = false,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(provider);

            return JsonModel.SuccessResponse(MapToProviderDto(provider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider");
            return new JsonModel { data = new object(), Message = $"Failed to create provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateProviderAsync(int providerId, UpdateProviderDto updateDto)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };

            // Update provider properties
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                provider.FirstName = updateDto.FirstName;
            if (!string.IsNullOrEmpty(updateDto.LastName))
                provider.LastName = updateDto.LastName;
            if (!string.IsNullOrEmpty(updateDto.Email))
                provider.Email = updateDto.Email;
            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                provider.Phone = updateDto.PhoneNumber;

            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            return JsonModel.SuccessResponse(MapToProviderDto(provider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to update provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteProviderAsync(int providerId)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };

            provider.IsActive = false;
            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to delete provider: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> VerifyProviderAsync(int providerId)
    {
        try
        {
            var provider = await _userRepository.GetByIdAsync(providerId);
            if (provider == null || provider.UserType != "Provider")
                return new JsonModel { data = new object(), Message = "Provider not found", StatusCode = 500 };

            // In a real implementation, this would involve verification logic
            provider.IsEmailVerified = true;
            provider.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(provider);

            return JsonModel.SuccessResponse(MapToProviderDto(provider));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying provider: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to verify provider: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider schedule operations
    public async Task<JsonModel> GetProviderScheduleAsync(int providerId)
    {
        try
        {
            // This would typically fetch from a schedule repository
            var schedule = new ProviderScheduleDto
            {
                ProviderId = providerId.ToString(),
                WeeklySchedule = new List<WeeklyScheduleDto>
                {
                    new WeeklyScheduleDto { DayOfWeek = "1", DayName = "Monday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "2", DayName = "Tuesday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "3", DayName = "Wednesday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "4", DayName = "Thursday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "5", DayName = "Friday", IsAvailable = true, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17) },
                    new WeeklyScheduleDto { DayOfWeek = "6", DayName = "Saturday", IsAvailable = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero },
                    new WeeklyScheduleDto { DayOfWeek = "0", DayName = "Sunday", IsAvailable = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero }
                },
                DefaultDurationMinutes = 30,
                IsActive = true
            };

            return new JsonModel { data = schedule, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider schedule: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to get provider schedule: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateProviderScheduleAsync(int providerId, UpdateProviderScheduleDto scheduleDto)
    {
        try
        {
            // This would typically update a schedule repository
            var updatedSchedule = new ProviderScheduleDto
            {
                ProviderId = providerId.ToString(),
                WeeklySchedule = scheduleDto.WeeklySchedule,
                AvailableDates = scheduleDto.AvailableDates,
                UnavailableDates = scheduleDto.UnavailableDates,
                DefaultDurationMinutes = scheduleDto.DefaultDurationMinutes,
                IsActive = scheduleDto.IsActive
            };

            return new JsonModel { data = updatedSchedule, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider schedule: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to update provider schedule: {ex.Message}", StatusCode = 500 };
        }
    }

    // User statistics
    public async Task<JsonModel> GetUserStatsAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            var stats = user.UserType == "Patient" 
                ? await GetPatientStatsAsync(userId)
                : await GetProviderStatsAsync(userId);

            return new JsonModel { data = stats, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get user stats: {ex.Message}", StatusCode = 500 };
        }
    }

    // Provider reviews
    public async Task<JsonModel> GetProviderReviewsAsync(int providerId)
    {
        try
        {
            // This would typically fetch from a reviews repository
            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    Id = Guid.NewGuid().ToString(),
                    ProviderId = providerId.ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "John Doe",
                    Rating = 5,
                    Comment = "Excellent consultation. Very professional and knowledgeable.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ReviewDto
                {
                    Id = Guid.NewGuid().ToString(),
                    ProviderId = providerId.ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "Jane Smith",
                    Rating = 4,
                    Comment = "Good experience. Would recommend.",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return new JsonModel { data = .SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider reviews: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to get provider reviews: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> AddProviderReviewAsync(int providerId, int userId, AddReviewDto reviewDto)
    {
        try
        {
            var review = new ReviewDto
            {
                Id = Guid.NewGuid().ToString(),
                ProviderId = providerId.ToString(),
                PatientId = userId.ToString(),
                PatientName = "Current User", // Would get from user data
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return new JsonModel { data = review, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding provider review: {ProviderId}", providerId);
            return new JsonModel { data = new object(), Message = $"Failed to add review: {ex.Message}", StatusCode = 500 };
        }
    }

    // Notifications
    public async Task<JsonModel> GetUserNotificationsAsync(int userId)
    {
        try
        {
            // This would typically fetch from a notifications repository
            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId.ToString(),
                    Title = "Appointment Reminder",
                    Message = "Your appointment with Dr. Smith is in 1 hour.",
                    Type = "appointment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new NotificationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId.ToString(),
                    Title = "Payment Successful",
                    Message = "Your payment of $75 has been processed successfully.",
                    Type = "payment",
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            return new JsonModel { data = .SuccessResponse(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get notifications: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkNotificationAsReadAsync(Guid notificationId)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to mark notification as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        try
        {
            // This would typically update a notifications repository
            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to mark all notifications as read: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteNotificationAsync(Guid notificationId)
    {
        try
        {
            // This would typically delete from a notifications repository
            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
            return new JsonModel { data = new object(), Message = $"Failed to delete notification: {ex.Message}", StatusCode = 500 };
        }
    }

    // User preferences
    public async Task<JsonModel> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            // This would typically fetch from a preferences repository
            var preferences = new
            {
                EmailNotifications = true,
                SMSNotifications = false,
                AppointmentReminders = true,
                MarketingEmails = false,
                Language = "en",
                TimeZone = "UTC"
            };

            return new JsonModel { data = preferences, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesDto preferencesDto)
    {
        try
        {
            // This would typically update a preferences repository
            return new JsonModel { data = preferencesDto.Preferences, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to update preferences: {ex.Message}", StatusCode = 500 };
        }
    }

    // Password operations
    private string HashPassword(string password)
    {
        // Simple password hashing implementation - in production use BCrypt.Net.BCrypt
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        // Simple password verification - in production use BCrypt.Net.BCrypt
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }

    public async Task<JsonModel> RequestPasswordResetAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            // In a real implementation, send password reset email
            await _notificationService.SendPasswordResetEmailAsync(email, "reset-token");

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset: {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to request password reset: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResetPasswordAsync(ResetPasswordDto resetDto)
    {
        try
        {
            // In a real implementation, verify token and update password
            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return new JsonModel { data = new object(), Message = $"Failed to reset password: {ex.Message}", StatusCode = 500 };
        }
    }

    // Email verification
    public async Task<JsonModel> VerifyEmailAsync(string token)
    {
        try
        {
            // In a real implementation, verify email token
            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return new JsonModel { data = new object(), Message = $"Failed to verify email: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> SendEmailVerificationAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };
            }

            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await _notificationService.SendEmailVerificationAsync(email, user.UserName, verificationToken);
            _logger.LogInformation("Email notifications disabled - would have sent email verification to {Email}", email);

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification to {Email}", email);
            return new JsonModel { data = new object(), Message = $"Failed to send email verification: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ResendEmailVerificationAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };
            }

            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // EMAIL FUNCTIONALITY DISABLED - Commented out for now
            // await _notificationService.SendEmailVerificationAsync(user.Email, user.UserName, verificationToken);
            _logger.LogInformation("Email notifications disabled - would have sent email verification to {Email}", user.Email);

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification for user {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to resend email verification: {ex.Message}", StatusCode = 500 };
        }
    }

    // Account management
    public async Task<JsonModel> DeleteAccountAsync(int userId, string reason)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to delete account: {ex.Message}", StatusCode = 500 };
        }
    }

    // Helper methods
    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Phone = user.Phone ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            UserType = user.UserType,
            Role = user.RoleName,
            UserRoleId = user.UserRoleId.ToString(),
            IsActive = user.IsActive,
            IsVerified = user.IsEmailVerified,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            ProfilePicture = user.ProfilePicture,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Address = user.Address,
            City = user.City,
            State = user.State,
            ZipCode = user.ZipCode,
            Country = user.Country,
            EmergencyContact = user.EmergencyContact,
            EmergencyPhone = user.EmergencyPhone,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private PatientDto MapToPatientDto(User user)
    {
        return new PatientDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Phone = user.Phone,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            ProfilePicture = user.ProfilePicture,
            UserType = user.UserType,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private ProviderDto MapToProviderDto(User user)
    {
        return new ProviderDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}",
            Phone = user.Phone,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            ProfilePicture = user.ProfilePicture,
            UserType = user.UserType,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            CreatedAt = user.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = user.UpdatedDate ?? DateTime.UtcNow,
            LastLoginAt = user.LastLoginAt,
            StripeCustomerId = user.StripeCustomerId
        };
    }

    private async Task<object> GetPatientStatsAsync(int patientId)
    {
        // This would typically fetch from appointment and payment repositories
        return new
        {
            TotalAppointments = 15,
            CompletedAppointments = 12,
            CancelledAppointments = 2,
            TotalSpent = 450.00m
        };
    }

    private async Task<object> GetProviderStatsAsync(int providerId)
    {
        // This would typically fetch from appointment and payment repositories
        return new
        {
            TotalAppointments = 150,
            CompletedAppointments = 140,
            TotalEarnings = 10500.00m
        };
    }

    // === BEGIN INTERFACE STUBS ===
    public async Task<JsonModel> GetUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new JsonModel { data = new object(), Message = "User not found", StatusCode = 500 };
            }

            var userDto = MapToUserDto(user);
            return new JsonModel { data = userDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", userId);
            return new JsonModel { data = new object(), Message = $"Failed to get user: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(MapToUserDto).ToList();
            return new JsonModel { data = .SuccessResponse(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return new JsonModel { data = new object(), Message = $"Failed to get users: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                return new JsonModel { data = new object(), Message = "User with this email already exists", StatusCode = 500 };
            }

            // Get the appropriate UserRole from database
            var userRole = await GetUserRoleByNameAsync(createUserDto.UserType);
            if (userRole == null)
            {
                return new JsonModel { data = new object(), Message = $"Invalid user type: {createUserDto.UserType}", StatusCode = 500 };
            }

            // Create new user
            var user = new User
            {
                Id = 0, // Will be set by the database
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                Phone = createUserDto.PhoneNumber,
                PhoneNumber = createUserDto.PhoneNumber,
                UserType = createUserDto.UserType,
                UserRoleId = userRole.Id, // UserRole.Id is already int
                Gender = createUserDto.Gender,
                Address = createUserDto.Address,
                City = createUserDto.City,
                State = createUserDto.State,
                ZipCode = createUserDto.ZipCode,
                Country = createUserDto.Country,
                EmergencyContact = createUserDto.EmergencyContactName,
                EmergencyPhone = createUserDto.EmergencyContactPhone,
                DateOfBirth = createUserDto.DateOfBirth ?? DateTime.UtcNow,
                IsActive = true,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            // Hash the password
            user.PasswordHash = HashPassword(createUserDto.Password);

            // Save user to database
            await _userRepository.CreateAsync(user);

            // Map to DTO and return
            var userDto = MapToUserDto(user);
            return new JsonModel { data = userDto, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", createUserDto.Email);
            return new JsonModel { data = new object(), Message = $"Failed to create user: {ex.Message}", StatusCode = 500 };
        }
    }
    public async Task<JsonModel> GetMedicalHistoryAsync(int userId)
    {
        throw new NotImplementedException();
    }
    public async Task<JsonModel> UpdateMedicalHistoryAsync(int userId, UpdateMedicalHistoryDto medicalHistoryDto)
    {
        throw new NotImplementedException();
    }
            public async Task<JsonModel> GetPaymentMethodsAsync(int userId)
    {
        throw new NotImplementedException();
    }
    public async Task<JsonModel> AddPaymentMethodAsync(int userId, AddPaymentMethodDto addPaymentMethodDto)
    {
        throw new NotImplementedException();
    }
    public async Task<JsonModel> DeletePaymentMethodAsync(int userId, string paymentMethodId)
    {
        throw new NotImplementedException();
    }
    public async Task<JsonModel> SetDefaultPaymentMethodAsync(int userId, string paymentMethodId)
    {
        throw new NotImplementedException();
    }
    // === END INTERFACE STUBS ===

    // === BEGIN INTERFACE OVERLOADS ===
    // Removed conflicting method overloads - interface methods now use int parameters

    private async Task<UserRole> GetUserRoleByNameAsync(string userTypeName)
    {
        // First, try to get all UserRoles to see what's available
        var allUserRoles = await _userRoleRepository.GetAllAsync();
        _logger.LogInformation("Available UserRoles: {UserRoles}", string.Join(", ", allUserRoles.Select(ur => ur.Name)));
        
        // Try exact match first
        var userRole = await _userRoleRepository.GetByNameAsync(userTypeName);
        if (userRole != null)
        {
            _logger.LogInformation("Found UserRole: {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
            return userRole;
        }
        
        // Try case-insensitive match
        userRole = allUserRoles.FirstOrDefault(ur => 
            ur.Name.Equals(userTypeName, StringComparison.OrdinalIgnoreCase));
        
        if (userRole != null)
        {
            _logger.LogInformation("Found UserRole (case-insensitive): {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
            return userRole;
        }
        
        // Try mapping common variations
        var roleMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Admin", "Admin" },
            { "Administrator", "Admin" },
            { "User", "Client" },
            { "Patient", "Client" },
            { "Client", "Client" },
            { "Provider", "Provider" },
            { "Doctor", "Provider" },
            { "Physician", "Provider" },
            { "Support", "Support" },
            { "CustomerSupport", "Support" }
        };
        
        if (roleMapping.TryGetValue(userTypeName, out var mappedRole))
        {
            userRole = allUserRoles.FirstOrDefault(ur => 
                ur.Name.Equals(mappedRole, StringComparison.OrdinalIgnoreCase));
            
            if (userRole != null)
            {
                _logger.LogInformation("Found UserRole (mapped): {UserRoleName} with ID: {UserRoleId}", userRole.Name, userRole.Id);
                return userRole;
            }
        }
        
        _logger.LogWarning("No UserRole found for userType: {UserType}. Available roles: {AvailableRoles}", 
            userTypeName, string.Join(", ", allUserRoles.Select(ur => ur.Name)));
        
        return null;
    }
} 