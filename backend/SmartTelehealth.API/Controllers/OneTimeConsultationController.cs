using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OneTimeConsultationController : ControllerBase
{
    private readonly IConsultationService _consultationService;
    private readonly ILogger<OneTimeConsultationController> _logger;

    public OneTimeConsultationController(IConsultationService consultationService, ILogger<OneTimeConsultationController> logger)
    {
        _consultationService = consultationService;
        _logger = logger;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetMyOneTimeConsultations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
        {
            _logger.LogError("Invalid or missing UserId for current user");
            return Unauthorized();
        }
        var response = await _consultationService.GetUserOneTimeConsultationsAsync(parsedUserId);
        return StatusCode(response.StatusCode, response.Data);
    }
} 