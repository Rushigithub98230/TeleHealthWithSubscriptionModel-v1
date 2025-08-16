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

    [HttpGet("my-consultations")]
    public async Task<ActionResult<JsonModel>> GetMyOneTimeConsultations()
    {
        var userId = GetCurrentUserId();
        var response = await _consultationService.GetUserOneTimeConsultationsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 