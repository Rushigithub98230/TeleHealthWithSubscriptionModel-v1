using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHealthAssessmentService
{
    // CRUD Operations
    Task<ApiResponse<HealthAssessmentDto>> CreateAssessmentAsync(CreateHealthAssessmentDto createDto);
    Task<ApiResponse<HealthAssessmentDto>> GetAssessmentByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<HealthAssessmentDto>>> GetUserAssessmentsAsync(Guid userId);
    Task<ApiResponse<IEnumerable<HealthAssessmentDto>>> GetPendingAssessmentsAsync();
    Task<ApiResponse<HealthAssessmentDto>> UpdateAssessmentAsync(Guid id, UpdateHealthAssessmentDto updateDto);
    Task<ApiResponse<bool>> DeleteAssessmentAsync(Guid id);
    
    // Assessment Management
    Task<ApiResponse<bool>> ReviewAssessmentAsync(Guid id, Guid providerId, bool isEligible, string notes);
    Task<ApiResponse<bool>> CompleteAssessmentAsync(Guid id);
    Task<ApiResponse<bool>> CancelAssessmentAsync(Guid id, string reason);
    
    // Assessment Templates
    Task<ApiResponse<AssessmentTemplateDto>> CreateAssessmentTemplateAsync(CreateAssessmentTemplateDto createDto);
    Task<ApiResponse<AssessmentTemplateDto>> GetAssessmentTemplateAsync(Guid id);
    Task<ApiResponse<IEnumerable<AssessmentTemplateDto>>> GetAssessmentTemplatesByCategoryAsync(Guid categoryId);
    Task<ApiResponse<AssessmentTemplateDto>> UpdateAssessmentTemplateAsync(Guid id, UpdateAssessmentTemplateDto updateDto);
    Task<ApiResponse<bool>> DeleteAssessmentTemplateAsync(Guid id);
    
    // Assessment Reports
    Task<ApiResponse<AssessmentReportDto>> GenerateAssessmentReportAsync(Guid assessmentId);
    Task<ApiResponse<byte[]>> ExportAssessmentReportAsync(Guid assessmentId, string format = "pdf");
    Task<ApiResponse<IEnumerable<AssessmentReportDto>>> GetAssessmentReportsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Provider Workflow
    Task<ApiResponse<IEnumerable<HealthAssessmentDto>>> GetProviderPendingAssessmentsAsync(Guid providerId);
    Task<ApiResponse<IEnumerable<HealthAssessmentDto>>> GetProviderReviewedAssessmentsAsync(Guid providerId);
    Task<ApiResponse<bool>> AssignAssessmentToProviderAsync(Guid assessmentId, Guid providerId);
} 