using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IProviderService
    {
        Task<ApiResponse<List<ProviderDto>>> GetAllProvidersAsync();
        Task<ApiResponse<ProviderDto>> GetProviderByIdAsync(Guid id);
        Task<ApiResponse<ProviderDto>> CreateProviderAsync(CreateProviderDto createProviderDto);
        Task<ApiResponse<ProviderDto>> UpdateProviderAsync(Guid id, UpdateProviderDto updateProviderDto);
        Task<ApiResponse<bool>> DeleteProviderAsync(Guid id);
    }
} 