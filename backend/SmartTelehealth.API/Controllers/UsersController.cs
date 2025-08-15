using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<JsonModel> GetCurrentUserProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }

                var result = await _userService.GetUserByIdAsync(userIdInt);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user profile");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<JsonModel> UpdateProfile([FromBody] UpdateUserDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }

                updateDto.Id = userId;
                var result = await _userService.UpdateUserAsync(userIdInt, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get user by ID (admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JsonModel> GetUserById(string id)
        {
            try
            {
                if (!int.TryParse(id, out int userId))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }

                var result = await _userService.GetUserByIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all users with pagination (admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JsonModel>> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? userType = null)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new JsonModel>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Create new user (admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JsonModel> CreateUser([FromBody] CreateUserDto createDto)
        {
            try
            {
                var result = await _userService.CreateUserAsync(createDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update user (admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JsonModel> UpdateUser(string id, [FromBody] UpdateUserDto updateDto)
        {
            try
            {
                if (!int.TryParse(id, out int userId))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }

                updateDto.Id = id;
                var result = await _userService.UpdateUserAsync(userId, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", id);
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Delete user (admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JsonModel> DeleteUser(string id)
        {
            try
            {
                if (!int.TryParse(id, out int userId))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }

                var result = await _userService.DeleteUserAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get all providers
        /// </summary>
        [HttpGet("providers")]
        public async Task<ActionResult<JsonModel>> GetAllProviders()
        {
            try
            {
                var result = await _userService.GetAllProvidersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all providers");
                return StatusCode(500, new JsonModel>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get provider by ID
        /// </summary>
        [HttpGet("providers/{id}")]
        public async Task<ActionResult<JsonModel> GetProviderById(string id)
        {
            try
            {
                if (!int.TryParse(id, out int providerId))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid provider ID format"
                    });
                }
                var result = await _userService.GetProviderByIdAsync(providerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider by ID: {ProviderId}", id);
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update provider profile
        /// </summary>
        [HttpPut("providers/{id}")]
        [Authorize(Roles = "Provider")]
        public async Task<ActionResult<JsonModel> UpdateProviderProfile(string id, [FromBody] UpdateProviderDto updateDto)
        {
            try
            {
                if (!int.TryParse(id, out int providerId))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid provider ID format"
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != id)
                {
                    return Forbid();
                }

                if (!int.TryParse(id, out int idInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid ID format"
                    });
                }
                updateDto.Id = idInt;
                var result = await _userService.UpdateProviderAsync(providerId, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider profile: {ProviderId}", id);
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get user medical history
        /// </summary>
        [HttpGet("medical-history")]
        [Authorize]
        public async Task<ActionResult<JsonModel> GetMedicalHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }
                var result = await _userService.GetMedicalHistoryAsync(userIdInt);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medical history");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Update user medical history
        /// </summary>
        [HttpPut("medical-history")]
        [Authorize]
        public async Task<ActionResult<JsonModel> UpdateMedicalHistory([FromBody] UpdateMedicalHistoryDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }
                updateDto.UserId = userIdInt;
                var result = await _userService.UpdateMedicalHistoryAsync(userIdInt, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical history");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get user payment methods
        /// </summary>
        [HttpGet("payment-methods")]
        [Authorize]
        public async Task<ActionResult<JsonModel>> GetPaymentMethods()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new JsonModel>
            {
                Success = false,
                Message = "Invalid user ID format"
            });
        }
        var result = await _userService.GetPaymentMethodsAsync(userIdInt);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods");
                return StatusCode(500, new JsonModel>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Add payment method
        /// </summary>
        [HttpPost("payment-methods")]
        [Authorize]
        public async Task<ActionResult<JsonModel> AddPaymentMethod([FromBody] CreatePaymentMethodDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }
                var addPaymentMethodDto = new SmartTelehealth.Application.DTOs.AddPaymentMethodDto
                {
                    PaymentMethodId = createDto.Token
                };
                var result = await _userService.AddPaymentMethodAsync(userIdInt, addPaymentMethodDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Delete payment method
        /// </summary>
        [HttpDelete("payment-methods/{id}")]
        [Authorize]
        public async Task<ActionResult<JsonModel> DeletePaymentMethod(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }
                var result = await _userService.DeletePaymentMethodAsync(userIdInt, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment method");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Set default payment method
        /// </summary>
        [HttpPut("payment-methods/{id}/default")]
        [Authorize]
        public async Task<ActionResult<JsonModel> SetDefaultPaymentMethod(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new JsonModel
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest(new JsonModel
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    });
                }
                var result = await _userService.SetDefaultPaymentMethodAsync(userIdInt, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default payment method");
                return StatusCode(500, new JsonModel
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }
} 