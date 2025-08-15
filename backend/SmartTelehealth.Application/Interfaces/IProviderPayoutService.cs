using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderPayoutService
{
    Task<JsonModel> GetPayoutAsync(Guid id);
    Task<JsonModel> ProcessPayoutAsync(Guid id, ProcessPayoutDto processDto);
    Task<JsonModel> GetPayoutsByProviderAsync(int providerId);
    Task<JsonModel> GetPayoutsByPeriodAsync(Guid periodId);
    Task<JsonModel> GetAllPayoutsAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<JsonModel> GetPendingPayoutsAsync();
    Task<JsonModel> GetPayoutsByStatusAsync(string status);
    Task<JsonModel> GetProviderEarningsAsync(int providerId);
    Task<JsonModel> GetPayoutStatisticsAsync();
    Task<JsonModel> GeneratePayoutsForPeriodAsync(Guid periodId);
    Task<JsonModel> ProcessAllPendingPayoutsAsync();
}

public interface IPayoutPeriodService
{
    Task<JsonModel> CreatePeriodAsync(CreatePayoutPeriodDto createDto);
    Task<JsonModel> GetPeriodAsync(Guid id);
    Task<JsonModel> UpdatePeriodAsync(Guid id, CreatePayoutPeriodDto updateDto);
    Task<JsonModel> GetAllPeriodsAsync();
    Task<JsonModel> GetActivePeriodsAsync();
    Task<JsonModel> DeletePeriodAsync(Guid id);
    Task<JsonModel> ProcessPeriodAsync(Guid id);
    Task<JsonModel> GetPeriodStatisticsAsync();
}

public class PayoutStatisticsDto
{
    public int TotalPayouts { get; set; }
    public int PendingPayouts { get; set; }
    public int ProcessedPayouts { get; set; }
    public int OnHoldPayouts { get; set; }
    public decimal TotalPayoutAmount { get; set; }
    public decimal PendingPayoutAmount { get; set; }
    public decimal ProcessedPayoutAmount { get; set; }
    public decimal AveragePayoutAmount { get; set; }
    public int TotalProviders { get; set; }
    public int ProvidersWithPendingPayouts { get; set; }
}

public class PayoutPeriodStatisticsDto
{
    public int TotalPeriods { get; set; }
    public int OpenPeriods { get; set; }
    public int ProcessingPeriods { get; set; }
    public int CompletedPeriods { get; set; }
    public decimal TotalAmountProcessed { get; set; }
    public int TotalPayoutsProcessed { get; set; }
    public decimal AveragePeriodAmount { get; set; }
} 