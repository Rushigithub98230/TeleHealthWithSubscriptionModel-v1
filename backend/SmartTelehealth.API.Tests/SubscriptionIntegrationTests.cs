using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartTelehealth.API;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Tests;

public class SubscriptionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly HttpClient _client;
    private readonly string _testDbName;

    public SubscriptionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _testDbName = $"TestDb_{Guid.NewGuid()}";
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.test.json", optional: false);
                config.AddEnvironmentVariables();
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for integration tests with a shared name
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_testDbName);
                });

                // Override JWT configuration for tests
                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("test_jwt_secret_key_for_tests_only")),
                        ValidateIssuer = true,
                        ValidIssuer = "SmartTelehealth.Test",
                        ValidateAudience = true,
                        ValidAudience = "SmartTelehealthTestUsers",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                // Add authorization policies
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOnly", policy =>
                        policy.RequireClaim("isAdmin", "true"));
                });

                // Add missing services for tests
                services.AddScoped<IPdfService, MockPdfService>();
            });
        });

        _client = _factory.CreateClient();
        
        // Get services from the factory
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        
        // Ensure database is created
        _context.Database.EnsureCreated();
        
        // Seed test data
        SeedTestDataAsync().Wait();
    }

    private async Task SeedTestDataAsync()
    {
        // Create roles
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new Role { Name = "User" });
        }
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new Role { Name = "Admin" });
        }

        // Create test users
        var testUser = new User
        {
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };

        var adminUser = new User
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true
        };

        if (await _userManager.FindByEmailAsync(testUser.Email) == null)
        {
            var result = await _userManager.CreateAsync(testUser, "TestPassword123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(testUser, "User");
            }
        }

        if (await _userManager.FindByEmailAsync(adminUser.Email) == null)
        {
            var result = await _userManager.CreateAsync(adminUser, "AdminPassword123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create test billing cycle
        var monthlyBillingCycle = new MasterBillingCycle
        {
            Id = Guid.NewGuid(),
            Name = "Monthly",
            Description = "Monthly billing cycle",
            DurationInDays = 30,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (!_context.MasterBillingCycles.Any())
        {
            _context.MasterBillingCycles.Add(monthlyBillingCycle);
            await _context.SaveChangesAsync();
        }

        // Create test subscription plans
        var basicPlan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Basic Plan",
            Description = "Basic telehealth subscription",
            Price = 29.99m,
            IsActive = true,
            BillingCycleId = monthlyBillingCycle.Id,
            CreatedAt = DateTime.UtcNow
        };

        var premiumPlan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Premium Plan",
            Description = "Premium telehealth subscription",
            Price = 59.99m,
            IsActive = true,
            BillingCycleId = monthlyBillingCycle.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (!_context.SubscriptionPlans.Any())
        {
            _context.SubscriptionPlans.AddRange(basicPlan, premiumPlan);
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateJwtToken(User user, string role)
    {
        var key = Encoding.ASCII.GetBytes("test_jwt_secret_key_for_tests_only");
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Role, role)
        };

        if (role == "Admin")
        {
            claims.Add(new Claim("isAdmin", "true"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "SmartTelehealth.Test",
            Audience = "SmartTelehealthTestUsers",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new InvalidOperationException($"User with email {email} not found");
        }
        
        var token = GenerateJwtToken(user, role);
        
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task CreateSubscription_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        var plan = await _context.SubscriptionPlans.FirstAsync();
        
        var createDto = new CreateSubscriptionDto
        {
            PlanId = plan.Id.ToString(),
            UserId = (await _userManager.FindByEmailAsync("testuser@test.com")).Id.ToString(),
            StartDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/subscriptions", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Active", result.Data.Status);
    }

    [Fact]
    public async Task CreateSubscription_WithUnauthorizedUser_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No authentication
        var plan = await _context.SubscriptionPlans.FirstAsync();
        
        var createDto = new CreateSubscriptionDto
        {
            PlanId = plan.Id.ToString(),
            UserId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/subscriptions", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlan_AsAdmin_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@test.com", "Admin");
        
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Test Plan",
            Description = "Test Description",
            Price = 39.99m
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/subscriptions/plans", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionPlanDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Plan", result.Data.Name);
    }

    [Fact]
    public async Task CreatePlan_AsUser_ReturnsForbidden()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        var createDto = new CreateSubscriptionPlanDto
        {
            Name = "Test Plan",
            Description = "Test Description",
            Price = 39.99m
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/subscriptions/plans", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscription_WithValidId_ReturnsSubscription()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a subscription first
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            BillingCycleId = plan.BillingCycleId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            CurrentPrice = plan.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/api/subscriptions/{subscription.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(subscription.Id.ToString(), result.Data.Id);
        Assert.Equal("Active", result.Data.Status);
    }

    [Fact]
    public async Task CancelSubscription_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a subscription first
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            BillingCycleId = plan.BillingCycleId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            CurrentPrice = plan.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var reason = "User request";
        var content = new StringContent(JsonSerializer.Serialize(reason), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync($"/api/subscriptions/{subscription.Id}/cancel", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Cancelled", result.Data.Status);
    }

    [Fact]
    public async Task GetAllPlans_AsAdmin_ReturnsAllPlans()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@test.com", "Admin");

        // Act
        var response = await client.GetAsync("/webadmin/subscription-management/plans");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<SubscriptionPlanDto>>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Any());
    }

    [Fact]
    public async Task GetAllPlans_AsUser_ReturnsForbidden()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");

        // Act
        var response = await client.GetAsync("/webadmin/subscription-management/plans");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserSubscriptions_WithValidUser_ReturnsSubscriptions()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        // Create test subscriptions
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var subscriptions = new List<Subscription>
        {
            new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SubscriptionPlanId = plan.Id,
                BillingCycleId = plan.BillingCycleId,
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                NextBillingDate = DateTime.UtcNow.AddDays(30),
                CurrentPrice = plan.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SubscriptionPlanId = plan.Id,
                BillingCycleId = plan.BillingCycleId,
                Status = "Cancelled",
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(-30),
                NextBillingDate = DateTime.UtcNow.AddDays(-30),
                CurrentPrice = plan.Price,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            }
        };
        
        _context.Subscriptions.AddRange(subscriptions);
        await _context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/api/subscriptions/user/{user.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<SubscriptionDto>>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task PauseSubscription_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a subscription first
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            BillingCycleId = plan.BillingCycleId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            CurrentPrice = plan.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var response = await client.PostAsync($"/api/subscriptions/{subscription.Id}/pause", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Paused", result.Data.Status);
    }

    [Fact]
    public async Task ResumeSubscription_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a paused subscription first
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = plan.Id,
            BillingCycleId = plan.BillingCycleId,
            Status = "Paused",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            CurrentPrice = plan.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var response = await client.PostAsync($"/api/subscriptions/{subscription.Id}/resume", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Active", result.Data.Status);
    }

    [Fact]
    public async Task UpgradeSubscription_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a subscription and a premium plan
        var basicPlan = await _context.SubscriptionPlans.FirstAsync(p => p.Name == "Basic Plan");
        var premiumPlan = await _context.SubscriptionPlans.FirstAsync(p => p.Name == "Premium Plan");
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SubscriptionPlanId = basicPlan.Id,
            BillingCycleId = basicPlan.BillingCycleId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            NextBillingDate = DateTime.UtcNow.AddDays(30),
            CurrentPrice = basicPlan.Price,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var content = new StringContent(JsonSerializer.Serialize(premiumPlan.Id.ToString()), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync($"/api/subscriptions/{subscription.Id}/upgrade", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(premiumPlan.Id.ToString(), result.Data.PlanId);
    }

    [Fact(Skip = "Billing history test needs to be implemented with proper billing record creation")]
    public async Task GetBillingHistory_WithValidSubscription_ReturnsHistory()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        // Create a subscription through the API
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var createSubscriptionDto = new CreateSubscriptionDto
        {
            PlanId = plan.Id.ToString(),
            UserId = user.Id.ToString(),
            StartDate = DateTime.UtcNow
        };

        var subscriptionJson = JsonSerializer.Serialize(createSubscriptionDto);
        var subscriptionContent = new StringContent(subscriptionJson, Encoding.UTF8, "application/json");
        var subscriptionResponse = await client.PostAsync("/api/subscriptions", subscriptionContent);
        subscriptionResponse.EnsureSuccessStatusCode();
        
        var subscriptionResult = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(
            await subscriptionResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var subscriptionId = Guid.Parse(subscriptionResult.Data.Id);

        // Create a billing record through the API instead of directly in the database
        var createBillingRecordDto = new CreateBillingRecordDto
        {
            UserId = user.Id.ToString(),
            SubscriptionId = subscriptionId.ToString(),
            Amount = plan.Price,
            Description = "Test billing record",
            Type = "Subscription"
        };

        var billingJson = JsonSerializer.Serialize(createBillingRecordDto);
        var billingContent = new StringContent(billingJson, Encoding.UTF8, "application/json");
        var billingResponse = await client.PostAsync("/api/billing", billingContent);
        billingResponse.EnsureSuccessStatusCode();

        // Act
        var response = await client.GetAsync($"/api/billing/subscription/{subscriptionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<BillingRecordDto>>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(subscriptionId.ToString(), result.Data.First().SubscriptionId);
    }

    [Fact]
    public async Task CompleteSubscriptionLifecycle_FromPurchaseToCancellation_WorksEndToEnd()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        var plan = await _context.SubscriptionPlans.FirstAsync();
        var user = await _userManager.FindByEmailAsync("testuser@test.com");
        
        var createDto = new CreateSubscriptionDto
        {
            PlanId = plan.Id.ToString(),
            UserId = user.Id.ToString(),
            StartDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Create subscription
        var createResponse = await client.PostAsync("/api/subscriptions", content);
        createResponse.EnsureSuccessStatusCode();
        
        var createResult = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(
            await createResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(createResult);
        Assert.True(createResult.Success);
        Assert.Equal("Active", createResult.Data.Status);

        // Act - Cancel subscription
        var cancelContent = new StringContent(JsonSerializer.Serialize("User request"), Encoding.UTF8, "application/json");
        var cancelResponse = await client.PostAsync($"/api/subscriptions/{createResult.Data.Id}/cancel", cancelContent);
        cancelResponse.EnsureSuccessStatusCode();
        
        var cancelResult = JsonSerializer.Deserialize<ApiResponse<SubscriptionDto>>(
            await cancelResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(cancelResult);
        Assert.True(cancelResult.Success);
        Assert.Equal("Cancelled", cancelResult.Data.Status);
    }

    [Fact]
    public async Task AdminManagement_WithFullPipeline_WorksEndToEnd()
    {
        // Arrange
        var adminClient = await CreateAuthenticatedClientAsync("admin@test.com", "Admin");
        var userClient = await CreateAuthenticatedClientAsync("testuser@test.com", "User");
        
        var createPlanDto = new CreateSubscriptionPlanDto
        {
            Name = "Integration Test Plan",
            Description = "Plan created during integration test",
            Price = 49.99m
        };

        var json = JsonSerializer.Serialize(createPlanDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Admin creates plan
        var createResponse = await adminClient.PostAsync("/api/subscriptions/plans", content);
        createResponse.EnsureSuccessStatusCode();
        
        var createResult = JsonSerializer.Deserialize<ApiResponse<SubscriptionPlanDto>>(
            await createResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(createResult);
        Assert.True(createResult.Success);
        Assert.Equal("Integration Test Plan", createResult.Data.Name);

        // Act - User tries to create plan (should fail)
        var userCreateResponse = await userClient.PostAsync("/api/subscriptions/plans", content);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, userCreateResponse.StatusCode);

        // Act - Admin gets all plans
        var getAllResponse = await adminClient.GetAsync("/webadmin/subscription-management/plans");
        getAllResponse.EnsureSuccessStatusCode();
        
        var getAllResult = JsonSerializer.Deserialize<ApiResponse<IEnumerable<SubscriptionPlanDto>>>(
            await getAllResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        Assert.NotNull(getAllResult);
        Assert.True(getAllResult.Success);
        Assert.True(getAllResult.Data.Any(p => p.Name == "Integration Test Plan"));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

// Mock PDF Service for testing
public class MockPdfService : IPdfService
{
    public Task<byte[]> GenerateInvoicePdfAsync(BillingRecordDto billingRecord, UserDto user, SubscriptionDto? subscription = null)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
    }

    public Task<byte[]> GenerateBillingHistoryPdfAsync(IEnumerable<BillingRecordDto> billingRecords, UserDto user)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
    }

    public Task<byte[]> GenerateSubscriptionSummaryPdfAsync(SubscriptionDto subscription, UserDto user)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
    }
}
