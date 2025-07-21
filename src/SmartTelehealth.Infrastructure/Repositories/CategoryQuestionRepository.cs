using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class CategoryQuestionRepository : ICategoryQuestionRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryQuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CategoryQuestion?> GetByIdAsync(Guid id) =>
            await _context.CategoryQuestions.FindAsync(id);
        public async Task<IEnumerable<CategoryQuestion>> GetByCategoryIdAsync(Guid categoryId) =>
            await _context.CategoryQuestions.Where(q => q.CategoryId == categoryId && q.IsActive).ToListAsync();
        public async Task<IEnumerable<CategoryQuestion>> GetAllAsync() =>
            await _context.CategoryQuestions.ToListAsync();
        public async Task AddAsync(CategoryQuestion question)
        {
            await _context.CategoryQuestions.AddAsync(question);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(CategoryQuestion question)
        {
            _context.CategoryQuestions.Update(question);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var q = await _context.CategoryQuestions.FindAsync(id);
            if (q != null)
            {
                _context.CategoryQuestions.Remove(q);
                await _context.SaveChangesAsync();
            }
        }
    }
} 