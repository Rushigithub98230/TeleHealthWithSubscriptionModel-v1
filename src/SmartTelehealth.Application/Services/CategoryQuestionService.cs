using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Services
{
    public class CategoryQuestionService : ICategoryQuestionService
    {
        private readonly ICategoryQuestionRepository _repo;
        public CategoryQuestionService(ICategoryQuestionRepository repo)
        {
            _repo = repo;
        }
        public Task<CategoryQuestion?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);
        public Task<IEnumerable<CategoryQuestion>> GetByCategoryIdAsync(Guid categoryId) => _repo.GetByCategoryIdAsync(categoryId);
        public Task<IEnumerable<CategoryQuestion>> GetAllAsync() => _repo.GetAllAsync();
        public Task AddAsync(CategoryQuestion question) => _repo.AddAsync(question);
        public Task UpdateAsync(CategoryQuestion question) => _repo.UpdateAsync(question);
        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
} 