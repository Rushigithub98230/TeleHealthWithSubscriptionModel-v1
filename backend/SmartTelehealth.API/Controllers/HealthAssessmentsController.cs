using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthAssessmentsController : ControllerBase
{
    private readonly IHealthAssessmentService _healthAssessmentService;

    public HealthAssessmentsController(IHealthAssessmentService healthAssessmentService)
    {
        _healthAssessmentService = healthAssessmentService;
    }

    /// <summary>
    /// Create a new health assessment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateAssessment([FromBody] CreateHealthAssessmentDto createDto)
    {
        var response = await _healthAssessmentService.CreateAssessmentAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get user's health assessments
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<JsonModel>> GetUserAssessments(int userId)
    {
        var response = await _healthAssessmentService.GetUserAssessmentsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's pending assessments
    /// </summary>
    [HttpGet("provider/{providerId}/pending")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<ActionResult<JsonModel>> GetProviderPendingAssessments(int providerId)
    {
        var response = await _healthAssessmentService.GetProviderPendingAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's reviewed assessments
    /// </summary>
    [HttpGet("provider/{providerId}/reviewed")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<ActionResult<JsonModel>> GetProviderReviewedAssessments(int providerId)
    {
        var response = await _healthAssessmentService.GetProviderReviewedAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get pending health assessments (Provider only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<ActionResult<JsonModel>> GetPendingAssessments()
    {
        var response = await _healthAssessmentService.GetPendingAssessmentsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get health assessment by ID
    /// </summary>
    [HttpGet("{assessmentId}")]
    public async Task<ActionResult<JsonModel>> GetAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.GetAssessmentByIdAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update health assessment
    /// </summary>
    [HttpPut("{assessmentId}")]
    public async Task<ActionResult<JsonModel>> UpdateAssessment(Guid assessmentId, [FromBody] UpdateHealthAssessmentDto updateDto)
    {
        var response = await _healthAssessmentService.UpdateAssessmentAsync(assessmentId, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete health assessment
    /// </summary>
    [HttpDelete("{assessmentId}")]
    public async Task<ActionResult<JsonModel>> DeleteAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.DeleteAssessmentAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Review health assessment (Provider only)
    /// </summary>
    [HttpPost("{assessmentId}/review")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<ActionResult<JsonModel>> ReviewAssessment(Guid assessmentId, [FromBody] ReviewAssessmentDto reviewDto)
    {
        var userId = GetCurrentUserId();
        var response = await _healthAssessmentService.ReviewAssessmentAsync(assessmentId, userId, reviewDto.IsEligible, reviewDto.Notes);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Complete health assessment
    /// </summary>
    [HttpPost("{assessmentId}/complete")]
    public async Task<ActionResult<JsonModel>> CompleteAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.CompleteAssessmentAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Cancel health assessment
    /// </summary>
    [HttpPost("{assessmentId}/cancel")]
    public async Task<ActionResult<JsonModel>> CancelAssessment(Guid assessmentId, [FromBody] CancelAssessmentDto cancelDto)
    {
        var response = await _healthAssessmentService.CancelAssessmentAsync(assessmentId, cancelDto.Reason);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create assessment template (Admin only)
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> CreateAssessmentTemplate([FromBody] CreateAssessmentTemplateDto createDto)
    {
        var response = await _healthAssessmentService.CreateAssessmentTemplateAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment template by ID
    /// </summary>
    [HttpGet("templates/{id}")]
    public async Task<ActionResult<JsonModel>> GetAssessmentTemplate(Guid id)
    {
        var response = await _healthAssessmentService.GetAssessmentTemplateAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment templates by category
    /// </summary>
    [HttpGet("templates/category/{categoryId}")]
    public async Task<ActionResult<JsonModel>> GetAssessmentTemplatesByCategory(Guid categoryId)
    {
        var response = await _healthAssessmentService.GetAssessmentTemplatesByCategoryAsync(categoryId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update assessment template (Admin only)
    /// </summary>
    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> UpdateAssessmentTemplate(Guid id, [FromBody] UpdateAssessmentTemplateDto updateDto)
    {
        var response = await _healthAssessmentService.UpdateAssessmentTemplateAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete assessment template (Admin only)
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> DeleteAssessmentTemplate(Guid id)
    {
        var response = await _healthAssessmentService.DeleteAssessmentTemplateAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Generate assessment report
    /// </summary>
    [HttpGet("{assessmentId}/report")]
    public async Task<ActionResult<JsonModel>> GenerateAssessmentReport(Guid assessmentId)
    {
        var response = await _healthAssessmentService.GenerateAssessmentReportAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Export assessment report
    /// </summary>
    [HttpGet("{assessmentId}/export")]
    public async Task<ActionResult<JsonModel>> ExportAssessmentReport(Guid assessmentId, [FromQuery] string format = "pdf")
    {
        var response = await _healthAssessmentService.ExportAssessmentReportAsync(assessmentId, format);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment reports for user
    /// </summary>
    [HttpGet("reports/{userId}")]
    public async Task<ActionResult<JsonModel>> GetAssessmentReports(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var response = await _healthAssessmentService.GetAssessmentReportsAsync(userId, startDate, endDate);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Assign assessment to provider (Admin only)
    /// </summary>
    [HttpPost("{assessmentId}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> AssignAssessmentToProvider(Guid assessmentId, [FromBody] AssignAssessmentDto assignDto)
    {
        var response = await _healthAssessmentService.AssignAssessmentToProviderAsync(assessmentId, assignDto.ProviderId);
        return StatusCode(response.StatusCode, response);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

// Supporting DTOs
public class ReviewAssessmentDto
{
    public int ProviderId { get; set; }
    public bool IsEligible { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CancelAssessmentDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AssignAssessmentDto
{
    public int ProviderId { get; set; }
} 