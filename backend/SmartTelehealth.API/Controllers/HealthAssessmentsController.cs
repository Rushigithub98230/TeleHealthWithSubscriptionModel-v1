using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

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
    public async Task<IActionResult> CreateAssessment([FromBody] CreateHealthAssessmentDto createDto)
    {
        var response = await _healthAssessmentService.CreateAssessmentAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get user's health assessments
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAssessments(int userId)
    {
        var response = await _healthAssessmentService.GetUserAssessmentsAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's pending assessments
    /// </summary>
    [HttpGet("provider-assessment/{providerId}/pending")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetProviderPendingAssessments(int providerId)
    {
        var response = await _healthAssessmentService.GetProviderPendingAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's reviewed assessments
    /// </summary>
    [HttpGet("provider-assessment/{providerId}/reviewed")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetProviderReviewedAssessments(int providerId)
    {
        var response = await _healthAssessmentService.GetProviderReviewedAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get pending health assessments (Provider only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetPendingAssessments()
    {
        var response = await _healthAssessmentService.GetPendingAssessmentsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get health assessment by ID
    /// </summary>
    [HttpGet("assessment/{assessmentId}")]
    public async Task<IActionResult> GetAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.GetAssessmentByIdAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update health assessment
    /// </summary>
    [HttpPut("assessment/{assessmentId}")]
    public async Task<IActionResult> UpdateAssessment(Guid assessmentId, [FromBody] UpdateHealthAssessmentDto updateDto)
    {
        var response = await _healthAssessmentService.UpdateAssessmentAsync(assessmentId, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete health assessment
    /// </summary>
    [HttpDelete("assessment/{assessmentId}")]
    public async Task<IActionResult> DeleteAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.DeleteAssessmentAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Review health assessment (Provider only)
    /// </summary>
    [HttpPost("assessment/{assessmentId}/review")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> ReviewAssessment(Guid assessmentId, [FromBody] ReviewAssessmentDto reviewDto)
    {
        var response = await _healthAssessmentService.ReviewAssessmentAsync(assessmentId, reviewDto.ProviderId, reviewDto.IsEligible, reviewDto.Notes);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Complete health assessment
    /// </summary>
    [HttpPost("assessment/{assessmentId}/complete")]
    public async Task<IActionResult> CompleteAssessment(Guid assessmentId)
    {
        var response = await _healthAssessmentService.CompleteAssessmentAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Cancel health assessment
    /// </summary>
    [HttpPost("assessment/{assessmentId}/cancel")]
    public async Task<IActionResult> CancelAssessment(Guid assessmentId, [FromBody] CancelAssessmentDto cancelDto)
    {
        var response = await _healthAssessmentService.CancelAssessmentAsync(assessmentId, cancelDto.Reason);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create assessment template (Admin only)
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAssessmentTemplate([FromBody] CreateAssessmentTemplateDto createDto)
    {
        var response = await _healthAssessmentService.CreateAssessmentTemplateAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment template by ID
    /// </summary>
    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetAssessmentTemplate(Guid id)
    {
        var response = await _healthAssessmentService.GetAssessmentTemplateAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment templates by category
    /// </summary>
    [HttpGet("templates/category/{categoryId}")]
    public async Task<IActionResult> GetAssessmentTemplatesByCategory(Guid categoryId)
    {
        var response = await _healthAssessmentService.GetAssessmentTemplatesByCategoryAsync(categoryId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update assessment template (Admin only)
    /// </summary>
    [HttpPut("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAssessmentTemplate(Guid id, [FromBody] UpdateAssessmentTemplateDto updateDto)
    {
        var response = await _healthAssessmentService.UpdateAssessmentTemplateAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete assessment template (Admin only)
    /// </summary>
    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAssessmentTemplate(Guid id)
    {
        var response = await _healthAssessmentService.DeleteAssessmentTemplateAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Generate assessment report
    /// </summary>
    [HttpGet("assessment/{assessmentId}/report")]
    public async Task<IActionResult> GenerateAssessmentReport(Guid assessmentId)
    {
        var response = await _healthAssessmentService.GenerateAssessmentReportAsync(assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Export assessment report
    /// </summary>
    [HttpGet("assessment/{assessmentId}/export")]
    public async Task<IActionResult> ExportAssessmentReport(Guid assessmentId, [FromQuery] string format = "pdf")
    {
        var response = await _healthAssessmentService.ExportAssessmentReportAsync(assessmentId, format);
        if (response.Success)
        {
            var fileName = $"assessment-report-{assessmentId}.{format}";
            return File(response.Data ?? Array.Empty<byte>(), GetContentType(format), fileName);
        }
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment reports for user
    /// </summary>
    [HttpGet("reports/user/{userId}")]
    public async Task<IActionResult> GetAssessmentReports(int userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var response = await _healthAssessmentService.GetAssessmentReportsAsync(userId, startDate, endDate);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Assign assessment to provider (Admin only)
    /// </summary>
    [HttpPost("assessment/{assessmentId}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignAssessmentToProvider(Guid assessmentId, [FromBody] AssignAssessmentDto assignDto)
    {
        var response = await _healthAssessmentService.AssignAssessmentToProviderAsync(assessmentId, assignDto.ProviderId);
        return StatusCode(response.StatusCode, response);
    }

    private string GetContentType(string format)
    {
        return format.ToLower() switch
        {
            "pdf" => "application/pdf",
            "csv" => "text/csv",
            "json" => "application/json",
            _ => "application/octet-stream"
        };
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