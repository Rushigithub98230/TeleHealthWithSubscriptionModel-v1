using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id);
    Task<IEnumerable<Provider>> GetAllAsync();
    Task<IEnumerable<Provider>> GetActiveProvidersAsync();
    Task<IEnumerable<Provider>> GetAvailableProvidersAsync();
    Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty);
    Task<Provider> CreateAsync(Provider provider);
    Task<Provider> UpdateAsync(Provider provider);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    Task<int> GetActiveProviderCountAsync();
    Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm);
    Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time);
} 