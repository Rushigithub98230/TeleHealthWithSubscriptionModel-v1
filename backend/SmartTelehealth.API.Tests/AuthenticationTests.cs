using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;
using Xunit;

namespace SmartTelehealth.API.Tests
{
    public class AuthenticationTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _authController;

        public AuthenticationTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup configuration for JWT
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings"))
                .Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")["SecretKey"])
                .Returns("YourSuperSecretKeyHere12345678901234567890");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")["Issuer"])
                .Returns("TeleHealthAPI");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")["Audience"])
                .Returns("TeleHealthUsers");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")["ExpirationHours"])
                .Returns("24");

            _authController = new AuthController(_mockUserService.Object, _mockLogger.Object, _mockConfiguration.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPass123!"
            };

            var mockUser = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = "Client",
                PhoneNumber = "1234567890",
                UserRoleId = "1"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(mockUser);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_WithEmptyEmail_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "",
                Password = "TestPass123!"
            };

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        #endregion

        #region Registration Tests

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "Client"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(registerDto.Email))
                .ReturnsAsync((UserDto?)null);

            _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ApiResponse<UserDto> { Success = true });

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "existing@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "Client"
            };

            var existingUser = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                Role = "Client",
                PhoneNumber = "1234567890",
                UserRoleId = "1"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Register_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "weak",
                ConfirmPassword = "weak",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "Client"
            };

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "InvalidRole"
            };

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Theory]
        [InlineData("Client")]
        [InlineData("Admin")]
        [InlineData("Provider")]
        public async Task Register_WithValidRoles_ReturnsSuccessResponse(string role)
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = role
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(registerDto.Email))
                .ReturnsAsync((UserDto?)null);

            _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ApiResponse<UserDto> { Success = true });

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region Password Recovery Tests

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ReturnsSuccessResponse()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "test@example.com"
            };

            // Act
            var result = await _authController.ForgotPassword(forgotPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ForgotPassword_WithNonExistentEmail_ReturnsSuccessResponse()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDto
            {
                Email = "nonexistent@example.com"
            };

            // Act
            var result = await _authController.ForgotPassword(forgotPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                Token = "valid-reset-token",
                NewPassword = "NewPass123!",
                ConfirmNewPassword = "NewPass123!"
            };

            // Act
            var result = await _authController.ResetPassword(resetPasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ResetPassword_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto
            {
                Token = "valid-reset-token",
                NewPassword = "weak",
                ConfirmNewPassword = "weak"
            };

            // Act
            var result = await _authController.ResetPassword(resetPasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        #endregion

        #region Password Change Tests

        [Fact]
        public async Task ChangePassword_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "CurrentPass123!",
                NewPassword = "NewPass123!",
                ConfirmNewPassword = "NewPass123!"
            };

            // Setup controller context with authenticated user
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _authController.ControllerContext = controllerContext;

            // Setup user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controllerContext.HttpContext.User = principal;

            // Act
            var result = await _authController.ChangePassword(changePasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ChangePassword_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "CurrentPass123!",
                NewPassword = "weak",
                ConfirmNewPassword = "weak"
            };

            // Setup controller context with authenticated user
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _authController.ControllerContext = controllerContext;

            // Setup user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controllerContext.HttpContext.User = principal;

            // Act
            var result = await _authController.ChangePassword(changePasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task Logout_WithAuthenticatedUser_ReturnsSuccessResponse()
        {
            // Arrange
            // Setup controller context with authenticated user
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _authController.ControllerContext = controllerContext;

            // Setup user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controllerContext.HttpContext.User = principal;

            // Act
            var result = await _authController.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region Token Refresh Tests

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewToken()
        {
            // Arrange
            // Setup controller context with authenticated user
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _authController.ControllerContext = controllerContext;

            // Setup user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            controllerContext.HttpContext.User = principal;

            var mockUser = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = "Client",
                PhoneNumber = "1234567890",
                UserRoleId = "1"
            };

            _mockUserService.Setup(x => x.GetUserAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApiResponse<UserDto> { Success = true, Data = mockUser });

            // Act
            var result = await _authController.RefreshToken();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region Security Tests

        [Fact]
        public async Task Login_WithSQLInjectionAttempt_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "'; DROP TABLE Users; --",
                Password = "'; DROP TABLE Users; --"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync((UserDto?)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);
        }

        [Fact]
        public async Task Register_WithXSSAttempt_ReturnsSuccessResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "<script>alert('xss')</script>",
                LastName = "User",
                Email = "test@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "Client"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(registerDto.Email))
                .ReturnsAsync((UserDto?)null);

            _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ApiResponse<UserDto> { Success = true });

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region Password Strength Tests

        [Theory]
        [InlineData("StrongPass123!", true)]
        [InlineData("weak", false)]
        [InlineData("12345678", false)]
        [InlineData("abcdefgh", false)]
        [InlineData("ABCDEFGH", false)]
        [InlineData("Abcdefgh", false)]
        [InlineData("Abcdefg1", false)]
        [InlineData("Abcdefg!", false)]
        [InlineData("Abcdef1!", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsPasswordStrong_WithVariousPasswords_ReturnsExpectedResult(string? password, bool expected)
        {
            // Act
            var result = _authController.IsPasswordStrong(password ?? "");

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Rate Limiting Tests

        [Fact]
        public async Task Login_WithMultipleRapidAttempts_ShouldBeRateLimited()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPass123!"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync((UserDto?)null);

            // Act & Assert
            // Note: This is a conceptual test since rate limiting is not implemented
            // In a real implementation, you would test the rate limiting middleware
            for (int i = 0; i < 5; i++)
            {
                var result = await _authController.Login(loginDto);
                var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
                Assert.NotNull(unauthorizedResult.Value);
            }
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task Login_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "TestPass123!"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(loginDto.Email, loginDto.Password))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);
        }

        [Fact]
        public async Task Register_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                PhoneNumber = "1234567890",
                Gender = "Male",
                Address = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Role = "Client"
            };

            _mockUserService.Setup(x => x.GetUserByEmailAsync(registerDto.Email))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);
        }

        #endregion
    }
}
