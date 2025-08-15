using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class BillingRepository : IBillingRepository
    {
        private readonly ApplicationDbContext _context;
        public BillingRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<BillingRecord?> GetByIdAsync(Guid id)
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Include(br => br.Currency)
                .FirstOrDefaultAsync(br => br.Id == id);
        }
        
        public async Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.BillingRecords
                .Include(br => br.Subscription)
                .Include(br => br.Currency)
                .Where(br => br.UserId == userId)
                .OrderByDescending(br => br.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Currency)
                .Where(br => br.SubscriptionId == subscriptionId)
                .OrderByDescending(br => br.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<BillingRecord> CreateAsync(BillingRecord billingRecord)
        {
            billingRecord.CreatedDate = DateTime.UtcNow;
            billingRecord.UpdatedDate = DateTime.UtcNow;
            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();
            return billingRecord;
        }
        
        public async Task<BillingRecord> UpdateAsync(BillingRecord billingRecord)
        {
            billingRecord.UpdatedDate = DateTime.UtcNow;
            _context.BillingRecords.Update(billingRecord);
            await _context.SaveChangesAsync();
            return billingRecord;
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            var billingRecord = await _context.BillingRecords.FindAsync(id);
            if (billingRecord == null)
                return false;
                
            _context.BillingRecords.Remove(billingRecord);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<BillingRecord>> GetPendingPaymentsAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.Status == BillingRecord.BillingStatus.Pending)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<BillingRecord>> GetOverduePaymentsAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.Status == BillingRecord.BillingStatus.Pending && br.DueDate < DateTime.UtcNow)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<BillingRecord>> GetFailedPaymentsAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.Status == BillingRecord.BillingStatus.Failed)
                .OrderByDescending(br => br.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<BillingAdjustment> CreateAdjustmentAsync(BillingAdjustment adjustment)
        {
            adjustment.CreatedDate = DateTime.UtcNow;
            _context.BillingAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();
            return adjustment;
        }
        
        public async Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId)
        {
            return await _context.BillingAdjustments
                .Include(ba => ba.BillingRecord)
                .Where(ba => ba.BillingRecordId == billingRecordId)
                .OrderByDescending(ba => ba.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<BillingRecord>> GetAllAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Include(br => br.Currency)
                .OrderByDescending(br => br.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId)
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.BillingCycleId == billingCycleId)
                .OrderByDescending(br => br.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetOverdueRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.Status == BillingRecord.BillingStatus.Pending && br.DueDate < DateTime.UtcNow)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetPendingRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(br => br.User)
                .Include(br => br.Subscription)
                .Where(br => br.Status == BillingRecord.BillingStatus.Pending)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }
    }
} 