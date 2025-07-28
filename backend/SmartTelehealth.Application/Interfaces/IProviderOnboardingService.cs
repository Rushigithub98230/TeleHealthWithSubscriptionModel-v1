using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderOnboardingService
{
    Task<ApiResponse<ProviderOnboardingDto>> CreateOnboardingAsync(CreateProviderOnboardingDto createDto);
    Task<ApiResponse<ProviderOnboardingDto>> GetOnboardingAsync(Guid id);
    Task<ApiResponse<ProviderOnboardingDto>> GetOnboardingByUserIdAsync(Guid userId);
    Task<ApiResponse<ProviderOnboardingDto>> UpdateOnboardingAsync(Guid id, UpdateProviderOnboardingDto updateDto);
    Task<ApiResponse<ProviderOnboardingDto>> SubmitOnboardingAsync(Guid id);
    Task<ApiResponse<ProviderOnboardingDto>> ReviewOnboardingAsync(Guid id, ReviewProviderOnboardingDto reviewDto);
    Task<ApiResponse<IEnumerable<ProviderOnboardingDto>>> GetAllOnboardingsAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<ApiResponse<IEnumerable<ProviderOnboardingDto>>> GetPendingOnboardingsAsync();
    Task<ApiResponse<IEnumerable<ProviderOnboardingDto>>> GetOnboardingsByStatusAsync(string status);
    Task<ApiResponse<bool>> DeleteOnboardingAsync(Guid id);
    Task<ApiResponse<OnboardingStatisticsDto>> GetOnboardingStatisticsAsync();
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