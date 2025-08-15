using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHealthAssessmentService
{
    // CRUD Operations
    Task<JsonModel> CreateAssessmentAsync(CreateHealthAssessmentDto createDto);
    Task<JsonModel> GetAssessmentByIdAsync(Guid id);
    Task<JsonModel> GetUserAssessmentsAsync(int userId);
    Task<JsonModel> GetPendingAssessmentsAsync();
    Task<JsonModel> UpdateAssessmentAsync(Guid id, UpdateHealthAssessmentDto updateDto);
    Task<JsonModel> DeleteAssessmentAsync(Guid id);
    
    // Assessment Management
            Task<JsonModel> ReviewAssessmentAsync(Guid id, int providerId, bool isEligible, string notes);
    Task<JsonModel> CompleteAssessmentAsync(Guid id);
    Task<JsonModel> CancelAssessmentAsync(Guid id, string reason);
    
    // Assessment Templates
    Task<JsonModel> CreateAssessmentTemplateAsync(CreateAssessmentTemplateDto createDto);
    Task<JsonModel> GetAssessmentTemplateAsync(Guid id);
    Task<JsonModel> GetAssessmentTemplatesByCategoryAsync(Guid categoryId);
    Task<JsonModel> UpdateAssessmentTemplateAsync(Guid id, UpdateAssessmentTemplateDto updateDto);
    Task<JsonModel> DeleteAssessmentTemplateAsync(Guid id);
    
    // Assessment Reports
    Task<JsonModel> GenerateAssessmentReportAsync(Guid assessmentId);
    Task<JsonModel> ExportAssessmentReportAsync(Guid assessmentId, string format = "pdf");
    Task<JsonModel> GetAssessmentReportsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Provider Workflow
    Task<JsonModel> GetProviderPendingAssessmentsAsync(int providerId);
    Task<JsonModel> GetProviderReviewedAssessmentsAsync(int providerId);
            Task<JsonModel> AssignAssessmentToProviderAsync(Guid assessmentId, int providerId);
} 