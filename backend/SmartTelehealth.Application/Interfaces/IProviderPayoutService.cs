using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderPayoutService
{
    Task<ApiResponse<ProviderPayoutDto>> GetPayoutAsync(Guid id);
    Task<ApiResponse<ProviderPayoutDto>> ProcessPayoutAsync(Guid id, ProcessPayoutDto processDto);
    Task<ApiResponse<IEnumerable<ProviderPayoutDto>>> GetPayoutsByProviderAsync(int providerId);
    Task<ApiResponse<IEnumerable<ProviderPayoutDto>>> GetPayoutsByPeriodAsync(Guid periodId);
    Task<ApiResponse<IEnumerable<ProviderPayoutDto>>> GetAllPayoutsAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<ApiResponse<IEnumerable<ProviderPayoutDto>>> GetPendingPayoutsAsync();
    Task<ApiResponse<IEnumerable<ProviderPayoutDto>>> GetPayoutsByStatusAsync(string status);
    Task<ApiResponse<ProviderEarningsDto>> GetProviderEarningsAsync(int providerId);
    Task<ApiResponse<PayoutStatisticsDto>> GetPayoutStatisticsAsync();
    Task<ApiResponse<bool>> GeneratePayoutsForPeriodAsync(Guid periodId);
    Task<ApiResponse<bool>> ProcessAllPendingPayoutsAsync();
}

public interface IPayoutPeriodService
{
    Task<ApiResponse<PayoutPeriodDto>> CreatePeriodAsync(CreatePayoutPeriodDto createDto);
    Task<ApiResponse<PayoutPeriodDto>> GetPeriodAsync(Guid id);
    Task<ApiResponse<PayoutPeriodDto>> UpdatePeriodAsync(Guid id, CreatePayoutPeriodDto updateDto);
    Task<ApiResponse<IEnumerable<PayoutPeriodDto>>> GetAllPeriodsAsync();
    Task<ApiResponse<IEnumerable<PayoutPeriodDto>>> GetActivePeriodsAsync();
    Task<ApiResponse<bool>> DeletePeriodAsync(Guid id);
    Task<ApiResponse<bool>> ProcessPeriodAsync(Guid id);
    Task<ApiResponse<PayoutPeriodStatisticsDto>> GetPeriodStatisticsAsync();
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