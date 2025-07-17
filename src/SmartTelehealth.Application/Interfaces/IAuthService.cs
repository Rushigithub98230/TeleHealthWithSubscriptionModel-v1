using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<ApiResponse<bool>> LogoutAsync();
    Task<ApiResponse<bool>> ValidateTokenAsync(string token);
} 