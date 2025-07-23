using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IPrescriptionRepository
{
    Task<Prescription> GetByIdAsync(Guid id);
    Task<IEnumerable<Prescription>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Prescription>> GetByProviderIdAsync(Guid providerId);
    Task<IEnumerable<Prescription>> GetByStatusAsync(string status);
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription> UpdateAsync(Prescription prescription);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Prescription>> GetOverduePrescriptionsAsync();
    Task<IEnumerable<Prescription>> GetRefillRequestsAsync(Guid userId);
    Task<int> GetPrescriptionCountAsync(Guid userId);
    Task<decimal> GetPrescriptionTotalAsync(Guid userId);
} 