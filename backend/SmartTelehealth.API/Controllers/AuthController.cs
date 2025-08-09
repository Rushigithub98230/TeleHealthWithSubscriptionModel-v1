using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid login data" });
            }

            var user = await _userService.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid email or password" });
            }

            var token = GenerateJwtToken(user);
            
            return Ok(new
            {
                success = true,
                data = new
                {
                    token = token,
                    user = new
                    {
                        id = user.Id.ToString(),
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        role = user.Role,
                        phoneNumber = user.PhoneNumber
                    }
                },
                message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return StatusCode(500, new { success = false, message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid registration data" });
            }

            // Validate role - only allow Client, Admin, Provider
            var validRoles = new[] { "Client", "Admin", "Provider" };
            if (!validRoles.Contains(registerDto.Role, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = $"Invalid role. Allowed roles: {string.Join(", ", validRoles)}" });
            }

            // Check if user already exists
            var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "User with this email already exists" });
            }

            // Validate password strength
            if (!IsPasswordStrong(registerDto.Password))
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character" });
            }

            // Determine UserType based on role
            string userType = registerDto.Role?.ToLower() switch
            {
                "admin" => "Admin",
                "provider" => "Provider", 
                _ => "Client" // Default to Client for regular users
            };

            var userDto = new CreateUserDto
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Gender = registerDto.Gender,
                Address = registerDto.Address,
                City = registerDto.City,
                State = registerDto.State,
                ZipCode = registerDto.ZipCode,
                Password = registerDto.Password,
                UserType = userType
            };

            var result = await _userService.CreateUserAsync(userDto);
            
            if (result.Success)
            {
                _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
                return Ok(new { success = true, message = "User registered successfully" });
            }
            else
            {
                return BadRequest(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", registerDto.Email);
            return StatusCode(500, new { success = false, message = "An error occurred during registration" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid email address" });
            }

            var user = await _userService.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                return Ok(new { success = true, message = "If the email exists, a password reset link has been sent" });
            }

            // Generate password reset token
            var resetToken = Guid.NewGuid().ToString();
            var resetTokenExpiry = DateTime.UtcNow.AddHours(1);

            // TODO: Store reset token in database and send email
            // For now, just return success
            _logger.LogInformation("Password reset requested for user: {Email}", forgotPasswordDto.Email);

            return Ok(new { success = true, message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for user: {Email}", forgotPasswordDto.Email);
            return StatusCode(500, new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid reset password data" });
            }

            if (!IsPasswordStrong(resetPasswordDto.NewPassword))
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character" });
            }

            // TODO: Validate reset token and update password
            // For now, just return success
            _logger.LogInformation("Password reset completed for token: {Token}", resetPasswordDto.Token);

            return Ok(new { success = true, message = "Password has been reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { success = false, message = "An error occurred while resetting password" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid change password data" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            if (!IsPasswordStrong(changePasswordDto.NewPassword))
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character" });
            }

            // TODO: Implement password change logic
            _logger.LogInformation("Password change requested for user: {UserId}", userId);

            return Ok(new { success = true, message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, new { success = false, message = "An error occurred while changing password" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // TODO: Implement token blacklisting
                _logger.LogInformation("User logged out: {UserId}", userId);
            }

            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { success = false, message = "An error occurred during logout" });
        }
    }

    public bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        var hasUpperCase = password.Any(char.IsUpper);
        var hasLowerCase = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Invalid token" });
            }

            var user = await _userService.GetUserAsync(userId);
            if (user == null || !user.Success)
            {
                return Unauthorized(new { success = false, message = "User not found" });
            }

            var token = GenerateJwtToken(user.Data);
            
            return Ok(new
            {
                success = true,
                data = new
                {
                    token = token,
                    user = new
                    {
                        id = user.Data.Id.ToString(),
                        email = user.Data.Email,
                        firstName = user.Data.FirstName,
                        lastName = user.Data.LastName,
                        role = user.Data.Role,
                        phoneNumber = user.Data.PhoneNumber
                    }
                },
                message = "Token refreshed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { success = false, message = "An error occurred while refreshing token" });
        }
    }

    private string GenerateJwtToken(UserDto user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "YourSuperSecretKeyHere12345678901234567890");
        var issuer = jwtSettings["Issuer"] ?? "TeleHealthAPI";
        var audience = jwtSettings["Audience"] ?? "TeleHealthUsers";
        var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "24");

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
                new Claim("RoleId", user.UserRoleId)
            }),
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
} 