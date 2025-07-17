using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var loginDto = new LoginDto
            {
                Email = loginRequestDto.Email,
                Password = loginRequestDto.Password
            };
            var response = await _authService.LoginAsync(loginDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var registerDto = new RegisterDto
            {
                Email = registerRequestDto.Email,
                Password = registerRequestDto.Password,
                ConfirmPassword = registerRequestDto.Password, // Use same password for confirm
                FirstName = registerRequestDto.FirstName,
                LastName = registerRequestDto.LastName,
                PhoneNumber = "", // Default value
                Gender = "", // Default value
                Address = "", // Default value
                City = "", // Default value
                State = "", // Default value
                ZipCode = "" // Default value
            };
            var response = await _authService.RegisterAsync(registerDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
        {
            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = refreshTokenRequestDto.RefreshToken
            };
            var response = await _authService.RefreshTokenAsync(refreshTokenDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401, ApiResponse<object>.ErrorResponse("Unauthorized", 401));
            }
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = changePasswordRequestDto.CurrentPassword,
                NewPassword = changePasswordRequestDto.NewPassword,
                ConfirmNewPassword = changePasswordRequestDto.NewPassword // Use same password for confirm
            };
            var response = await _authService.ChangePasswordAsync(changePasswordDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordRequestDto)
        {
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = forgotPasswordRequestDto.Email
            };
            var response = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequestDto)
        {
            var resetPasswordDto = new ResetPasswordDto
            {
                Token = resetPasswordRequestDto.Token,
                NewPassword = resetPasswordRequestDto.NewPassword,
                ConfirmNewPassword = resetPasswordRequestDto.NewPassword // Use same password for confirm
            };
            var response = await _authService.ResetPasswordAsync(resetPasswordDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var response = await _authService.LogoutAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // TODO: Implement get profile logic
            return StatusCode(200, ApiResponse<object>.SuccessResponse(new object(), "Profile endpoint - to be implemented"));
        }
    }
} 