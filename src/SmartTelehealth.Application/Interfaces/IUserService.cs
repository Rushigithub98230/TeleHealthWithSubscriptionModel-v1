using SmartTelehealth.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateDto);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId);
        Task<ApiResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(string role);
        Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<ApiResponse<bool>> ResetPasswordAsync(string email);
        Task<ApiResponse<bool>> ConfirmPasswordResetAsync(string email, string resetToken, string newPassword);
        Task<ApiResponse<bool>> UpdateUserProfileAsync(string userId, UpdateUserProfileDto profileDto);
        Task<ApiResponse<bool>> UpdateUserPreferencesAsync(string userId, UpdateUserPreferencesDto preferencesDto);
        
        // Missing methods from controllers
        Task<ApiResponse<UserDto>> GetUserAsync(string userId);
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllProvidersAsync();
        Task<ApiResponse<UserDto>> GetProviderByIdAsync(string providerId);
        Task<ApiResponse<UserDto>> UpdateProviderAsync(string providerId, UpdateProviderDto updateProviderDto);
        Task<ApiResponse<MedicalHistoryDto>> GetMedicalHistoryAsync(string userId);
        Task<ApiResponse<MedicalHistoryDto>> UpdateMedicalHistoryAsync(string userId, UpdateMedicalHistoryDto medicalHistoryDto);
        Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetPaymentMethodsAsync(string userId);
        Task<ApiResponse<PaymentMethodDto>> AddPaymentMethodAsync(string userId, AddPaymentMethodDto addPaymentMethodDto);
        Task<ApiResponse<bool>> DeletePaymentMethodAsync(string userId, string paymentMethodId);
        Task<ApiResponse<bool>> SetDefaultPaymentMethodAsync(string userId, string paymentMethodId);
    }
} 