using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IQuestionnaireRepository _repo;
        public QuestionnaireService(IQuestionnaireRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto)
        {
            var template = new QuestionnaireTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                Questions = dto.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    IsRequired = q.IsRequired,
                    Order = q.Order,
                    HelpText = q.HelpText,
                    MediaUrl = q.MediaUrl,
                    CreatedAt = DateTime.UtcNow,
                    Options = q.Options.Select(o => new QuestionOption
                    {
                        Text = o.Text,
                        Value = o.Value,
                        Order = o.Order,
                        MediaUrl = o.MediaUrl
                    }).ToList()
                }).ToList()
            };
            await _repo.AddTemplateAsync(template);
            return template.Id;
        }

        public async Task DeleteTemplateAsync(Guid id)
        {
            await _repo.DeleteTemplateAsync(id);
        }

        public async Task<IEnumerable<QuestionnaireTemplateDto>> GetAllTemplatesAsync()
        {
            var templates = await _repo.GetAllTemplatesAsync();
            return templates.Select(MapToDto);
        }

        public async Task<QuestionnaireTemplateDto?> GetTemplateByIdAsync(Guid id)
        {
            var template = await _repo.GetTemplateByIdAsync(id);
            return template == null ? null : MapToDto(template);
        }

        public async Task<IEnumerable<QuestionnaireTemplateDto>> GetTemplatesByCategoryAsync(Guid categoryId)
        {
            var templates = await _repo.GetTemplatesByCategoryAsync(categoryId);
            return templates.Select(MapToDto);
        }

        public async Task<UserResponseDto?> GetUserResponseAsync(Guid userId, Guid templateId)
        {
            var response = await _repo.GetUserResponseAsync(userId, templateId);
            return response == null ? null : MapToDto(response);
        }

        public async Task<IEnumerable<UserResponseDto>> GetUserResponsesByCategoryAsync(Guid userId, Guid categoryId)
        {
            var responses = await _repo.GetUserResponsesByCategoryAsync(userId, categoryId);
            return responses.Select(MapToDto);
        }

        public async Task<Guid> SubmitUserResponseAsync(CreateUserResponseDto dto)
        {
            var response = new UserResponse
            {
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                TemplateId = dto.TemplateId,
                CreatedAt = DateTime.UtcNow,
                Answers = dto.Answers.Select(a => new UserAnswer
                {
                    QuestionId = a.QuestionId,
                    AnswerText = a.AnswerText,
                    CreatedAt = DateTime.UtcNow,
                    SelectedOptions = a.SelectedOptionIds.Select(optionId => new UserAnswerOption
                    {
                        OptionId = optionId
                    }).ToList()
                }).ToList()
            };
            await _repo.AddUserResponseAsync(response);
            return response.Id;
        }

        public async Task UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto)
        {
            var template = await _repo.GetTemplateByIdAsync(id);
            if (template == null) throw new Exception("Template not found");
            template.Name = dto.Name;
            template.Description = dto.Description;
            template.CategoryId = dto.CategoryId;
            template.IsActive = dto.IsActive;
            template.UpdatedAt = DateTime.UtcNow;
            // For simplicity, replace all questions (production: use smarter diff/merge)
            template.Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text,
                Type = q.Type,
                IsRequired = q.IsRequired,
                Order = q.Order,
                HelpText = q.HelpText,
                MediaUrl = q.MediaUrl,
                CreatedAt = DateTime.UtcNow,
                Options = q.Options.Select(o => new QuestionOption
                {
                    Text = o.Text,
                    Value = o.Value,
                    Order = o.Order,
                    MediaUrl = o.MediaUrl
                }).ToList()
            }).ToList();
            await _repo.UpdateTemplateAsync(template);
        }

        // Mapping helpers
        private static QuestionnaireTemplateDto MapToDto(QuestionnaireTemplate t)
        {
            return new QuestionnaireTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CategoryId = t.CategoryId,
                IsActive = t.IsActive,
                Version = t.Version,
                Questions = t.Questions?.OrderBy(q => q.Order).Select(MapToDto).ToList() ?? new()
            };
        }
        private static QuestionDto MapToDto(Question q)
        {
            return new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Type = q.Type,
                IsRequired = q.IsRequired,
                Order = q.Order,
                HelpText = q.HelpText,
                MediaUrl = q.MediaUrl,
                Options = q.Options?.OrderBy(o => o.Order).Select(MapToDto).ToList() ?? new()
            };
        }
        private static QuestionOptionDto MapToDto(QuestionOption o)
        {
            return new QuestionOptionDto
            {
                Id = o.Id,
                Text = o.Text,
                Value = o.Value,
                Order = o.Order,
                MediaUrl = o.MediaUrl
            };
        }
        private static UserResponseDto MapToDto(UserResponse r)
        {
            return new UserResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                CategoryId = r.CategoryId,
                TemplateId = r.TemplateId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Answers = r.Answers?.Select(MapToDto).ToList() ?? new()
            };
        }
        private static UserAnswerDto MapToDto(UserAnswer a)
        {
            return new UserAnswerDto
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                CreatedAt = a.CreatedAt,
                SelectedOptionIds = a.SelectedOptions?.Select(o => o.OptionId).ToList() ?? new()
            };
        }
    }
} 