using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderOnboardingRepository
{
    Task<ProviderOnboarding?> GetByIdAsync(Guid id);
    Task<ProviderOnboarding?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<ProviderOnboarding>> GetAllAsync();
    Task<IEnumerable<ProviderOnboarding>> GetByStatusAsync(string status);
    Task<IEnumerable<ProviderOnboarding>> GetPendingAsync();
    Task<IEnumerable<ProviderOnboarding>> GetByStatusWithPaginationAsync(string status, int page, int pageSize);
    Task<ProviderOnboarding> AddAsync(ProviderOnboarding onboarding);
    Task<ProviderOnboarding> UpdateAsync(ProviderOnboarding onboarding);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetTotalCountAsync();
} 