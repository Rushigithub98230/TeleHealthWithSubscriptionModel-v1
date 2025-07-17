# Immediate Next Steps - Backend Development

## 🚀 Quick Start Guide

### Step 1: Complete Entity Framework Setup (Today)

1. **Add missing using statements to ApplicationDbContext**
   ```csharp
   using System.ComponentModel.DataAnnotations;
   ```

2. **Create initial migration**
   ```bash
   dotnet ef migrations add InitialCreate --project src/SmartTelehealth.Infrastructure --startup-project src/SmartTelehealth.API
   ```

3. **Update database**
   ```bash
   dotnet ef database update --project src/SmartTelehealth.Infrastructure --startup-project src/SmartTelehealth.API
   ```

### Step 2: Create Repository Interfaces (Day 1-2)

Create these interfaces in `src/SmartTelehealth.Core/Interfaces/`:

```csharp
// ISubscriptionRepository.cs
public interface ISubscriptionRepository
{
    Task<Subscription> GetByIdAsync(Guid id);
    Task<IEnumerable<Subscription>> GetByUserIdAsync(Guid userId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription> UpdateAsync(Subscription subscription);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync();
}

// ICategoryRepository.cs
public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<Category> CreateAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task<bool> DeleteAsync(Guid id);
}
```

### Step 3: Implement Repository Classes (Day 2-3)

Create these implementations in `src/SmartTelehealth.Infrastructure/Repositories/`:

```csharp
// SubscriptionRepository.cs
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;
    
    public SubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Subscription> GetByIdAsync(Guid id)
    {
        return await _context.Subscriptions
            .Include(s => s.User)
            .Include(s => s.SubscriptionPlan)
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    // Implement other methods...
}
```

### Step 4: Create Service Interfaces (Day 3-4)

Create these in `src/SmartTelehealth.Application/Interfaces/`:

```csharp
// ISubscriptionService.cs
public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto createDto);
    Task<SubscriptionDto> GetSubscriptionAsync(Guid id);
    Task<SubscriptionDto> UpdateSubscriptionAsync(UpdateSubscriptionDto updateDto);
    Task<SubscriptionDto> PauseSubscriptionAsync(Guid id, string reason);
    Task<SubscriptionDto> ResumeSubscriptionAsync(Guid id);
    Task<SubscriptionDto> CancelSubscriptionAsync(Guid id, string reason);
    Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);
}
```

### Step 5: Implement Core Services (Day 4-5)

Create these in `src/SmartTelehealth.Application/Services/`:

```csharp
// SubscriptionService.cs
public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    
    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto createDto)
    {
        // Implementation here
    }
    
    // Implement other methods...
}
```

### Step 6: Add AutoMapper Profiles (Day 5)

Create `src/SmartTelehealth.Application/Mapping/MappingProfile.cs`:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.SubscriptionPlan.Name))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubscriptionPlan.Category.Name));
            
        CreateMap<CreateSubscriptionDto, Subscription>();
        CreateMap<UpdateSubscriptionDto, Subscription>();
        
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
    }
}
```

### Step 7: Update Controllers (Day 6)

Update the existing controllers to use the services:

```csharp
// SubscriptionsController.cs
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionDto>>> GetUserSubscriptions()
    {
        try
        {
            var userId = GetCurrentUserId(); // Implement this method
            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user subscriptions");
            return StatusCode(500, "An error occurred while retrieving subscriptions");
        }
    }
    
    // Update other methods...
}
```

### Step 8: Add JWT Authentication (Day 7-8)

1. **Add JWT settings to appsettings.json**
2. **Create JWT service**
3. **Update Program.cs with JWT configuration**
4. **Add authentication to controllers**

### Step 9: Create Seed Data (Day 8-9)

Create seed data for categories and subscription plans:

```csharp
// SeedData.cs
public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Primary Care",
                    Description = "General health consultations and preventive care",
                    BasePrice = 25.00m,
                    ConsultationFee = 15.00m,
                    IsActive = true,
                    DisplayOrder = 1
                },
                // Add more categories...
            };
            
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}
```

## 🎯 Week 1 Goals

- [ ] Complete Entity Framework setup
- [ ] Implement repository pattern
- [ ] Create basic service layer
- [ ] Add AutoMapper configuration
- [ ] Update API controllers
- [ ] Add JWT authentication
- [ ] Create seed data

## 🎯 Week 2 Goals

- [ ] Implement subscription CRUD operations
- [ ] Add billing logic
- [ ] Create category management
- [ ] Add basic validation
- [ ] Implement error handling
- [ ] Add logging

## 🚨 Critical Path Items

1. **Database Setup** - Must be completed first
2. **Repository Pattern** - Foundation for all data access
3. **Service Layer** - Core business logic
4. **JWT Authentication** - Required for API security
5. **Basic CRUD Operations** - Essential functionality

## 📋 Daily Checklist

### Day 1:
- [ ] Set up Entity Framework migrations
- [ ] Create repository interfaces
- [ ] Test database connection

### Day 2:
- [ ] Implement repository classes
- [ ] Add basic CRUD operations
- [ ] Test repository methods

### Day 3:
- [ ] Create service interfaces
- [ ] Implement basic services
- [ ] Add AutoMapper profiles

### Day 4:
- [ ] Update API controllers
- [ ] Test API endpoints
- [ ] Add error handling

### Day 5:
- [ ] Add JWT authentication
- [ ] Create seed data
- [ ] Test authentication

## 🔧 Development Environment Setup

1. **Install Required Tools:**
   - .NET 8 SDK
   - SQL Server LocalDB
   - Visual Studio 2022 or VS Code

2. **Configure Development Environment:**
   - Update connection string in appsettings.json
   - Set up Stripe test keys
   - Configure SendGrid API key

3. **Run Setup Script:**
   ```bash
   .\setup.ps1
   ```

## 📚 Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [AutoMapper Documentation](https://docs.automapper.org/en/stable/)

## 🎯 Success Metrics

- [ ] All API endpoints return proper responses
- [ ] Database operations work correctly
- [ ] Authentication system functional
- [ ] Basic CRUD operations working
- [ ] Error handling implemented
- [ ] Logging configured

This immediate action plan will get you started with the core backend functionality. Focus on completing each step before moving to the next, and test thoroughly at each stage. 