using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class CategoryQuestionAnswerRepository : ICategoryQuestionAnswerRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryQuestionAnswerRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CategoryQuestionAnswer?> GetByIdAsync(Guid id) =>
            await _context.CategoryQuestionAnswers.FindAsync(id);
        public async Task<IEnumerable<CategoryQuestionAnswer>> GetByUserIdAsync(Guid userId) =>
            await _context.CategoryQuestionAnswers.Where(a => a.UserId == userId).ToListAsync();
        public async Task<IEnumerable<CategoryQuestionAnswer>> GetByCategoryIdAsync(Guid categoryId) =>
            await _context.CategoryQuestionAnswers
                .Include(a => a.CategoryQuestion)
                .Where(a => a.CategoryQuestion.CategoryId == categoryId)
                .ToListAsync();
        public async Task<IEnumerable<CategoryQuestionAnswer>> GetAllAsync() =>
            await _context.CategoryQuestionAnswers.ToListAsync();
        public async Task AddAsync(CategoryQuestionAnswer answer)
        {
            await _context.CategoryQuestionAnswers.AddAsync(answer);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(CategoryQuestionAnswer answer)
        {
            _context.CategoryQuestionAnswers.Update(answer);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var a = await _context.CategoryQuestionAnswers.FindAsync(id);
            if (a != null)
            {
                _context.CategoryQuestionAnswers.Remove(a);
                await _context.SaveChangesAsync();
            }
        }
    }
} 