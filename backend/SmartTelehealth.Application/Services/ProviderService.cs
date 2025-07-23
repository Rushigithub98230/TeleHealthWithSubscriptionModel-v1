using AutoMapper;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public ProviderService(IProviderRepository providerRepository, IMapper mapper, IAuditService auditService)
        {
            _providerRepository = providerRepository;
            _mapper = mapper;
            _auditService = auditService;
        }

        public async Task<ApiResponse<List<ProviderDto>>> GetAllProvidersAsync()
        {
            var providers = await _providerRepository.GetAllAsync();
            var dtos = _mapper.Map<List<ProviderDto>>(providers);
            return ApiResponse<List<ProviderDto>>.SuccessResponse(dtos);
        }

        public async Task<ApiResponse<ProviderDto>> GetProviderByIdAsync(Guid id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null)
                return ApiResponse<ProviderDto>.ErrorResponse("Provider not found", 404);
            var dto = _mapper.Map<ProviderDto>(provider);
            return ApiResponse<ProviderDto>.SuccessResponse(dto);
        }

        public async Task<ApiResponse<ProviderDto>> CreateProviderAsync(CreateProviderDto createProviderDto)
        {
            var provider = _mapper.Map<Provider>(createProviderDto);
            var created = await _providerRepository.CreateAsync(provider);
            var dto = _mapper.Map<ProviderDto>(created);
            await _auditService.LogUserActionAsync(dto.Id.ToString(), "ProviderCreated", "Provider", dto.Id.ToString(), $"Provider {dto.FullName} created");
            return ApiResponse<ProviderDto>.SuccessResponse(dto, "Provider created", 201);
        }

        public async Task<ApiResponse<ProviderDto>> UpdateProviderAsync(Guid id, UpdateProviderDto updateProviderDto)
        {
            var existing = await _providerRepository.GetByIdAsync(id);
            if (existing == null)
                return ApiResponse<ProviderDto>.ErrorResponse("Provider not found", 404);
            _mapper.Map(updateProviderDto, existing);
            var updated = await _providerRepository.UpdateAsync(existing);
            var dto = _mapper.Map<ProviderDto>(updated);
            await _auditService.LogUserActionAsync(dto.Id.ToString(), "ProviderUpdated", "Provider", dto.Id.ToString(), $"Provider {dto.FullName} updated");
            return ApiResponse<ProviderDto>.SuccessResponse(dto, "Provider updated");
        }

        public async Task<ApiResponse<bool>> DeleteProviderAsync(Guid id)
        {
            var result = await _providerRepository.DeleteAsync(id);
            if (!result)
                return ApiResponse<bool>.ErrorResponse("Provider not found", 404);
            await _auditService.LogUserActionAsync(id.ToString(), "ProviderDeleted", "Provider", id.ToString(), $"Provider {id} deleted");
            return ApiResponse<bool>.SuccessResponse(true, "Provider deleted");
        }
    }
} 