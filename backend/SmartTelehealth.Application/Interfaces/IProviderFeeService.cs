using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderFeeService
{
    Task<JsonModel> CreateFeeAsync(CreateProviderFeeDto createDto);
    Task<JsonModel> GetFeeAsync(Guid id);
    Task<JsonModel> GetFeeByProviderAndCategoryAsync(int providerId, Guid categoryId);
    Task<JsonModel> UpdateFeeAsync(Guid id, UpdateProviderFeeDto updateDto);
    Task<JsonModel> ProposeFeeAsync(Guid id);
    Task<JsonModel> ReviewFeeAsync(Guid id, ReviewProviderFeeDto reviewDto);
    Task<JsonModel> GetFeesByProviderAsync(int providerId);
    Task<JsonModel> GetFeesByCategoryAsync(Guid categoryId);
    Task<JsonModel> GetAllFeesAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<JsonModel> GetPendingFeesAsync();
    Task<JsonModel> GetFeesByStatusAsync(string status);
    Task<JsonModel> DeleteFeeAsync(Guid id);
    Task<JsonModel> GetFeeStatisticsAsync();
}

public interface ICategoryFeeRangeService
{
    Task<JsonModel> CreateFeeRangeAsync(CreateCategoryFeeRangeDto createDto);
    Task<JsonModel> GetFeeRangeAsync(Guid id);
    Task<JsonModel> GetFeeRangeByCategoryAsync(Guid categoryId);
    Task<JsonModel> UpdateFeeRangeAsync(Guid id, UpdateCategoryFeeRangeDto updateDto);
    Task<JsonModel> GetAllFeeRangesAsync();
    Task<JsonModel> DeleteFeeRangeAsync(Guid id);
    Task<JsonModel> GetFeeRangeStatisticsAsync();
}

public class FeeStatisticsDto
{
    public int TotalFees { get; set; }
    public int PendingFees { get; set; }
    public int UnderReviewFees { get; set; }
    public int ApprovedFees { get; set; }
    public int RejectedFees { get; set; }
    public decimal AverageProposedFee { get; set; }
    public decimal AverageApprovedFee { get; set; }
    public decimal ApprovalRate { get; set; }
}

public class FeeRangeStatisticsDto
{
    public int TotalFeeRanges { get; set; }
    public int ActiveFeeRanges { get; set; }
    public decimal AverageMinimumFee { get; set; }
    public decimal AverageMaximumFee { get; set; }
    public decimal AveragePlatformCommission { get; set; }
} 