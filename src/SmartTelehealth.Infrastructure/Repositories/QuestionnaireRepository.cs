using SmartTelehealth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly Data.ApplicationDbContext _context;
        public QuestionnaireRepository(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
        {
            return await _context.QuestionnaireTemplates
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(Guid categoryId)
        {
            return await _context.QuestionnaireTemplates
                .Where(t => t.CategoryId == categoryId)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionnaireTemplate>> GetAllTemplatesAsync()
        {
            return await _context.QuestionnaireTemplates
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .ToListAsync();
        }

        public async Task AddTemplateAsync(QuestionnaireTemplate template)
        {
            _context.QuestionnaireTemplates.Add(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTemplateAsync(QuestionnaireTemplate template)
        {
            _context.QuestionnaireTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(Guid id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template != null)
            {
                _context.QuestionnaireTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        // User responses
        public async Task<UserResponse?> GetUserResponseAsync(Guid userId, Guid templateId)
        {
            return await _context.UserResponses
                .Include(r => r.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TemplateId == templateId);
        }

        public async Task<IEnumerable<UserResponse>> GetUserResponsesByCategoryAsync(Guid userId, Guid categoryId)
        {
            return await _context.UserResponses
                .Where(r => r.UserId == userId && r.CategoryId == categoryId)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .ToListAsync();
        }

        public async Task AddUserResponseAsync(UserResponse response)
        {
            _context.UserResponses.Add(response);
            await _context.SaveChangesAsync();
        }
    }
} 