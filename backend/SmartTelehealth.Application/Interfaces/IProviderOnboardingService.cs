using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderOnboardingService
{
    Task<JsonModel> CreateOnboardingAsync(CreateProviderOnboardingDto createDto);
    Task<JsonModel> GetOnboardingAsync(Guid id);
    Task<JsonModel> GetOnboardingByUserIdAsync(int userId);
    Task<JsonModel> UpdateOnboardingAsync(Guid id, UpdateProviderOnboardingDto updateDto);
    Task<JsonModel> SubmitOnboardingAsync(Guid id);
    Task<JsonModel> ReviewOnboardingAsync(Guid id, ReviewProviderOnboardingDto reviewDto);
    Task<JsonModel> GetAllOnboardingsAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<JsonModel> GetPendingOnboardingsAsync();
    Task<JsonModel> GetOnboardingsByStatusAsync(string status);
    Task<JsonModel> DeleteOnboardingAsync(Guid id);
    Task<JsonModel> GetOnboardingStatisticsAsync();
}

public class OnboardingStatisticsDto
{
    public int TotalOnboardings { get; set; }
    public int PendingOnboardings { get; set; }
    public int UnderReviewOnboardings { get; set; }
    public int ApprovedOnboardings { get; set; }
    public int RejectedOnboardings { get; set; }
    public int RequiresMoreInfoOnboardings { get; set; }
    public decimal ApprovalRate { get; set; }
    public int AverageProcessingTimeDays { get; set; }
} 