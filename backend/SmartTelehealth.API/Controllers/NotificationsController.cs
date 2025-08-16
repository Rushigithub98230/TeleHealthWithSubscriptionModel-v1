using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonModel>> GetAllNotifications()
    {
        var response = await _notificationService.GetNotificationsAsync();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetNotification(Guid id)
    {
        var response = await _notificationService.GetNotificationAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
    {
        var response = await _notificationService.CreateNotificationAsync(createNotificationDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JsonModel>> UpdateNotification(Guid id, [FromBody] UpdateNotificationDto updateNotificationDto)
    {
        if (id != updateNotificationDto.Id)
            return BadRequest(new JsonModel { data = new object(), Message = "ID mismatch", StatusCode = 400 });
        var response = await _notificationService.UpdateNotificationAsync(id, updateNotificationDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<JsonModel>> DeleteNotification(Guid id)
    {
        var response = await _notificationService.DeleteNotificationAsync(id);
        return StatusCode(response.StatusCode, response);
    }
}

// DTO for creating test notifications
public class CreateTestNotificationDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
} 