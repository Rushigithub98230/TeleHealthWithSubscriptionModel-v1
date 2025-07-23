using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IHealthAssessmentRepository
{
    Task<HealthAssessment> GetByIdAsync(Guid id);
    Task<IEnumerable<HealthAssessment>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<HealthAssessment>> GetByCategoryIdAsync(Guid categoryId);
    Task<IEnumerable<HealthAssessment>> GetPendingAssessmentsAsync();
    Task<HealthAssessment> CreateAsync(HealthAssessment assessment);
    Task<HealthAssessment> UpdateAsync(HealthAssessment assessment);
    Task<bool> DeleteAsync(Guid id);
} 