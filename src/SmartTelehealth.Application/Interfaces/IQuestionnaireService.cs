using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IQuestionnaireService
    {
        // Template CRUD
        Task<QuestionnaireTemplateDto?> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<QuestionnaireTemplateDto>> GetTemplatesByCategoryAsync(Guid categoryId);
        Task<IEnumerable<QuestionnaireTemplateDto>> GetAllTemplatesAsync();
        Task<Guid> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto);
        Task UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto);
        Task DeleteTemplateAsync(Guid id);

        // User responses
        Task<UserResponseDto?> GetUserResponseAsync(Guid userId, Guid templateId);
        Task<IEnumerable<UserResponseDto>> GetUserResponsesByCategoryAsync(Guid userId, Guid categoryId);
        Task<Guid> SubmitUserResponseAsync(CreateUserResponseDto dto);
    }
} 