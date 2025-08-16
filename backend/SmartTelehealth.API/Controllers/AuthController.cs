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
    public async Task<ActionResult<JsonModel>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid login data", StatusCode = 400 });
            }

            var user = await _userService.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            
            if (user == null)
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "Invalid email or password", StatusCode = 401 });
            }

            var token = GenerateJwtToken(user);
            
            return Ok(new JsonModel
            {
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
                Message = "Login successful",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred during login", StatusCode = 500 });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<JsonModel>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid registration data", StatusCode = 400 });
            }

            // Validate role - only allow Client, Admin, Provider
            var validRoles = new[] { "Client", "Admin", "Provider" };
            if (!validRoles.Contains(registerDto.Role, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new JsonModel { data = new object(), Message = $"Invalid role. Allowed roles: {string.Join(", ", validRoles)}", StatusCode = 400 });
            }

            // Check if user already exists
            var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "User with this email already exists", StatusCode = 400 });
            }

            // Validate password strength
            if (!IsPasswordStrong(registerDto.Password))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character", StatusCode = 400 });
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
            
            if (result.StatusCode == 200)
            {
                _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
                return Ok(new JsonModel { data = new object(), Message = "User registered successfully", StatusCode = 200 });
            }
            else
            {
                return BadRequest(new JsonModel { data = new object(), Message = result.Message, StatusCode = 400 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", registerDto.Email);
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred during registration", StatusCode = 500 });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<JsonModel>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid email address", StatusCode = 400 });
            }

            var user = await _userService.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                return Ok(new JsonModel { data = new object(), Message = "If the email exists, a password reset link has been sent", StatusCode = 200 });
            }

            // Generate password reset token
            var resetToken = Guid.NewGuid().ToString();
            var resetTokenExpiry = DateTime.UtcNow.AddHours(1);

            // TODO: Store reset token in database and send email
            // For now, just return success
            _logger.LogInformation("Password reset requested for user: {Email}", forgotPasswordDto.Email);

            return Ok(new JsonModel { data = new object(), Message = "If the email exists, a password reset link has been sent", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for user: {Email}", forgotPasswordDto.Email);
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred while processing the request", StatusCode = 500 });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<JsonModel>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid reset password data", StatusCode = 400 });
            }

            if (!IsPasswordStrong(resetPasswordDto.NewPassword))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character", StatusCode = 400 });
            }

            // TODO: Validate reset token and update password
            // For now, just return success
            _logger.LogInformation("Password reset completed for token: {Token}", resetPasswordDto.Token);

            return Ok(new JsonModel { data = new object(), Message = "Password has been reset successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for token: {Token}", resetPasswordDto.Token);
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred while resetting the password", StatusCode = 500 });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<JsonModel>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid change password data", StatusCode = 400 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "Invalid token", StatusCode = 401 });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 401 });
            }

            if (!IsPasswordStrong(changePasswordDto.NewPassword))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character", StatusCode = 400 });
            }

            // TODO: Implement password change logic
            // For now, just return success
            _logger.LogInformation("Password change requested for user: {UserId}", userIdInt);

            return Ok(new JsonModel { data = new object(), Message = "Password changed successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred while changing the password", StatusCode = 500 });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<JsonModel>> Logout()
    {
        try
        {
            // TODO: Implement logout logic (e.g., blacklist token, clear session)
            // For now, just return success
            _logger.LogInformation("User logged out: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Ok(new JsonModel { data = new object(), Message = "Logged out successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred during logout", StatusCode = 500 });
        }
    }

    private bool IsPasswordStrong(string password)
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
    public async Task<ActionResult<JsonModel>> RefreshToken()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "Invalid token", StatusCode = 401 });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 401 });
            }

            var user = await _userService.GetUserAsync(userIdInt);
            if (user == null || user.StatusCode != 200)
            {
                return Unauthorized(new JsonModel { data = new object(), Message = "User not found", StatusCode = 401 });
            }

            var token = GenerateJwtToken((UserDto)user.data);
            
            return Ok(new JsonModel
            {
                data = new
                {
                    token = token,
                    user = new
                    {
                        id = ((UserDto)user.data).Id.ToString(),
                        email = ((UserDto)user.data).Email,
                        firstName = ((UserDto)user.data).FirstName,
                        lastName = ((UserDto)user.data).LastName,
                        role = ((UserDto)user.data).Role,
                        phoneNumber = ((UserDto)user.data).PhoneNumber
                    }
                },
                Message = "Token refreshed successfully",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred while refreshing token", StatusCode = 500 });
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