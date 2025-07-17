using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _context;
    public SubscriptionPlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
        => await _context.SubscriptionPlans.FindAsync(id);

    public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        => await _context.SubscriptionPlans.ToListAsync();

    public async Task AddAsync(SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Update(plan);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.SubscriptionPlans.FindAsync(id);
        if (entity != null)
        {
            _context.SubscriptionPlans.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
} 