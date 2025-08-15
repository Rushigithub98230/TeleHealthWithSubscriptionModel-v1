using SmartTelehealth.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IUserService
    {
        // --- AUTHENTICATION ---
        Task<UserDto?> AuthenticateUserAsync(string email, string password);
        Task<UserDto?> GetUserByEmailAsync(string email);
        
        // --- USER MANAGEMENT ---
        Task<JsonModel> GetUserByIdAsync(int userId);
        Task<JsonModel> UpdateUserAsync(int userId, UpdateUserDto updateDto);
        Task<JsonModel> DeleteUserAsync(int userId);
        Task<JsonModel> GetUsersByRoleAsync(string role);
        Task<JsonModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<JsonModel> ResetPasswordAsync(string email);
        Task<JsonModel> ConfirmPasswordResetAsync(string email, string resetToken, string newPassword);
        Task<JsonModel> UpdateUserProfileAsync(int userId, UpdateUserProfileDto profileDto);
        Task<JsonModel> UpdateUserPreferencesAsync(int userId, UpdateUserPreferencesDto preferencesDto);
        
        // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
        Task<JsonModel> UploadProfilePictureAsync(int userId, IFormFile file);
        Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType = null);
        Task<JsonModel> UploadUserDocumentAsync(int userId, UploadUserDocumentRequest request);
        Task<JsonModel> DeleteUserDocumentAsync(Guid documentId, int userId);
        
        // --- PROVIDER MANAGEMENT ---
        Task<JsonModel> GetProviderByIdAsync(int providerId);
        Task<JsonModel> UpdateProviderAsync(int providerId, UpdateProviderDto updateProviderDto);
        Task<JsonModel> GetAllProvidersAsync();
        
        // --- MEDICAL HISTORY ---
        Task<JsonModel> GetMedicalHistoryAsync(int userId);
        Task<JsonModel> UpdateMedicalHistoryAsync(int userId, UpdateMedicalHistoryDto medicalHistoryDto);
        
        // --- PAYMENT METHODS ---
        Task<JsonModel> GetPaymentMethodsAsync(int userId);
        Task<JsonModel> AddPaymentMethodAsync(int userId, AddPaymentMethodDto addPaymentMethodDto);
        Task<JsonModel> DeletePaymentMethodAsync(int userId, string paymentMethodId);
        Task<JsonModel> SetDefaultPaymentMethodAsync(int userId, string paymentMethodId);
        
        // --- ADDITIONAL METHODS ---
        Task<JsonModel> GetUserAsync(int userId);
        Task<JsonModel> GetAllUsersAsync();
        Task<JsonModel> CreateUserAsync(CreateUserDto createUserDto);
    }
} 