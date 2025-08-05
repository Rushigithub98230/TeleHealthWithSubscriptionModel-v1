using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IQuestionnaireService
    {
        // Template CRUD
        Task<ApiResponse<QuestionnaireTemplateDto>> GetTemplateByIdAsync(Guid id);
        Task<ApiResponse<List<QuestionnaireTemplateDto>>> GetTemplatesByCategoryAsync(Guid categoryId);
        Task<IEnumerable<QuestionnaireTemplateDto>> GetAllTemplatesAsync();
        Task<ApiResponse<Guid>> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto, List<IFormFile> files);
        Task<ApiResponse<object>> UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto, List<IFormFile> files);
        Task<ApiResponse<object>> DeleteTemplateAsync(Guid id);

        // User responses
        Task<ApiResponse<UserResponseDto>> GetUserResponseAsync(Guid userId, Guid templateId);
        Task<ApiResponse<UserResponseDto>> GetUserResponseByIdAsync(Guid id);
        Task<ApiResponse<List<UserResponseDto>>> GetUserResponsesByCategoryAsync(Guid userId, Guid categoryId);
        Task<ApiResponse<Guid>> SubmitUserResponseAsync(CreateUserResponseDto dto);
    }
} 