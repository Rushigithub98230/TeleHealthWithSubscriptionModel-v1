using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

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
    public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetUserConsultations()
    {
        var userId = GetCurrentUserId();
        var response = await _consultationService.GetUserOneTimeConsultationsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultationDto>> GetConsultation(Guid id)
    {
        var response = await _consultationService.GetConsultationByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> CreateConsultation(CreateConsultationDto createDto)
    {
        var response = await _consultationService.CreateConsultationAsync(createDto);
        if (response.Success && response.Data != null)
        {
            return CreatedAtAction(nameof(GetConsultation), new { id = response.Data.Id }, response);
        }
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ConsultationDto>> UpdateConsultation(Guid id, UpdateConsultationDto updateDto)
    {
        var response = await _consultationService.UpdateConsultationAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelConsultation(Guid id, [FromBody] string reason)
    {
        var response = await _consultationService.CancelConsultationAsync(id, reason);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPost("{id}/start")]
    public async Task<ActionResult> StartConsultation(Guid id)
    {
        var response = await _consultationService.StartConsultationAsync(id);
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteConsultation(Guid id, [FromBody] string notes)
    {
        var response = await _consultationService.CompleteConsultationAsync(id, notes);
        return StatusCode(response.StatusCode, response);
    }
    
    private int GetCurrentUserId()
    {
        // This should be implemented based on your JWT token claims
        // For now, returning a default value
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return 0; // Default value if parsing fails
    }
} 