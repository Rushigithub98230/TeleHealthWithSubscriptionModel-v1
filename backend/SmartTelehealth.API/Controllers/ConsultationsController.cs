using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonModel>> GetUserConsultations()
    {
        var userId = GetCurrentUserId();
        var response = await _consultationService.GetUserConsultationsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetConsultation(Guid id)
    {
        var response = await _consultationService.GetConsultationByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateConsultation(CreateConsultationDto createDto)
    {
        var response = await _consultationService.CreateConsultationAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JsonModel>> UpdateConsultation(Guid id, UpdateConsultationDto updateDto)
    {
        var response = await _consultationService.UpdateConsultationAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<JsonModel>> CancelConsultation(Guid id, [FromBody] string reason)
    {
        var response = await _consultationService.CancelConsultationAsync(id, reason);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<JsonModel>> StartConsultation(Guid id)
    {
        var response = await _consultationService.StartConsultationAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<JsonModel>> CompleteConsultation(Guid id, [FromBody] string notes)
    {
        var response = await _consultationService.CompleteConsultationAsync(id, notes);
        return StatusCode(response.StatusCode, response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 