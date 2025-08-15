using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IProviderFeeService
{
    Task<ApiResponse<ProviderFeeDto>> CreateFeeAsync(CreateProviderFeeDto createDto);
    Task<ApiResponse<ProviderFeeDto>> GetFeeAsync(Guid id);
    Task<ApiResponse<ProviderFeeDto>> GetFeeByProviderAndCategoryAsync(int providerId, Guid categoryId);
    Task<ApiResponse<ProviderFeeDto>> UpdateFeeAsync(Guid id, UpdateProviderFeeDto updateDto);
    Task<ApiResponse<ProviderFeeDto>> ProposeFeeAsync(Guid id);
    Task<ApiResponse<ProviderFeeDto>> ReviewFeeAsync(Guid id, ReviewProviderFeeDto reviewDto);
    Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByProviderAsync(int providerId);
    Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByCategoryAsync(Guid categoryId);
    Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetAllFeesAsync(string? status = null, int page = 1, int pageSize = 50);
    Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetPendingFeesAsync();
    Task<ApiResponse<IEnumerable<ProviderFeeDto>>> GetFeesByStatusAsync(string status);
    Task<ApiResponse<bool>> DeleteFeeAsync(Guid id);
    Task<ApiResponse<FeeStatisticsDto>> GetFeeStatisticsAsync();
}

public interface ICategoryFeeRangeService
{
    Task<ApiResponse<CategoryFeeRangeDto>> CreateFeeRangeAsync(CreateCategoryFeeRangeDto createDto);
    Task<ApiResponse<CategoryFeeRangeDto>> GetFeeRangeAsync(Guid id);
    Task<ApiResponse<CategoryFeeRangeDto>> GetFeeRangeByCategoryAsync(Guid categoryId);
    Task<ApiResponse<CategoryFeeRangeDto>> UpdateFeeRangeAsync(Guid id, UpdateCategoryFeeRangeDto updateDto);
    Task<ApiResponse<IEnumerable<CategoryFeeRangeDto>>> GetAllFeeRangesAsync();
    Task<ApiResponse<bool>> DeleteFeeRangeAsync(Guid id);
    Task<ApiResponse<FeeRangeStatisticsDto>> GetFeeRangeStatisticsAsync();
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