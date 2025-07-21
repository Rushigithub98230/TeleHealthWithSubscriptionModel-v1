using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class CategoryQuestionAnswerService : ICategoryQuestionAnswerService
    {
        private readonly ICategoryQuestionAnswerRepository _repo;
        public CategoryQuestionAnswerService(ICategoryQuestionAnswerRepository repo)
        {
            _repo = repo;
        }
        public Task<CategoryQuestionAnswer?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
        public Task<IEnumerable<CategoryQuestionAnswer>> GetByUserIdAsync(Guid userId) => _repo.GetByUserIdAsync(userId);
        public Task<IEnumerable<CategoryQuestionAnswer>> GetByCategoryIdAsync(Guid categoryId) => _repo.GetByCategoryIdAsync(categoryId);
        public Task<IEnumerable<CategoryQuestionAnswer>> GetAllAsync() => _repo.GetAllAsync();
        public Task AddAsync(CategoryQuestionAnswer answer) => _repo.AddAsync(answer);
        public Task UpdateAsync(CategoryQuestionAnswer answer) => _repo.UpdateAsync(answer);
        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
} 