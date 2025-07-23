using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface ICategoryQuestionService
    {
        Task<CategoryQuestion?> GetByIdAsync(Guid id);
        Task<IEnumerable<CategoryQuestion>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<CategoryQuestion>> GetAllAsync();
        Task AddAsync(CategoryQuestion question);
        Task UpdateAsync(CategoryQuestion question);
        Task DeleteAsync(Guid id);
    }
} 