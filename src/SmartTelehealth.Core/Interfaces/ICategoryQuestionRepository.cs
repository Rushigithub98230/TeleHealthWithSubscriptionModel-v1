using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Core.Interfaces
{
    public interface ICategoryQuestionRepository
    {
        Task<CategoryQuestion?> GetByIdAsync(Guid id);
        Task<IEnumerable<CategoryQuestion>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<CategoryQuestion>> GetAllAsync();
        Task AddAsync(CategoryQuestion question);
        Task UpdateAsync(CategoryQuestion question);
        Task DeleteAsync(Guid id);
    }
} 