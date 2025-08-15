using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IProviderService
    {
        Task<JsonModel> GetAllProvidersAsync();
        Task<JsonModel> GetProviderByIdAsync(int id);
        Task<JsonModel> CreateProviderAsync(CreateProviderDto createProviderDto);
        Task<JsonModel> UpdateProviderAsync(int id, UpdateProviderDto updateProviderDto);
        Task<JsonModel> DeleteProviderAsync(int id);
    }
} 