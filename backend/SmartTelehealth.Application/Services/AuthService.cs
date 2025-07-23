using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _auditService = auditService;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    await _auditService.LogSecurityEventAsync("Unknown", "LoginFailed", $"Login failed for email {loginDto.Email}");
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    await _auditService.LogSecurityEventAsync(user.Id.ToString(), "LoginFailed", $"Login failed for user {user.Email}");
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password");
                }

                var token = _jwtService.GenerateToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user);
                await _auditService.LogSecurityEventAsync(user.Id.ToString(), "LoginSuccess", $"User {user.Email} logged in successfully");

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Login successful"
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "LoginError", $"Login error: {ex.Message}");
                return ApiResponse<LoginResponseDto>.ErrorResponse("An error occurred during login");
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    await _auditService.LogSecurityEventAsync("Unknown", "RegisterFailed", $"Registration failed for email {registerDto.Email}: already exists");
                    return ApiResponse<LoginResponseDto>.ErrorResponse("User with this email already exists");
                }

                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    DateOfBirth = DateTime.UtcNow, // Default value
                    Gender = registerDto.Gender,
                    Address = registerDto.Address,
                    City = registerDto.City,
                    State = registerDto.State,
                    ZipCode = registerDto.ZipCode,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    await _auditService.LogSecurityEventAsync("Unknown", "RegisterFailed", $"Registration failed for email {registerDto.Email}: {string.Join(", ", errors)}");
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Registration failed", errors);
                }

                // Assign default role (Patient)
                await _userManager.AddToRoleAsync(user, "Patient");
                var token = _jwtService.GenerateToken(user);
                var refreshToken = await GenerateRefreshTokenAsync(user);
                await _auditService.LogSecurityEventAsync(user.Id.ToString(), "RegisterSuccess", $"User {user.Email} registered successfully");

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Registration successful"
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "RegisterError", $"Registration error: {ex.Message}");
                return ApiResponse<LoginResponseDto>.ErrorResponse("An error occurred during registration");
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenDto.RefreshToken);
                if (user == null)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid refresh token");
                }

                var token = _jwtService.GenerateToken(user);
                var newRefreshToken = await GenerateRefreshTokenAsync(user);

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    User = _mapper.Map<UserDto>(user),
                    Message = "Token refreshed successfully"
                };

                return ApiResponse<LoginResponseDto>.SuccessResponse(loginResponse);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "RefreshTokenError", $"Refresh token error: {ex.Message}");
                return ApiResponse<LoginResponseDto>.ErrorResponse("An error occurred while refreshing token");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                // This would need to get the current user from the context
                // For now, we'll use a placeholder
                var userId = "current-user-id"; // This should come from the current user context
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResponse("Failed to change password", errors);
                }

                await _auditService.LogSecurityEventAsync(userId, "PasswordChanged", "Password changed successfully");
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "ChangePasswordError", $"Change password error: {ex.Message}");
                return ApiResponse<bool>.ErrorResponse("An error occurred while changing password");
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    return ApiResponse<bool>.SuccessResponse(true);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // TODO: Send email with reset token
                await _auditService.LogSecurityEventAsync(user.Id.ToString(), "ForgotPassword", "Password reset requested");
                
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "ForgotPasswordError", $"Forgot password error: {ex.Message}");
                return ApiResponse<bool>.ErrorResponse("An error occurred while processing the request");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Note: This is a simplified implementation. In a real application,
                // you would validate the token and find the user by token, not by email
                var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Token); // This should be token validation
                if (user == null)
                    return ApiResponse<bool>.ErrorResponse("Invalid reset token");

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<bool>.ErrorResponse("Failed to reset password", errors);
                }

                await _auditService.LogSecurityEventAsync(user.Id.ToString(), "PasswordReset", "Password reset successfully");
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "ResetPasswordError", $"Reset password error: {ex.Message}");
                return ApiResponse<bool>.ErrorResponse("An error occurred while resetting password");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                // This would typically invalidate the refresh token
                // For now, we'll just return success
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "LogoutError", $"Logout error: {ex.Message}");
                return ApiResponse<bool>.ErrorResponse("An error occurred during logout");
            }
        }

        public async Task<ApiResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                // This would validate the JWT token
                // For now, we'll just return true
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                await _auditService.LogSecurityEventAsync("System", "ValidateTokenError", $"Token validation error: {ex.Message}");
                return ApiResponse<bool>.ErrorResponse("Invalid token");
            }
        }

        private async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);
            return refreshToken;
        }
    }
} 