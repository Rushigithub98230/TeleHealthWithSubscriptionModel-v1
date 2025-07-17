using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly ApplicationDbContext _context;
        public PrescriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task<Prescription> GetByIdAsync(Guid id) => Task.FromResult<Prescription>(null);
        public Task<IEnumerable<Prescription>> GetByUserIdAsync(Guid userId) => Task.FromResult<IEnumerable<Prescription>>(new List<Prescription>());
        public Task<IEnumerable<Prescription>> GetByProviderIdAsync(Guid providerId) => Task.FromResult<IEnumerable<Prescription>>(new List<Prescription>());
        public Task<IEnumerable<Prescription>> GetByStatusAsync(string status) => Task.FromResult<IEnumerable<Prescription>>(new List<Prescription>());
        public Task<Prescription> CreateAsync(Prescription prescription) => Task.FromResult<Prescription>(null);
        public Task<Prescription> UpdateAsync(Prescription prescription) => Task.FromResult<Prescription>(null);
        public Task<bool> DeleteAsync(Guid id) => Task.FromResult(false);
        public Task<IEnumerable<Prescription>> GetOverduePrescriptionsAsync() => Task.FromResult<IEnumerable<Prescription>>(new List<Prescription>());
        public Task<IEnumerable<Prescription>> GetRefillRequestsAsync(Guid userId) => Task.FromResult<IEnumerable<Prescription>>(new List<Prescription>());
        public Task<int> GetPrescriptionCountAsync(Guid userId) => Task.FromResult(0);
        public Task<decimal> GetPrescriptionTotalAsync(Guid userId) => Task.FromResult(0m);
    }
} 