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
    /// Get health assessment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssessment(Guid id)
    {
        var response = await _healthAssessmentService.GetAssessmentByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get user's health assessments
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserAssessments(Guid userId)
    {
        var response = await _healthAssessmentService.GetUserAssessmentsAsync(userId);
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
    /// Update health assessment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateHealthAssessmentDto updateDto)
    {
        var response = await _healthAssessmentService.UpdateAssessmentAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete health assessment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssessment(Guid id)
    {
        var response = await _healthAssessmentService.DeleteAssessmentAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Review health assessment (Provider only)
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> ReviewAssessment(Guid id, [FromBody] ReviewAssessmentDto reviewDto)
    {
        var response = await _healthAssessmentService.ReviewAssessmentAsync(id, reviewDto.ProviderId, reviewDto.IsEligible, reviewDto.Notes);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Complete health assessment
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteAssessment(Guid id)
    {
        var response = await _healthAssessmentService.CompleteAssessmentAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Cancel health assessment
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelAssessment(Guid id, [FromBody] CancelAssessmentDto cancelDto)
    {
        var response = await _healthAssessmentService.CancelAssessmentAsync(id, cancelDto.Reason);
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
    [HttpGet("{id}/report")]
    public async Task<IActionResult> GenerateAssessmentReport(Guid id)
    {
        var response = await _healthAssessmentService.GenerateAssessmentReportAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Export assessment report
    /// </summary>
    [HttpGet("{id}/export")]
    public async Task<IActionResult> ExportAssessmentReport(Guid id, [FromQuery] string format = "pdf")
    {
        var response = await _healthAssessmentService.ExportAssessmentReportAsync(id, format);
        if (response.Success)
        {
            var fileName = $"assessment-report-{id}.{format}";
            return File(response.Data ?? Array.Empty<byte>(), GetContentType(format), fileName);
        }
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get assessment reports for user
    /// </summary>
    [HttpGet("reports/user/{userId}")]
    public async Task<IActionResult> GetAssessmentReports(Guid userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var response = await _healthAssessmentService.GetAssessmentReportsAsync(userId, startDate, endDate);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's pending assessments
    /// </summary>
    [HttpGet("provider/{providerId}/pending")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetProviderPendingAssessments(Guid providerId)
    {
        var response = await _healthAssessmentService.GetProviderPendingAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider's reviewed assessments
    /// </summary>
    [HttpGet("provider/{providerId}/reviewed")]
    [Authorize(Roles = "Provider,Admin")]
    public async Task<IActionResult> GetProviderReviewedAssessments(Guid providerId)
    {
        var response = await _healthAssessmentService.GetProviderReviewedAssessmentsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Assign assessment to provider (Admin only)
    /// </summary>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignAssessmentToProvider(Guid id, [FromBody] AssignAssessmentDto assignDto)
    {
        var response = await _healthAssessmentService.AssignAssessmentToProviderAsync(id, assignDto.ProviderId);
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
    public Guid ProviderId { get; set; }
    public bool IsEligible { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CancelAssessmentDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AssignAssessmentDto
{
    public Guid ProviderId { get; set; }
} 