using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;

namespace SmartTelehealth.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Provider?> GetByIdAsync(int id)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetActiveProvidersAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetAvailableProvidersAsync()
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && p.IsAvailable && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersByCategoryAsync(Guid categoryId)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.CategoryId == categoryId && pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.ProviderCategories.Any(pc => pc.CategoryId == categoryId && pc.IsAvailable) &&
                       p.IsActive && p.IsAvailable && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersBySpecialtyAsync(string specialty)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.Specialty.ToLower().Contains(specialty.ToLower()) &&
                       p.IsActive && p.IsAvailable && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<Provider> CreateAsync(Provider provider)
    {
        provider.CreatedDate = DateTime.UtcNow;
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<Provider> UpdateAsync(Provider provider)
    {
        provider.UpdatedDate = DateTime.UtcNow;
        _context.Providers.Update(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var provider = await _context.Providers.FindAsync(id);
        if (provider == null) return false;

        provider.IsDeleted = true;
        provider.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Providers
            .AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber)
    {
        return await _context.Providers
            .AnyAsync(p => p.LicenseNumber == licenseNumber && !p.IsDeleted);
    }

    public async Task<int> GetActiveProviderCountAsync()
    {
        return await _context.Providers
            .CountAsync(p => p.IsActive && !p.IsDeleted);
    }

    public async Task<IEnumerable<Provider>> SearchProvidersAsync(string searchTerm)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => (p.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                        p.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                        p.Specialty.ToLower().Contains(searchTerm.ToLower())) &&
                       p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Provider>> GetProvidersByAvailabilityAsync(TimeSpan time)
    {
        return await _context.Providers
            .Include(p => p.ProviderCategories.Where(pc => pc.IsAvailable))
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && p.IsAvailable && !p.IsDeleted &&
                       (!p.AvailableFrom.HasValue || time >= p.AvailableFrom.Value) &&
                       (!p.AvailableTo.HasValue || time <= p.AvailableTo.Value))
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }
} 