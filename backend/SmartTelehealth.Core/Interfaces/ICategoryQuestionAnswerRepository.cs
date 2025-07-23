using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Core.Interfaces
{
    public interface ICategoryQuestionAnswerRepository
    {
        Task<CategoryQuestionAnswer?> GetByIdAsync(Guid id);
        Task<IEnumerable<CategoryQuestionAnswer>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<CategoryQuestionAnswer>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<CategoryQuestionAnswer>> GetAllAsync();
        Task AddAsync(CategoryQuestionAnswer answer);
        Task UpdateAsync(CategoryQuestionAnswer answer);
        Task DeleteAsync(Guid id);
    }
} 