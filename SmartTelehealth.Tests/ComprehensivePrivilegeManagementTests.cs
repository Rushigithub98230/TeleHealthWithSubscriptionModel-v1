using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Application.Services;
using Xunit;
using FluentAssertions;
using AutoMapper;

namespace SmartTelehealth.Tests
{
    public class ComprehensivePrivilegeManagementTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPrivilegeService _privilegeService;
        private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
        private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IPrivilegeRepository _privilegeRepo;

        public ComprehensivePrivilegeManagementTests()
        {
            // Setup test database
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Add AutoMapper
            services.AddAutoMapper(typeof(SmartTelehealth.Application.DependencyInjection).Assembly);
            
            // Use SQL Server for integration testing
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS2022;Database=SmartTeleHealthTestDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"));
            
            // Register repositories and services
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
            services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
            services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IPrivilegeService, PrivilegeService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            
            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Get required services
            _privilegeService = _serviceProvider.GetRequiredService<IPrivilegeService>();
            _planPrivilegeRepo = _serviceProvider.GetRequiredService<ISubscriptionPlanPrivilegeRepository>();
            _usageRepo = _serviceProvider.GetRequiredService<IUserSubscriptionPrivilegeUsageRepository>();
            _subscriptionRepo = _serviceProvider.GetRequiredService<ISubscriptionRepository>();
            _privilegeRepo = _serviceProvider.GetRequiredService<IPrivilegeRepository>();
            
            // Ensure database is created and seeded
            _context.Database.EnsureCreated();
            SeedTestDataAsync().Wait();
        }

        private async Task SeedTestDataAsync()
        {
            // Create master data if not exists
            if (!_context.MasterBillingCycles.Any())
            {
                _context.MasterBillingCycles.AddRange(
                    new MasterBillingCycle { Name = "Monthly", DurationInDays = 30, IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new MasterBillingCycle { Name = "Annual", DurationInDays = 365, IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.MasterCurrencies.Any())
            {
                _context.MasterCurrencies.AddRange(
                    new MasterCurrency { Code = "USD", Name = "US Dollar", Symbol = "$", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new MasterCurrency { Code = "EUR", Name = "Euro", Symbol = "€", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.MasterPrivilegeTypes.Any())
            {
                _context.MasterPrivilegeTypes.AddRange(
                    new MasterPrivilegeType { Name = "General", Description = "General privileges", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new MasterPrivilegeType { Name = "Premium", Description = "Premium features", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new MasterPrivilegeType { Name = "Specialist", Description = "Specialist services", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.Privileges.Any())
            {
                var privilegeTypes = await _context.MasterPrivilegeTypes.ToListAsync();
                _context.Privileges.AddRange(
                    new Privilege { Name = "Teleconsultation", Description = "Video consultation with healthcare providers", IsActive = true, PrivilegeTypeId = privilegeTypes[0].Id, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "VideoCall", Description = "Video call functionality", IsActive = true, PrivilegeTypeId = privilegeTypes[0].Id, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "Prescription", Description = "Digital prescription service", IsActive = true, PrivilegeTypeId = privilegeTypes[0].Id, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "PrioritySupport", Description = "Priority customer support", IsActive = true, PrivilegeTypeId = privilegeTypes[1].Id, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "SpecialistConsultation", Description = "Consultation with specialists", IsActive = true, PrivilegeTypeId = privilegeTypes[2].Id, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.Categories.Any())
            {
                _context.Categories.AddRange(
                    new Category { Name = "Primary Care", Description = "General healthcare services", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Category { Name = "Specialist Care", Description = "Specialized medical services", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.UserRoles.Any())
            {
                _context.UserRoles.AddRange(
                    new UserRole { Name = "Patient", Description = "Regular patient user", SortOrder = 1, IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new UserRole { Name = "Provider", Description = "Healthcare provider", SortOrder = 2, IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new UserRole { Name = "Admin", Description = "System administrator", SortOrder = 3, IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
                );
                await _context.SaveChangesAsync();
            }
        }

        #region 1. ADMIN PRIVILEGE MANAGEMENT TESTS

        [Fact]
        public async Task Test_Create_New_Privilege()
        {
            // Arrange
            var privilegeType = await _context.MasterPrivilegeTypes.FirstAsync();
            var newPrivilege = new Privilege
            {
                Name = "NewTestPrivilege",
                Description = "A new test privilege for testing",
                PrivilegeTypeId = privilegeType.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };

            // Act
            await _privilegeRepo.AddAsync(newPrivilege);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedPrivilege = await _context.Privileges
                .Include(p => p.PrivilegeType)
                .FirstAsync(p => p.Name == "NewTestPrivilege");

            retrievedPrivilege.Should().NotBeNull();
            retrievedPrivilege.Description.Should().Be("A new test privilege for testing");
            retrievedPrivilege.PrivilegeType.Name.Should().Be(privilegeType.Name);
            retrievedPrivilege.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Test_Update_Existing_Privilege()
        {
            // Arrange
            var existingPrivilege = await _context.Privileges.FirstAsync();
            var originalDescription = existingPrivilege.Description;
            existingPrivilege.Description = "Updated description for testing";
            existingPrivilege.UpdatedDate = DateTime.UtcNow;
            existingPrivilege.UpdatedBy = 1;

            // Act
            await _privilegeRepo.UpdateAsync(existingPrivilege);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPrivilege = await _context.Privileges.FindAsync(existingPrivilege.Id);
            updatedPrivilege.Should().NotBeNull();
            updatedPrivilege.Description.Should().Be("Updated description for testing");
            updatedPrivilege.Description.Should().NotBe(originalDescription);
            updatedPrivilege.UpdatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Delete_Privilege()
        {
            // Arrange
            var privilegeToDelete = await _context.Privileges.FirstAsync();
            var privilegeId = privilegeToDelete.Id;

            // Act
            await _privilegeRepo.DeleteAsync(privilegeId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedPrivilege = await _context.Privileges.FindAsync(privilegeId);
            deletedPrivilege.Should().BeNull();
        }

        [Fact]
        public async Task Test_Get_All_Privileges()
        {
            // Act
            var allPrivileges = await _privilegeRepo.GetAllAsync();

            // Assert
            allPrivileges.Should().NotBeNull();
            allPrivileges.Should().HaveCountGreaterThan(0);
            allPrivileges.Should().OnlyContain(p => p.IsActive);
        }

        [Fact]
        public async Task Test_Get_Privilege_By_Id()
        {
            // Arrange
            var expectedPrivilege = await _context.Privileges.FirstAsync();

            // Act
            var retrievedPrivilege = await _privilegeRepo.GetByIdAsync(expectedPrivilege.Id);

            // Assert
            retrievedPrivilege.Should().NotBeNull();
            retrievedPrivilege.Id.Should().Be(expectedPrivilege.Id);
            retrievedPrivilege.Name.Should().Be(expectedPrivilege.Name);
        }

        #endregion

        #region 2. SUBSCRIPTION PLAN PRIVILEGE MANAGEMENT TESTS

        [Fact]
        public async Task Test_Create_Subscription_Plan_With_Privileges()
        {
            // Arrange
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");
            var videoCallPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "VideoCall");

            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Premium Test Plan",
                Description = "Premium plan with multiple privileges",
                Price = 99.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 14,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            // Create plan privileges
            var planPrivileges = new List<SubscriptionPlanPrivilege>
            {
                new SubscriptionPlanPrivilege
                {
                    SubscriptionPlanId = subscriptionPlan.Id,
                    PrivilegeId = consultationPrivilege.Id,
                    Value = 10, // 10 consultations per month
                    UsagePeriodId = monthlyBillingCycle.Id,
                    DurationMonths = 1,
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new SubscriptionPlanPrivilege
                {
                    SubscriptionPlanId = subscriptionPlan.Id,
                    PrivilegeId = videoCallPrivilege.Id,
                    Value = -1, // Unlimited video calls
                    UsagePeriodId = monthlyBillingCycle.Id,
                    DurationMonths = 1,
                    IsActive = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                }
            };

            _context.SubscriptionPlanPrivileges.AddRange(planPrivileges);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedPlan = await _context.SubscriptionPlans
                .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
                .FirstAsync(sp => sp.Id == subscriptionPlan.Id);

            retrievedPlan.Should().NotBeNull();
            retrievedPlan.PlanPrivileges.Should().HaveCount(2);
            retrievedPlan.PlanPrivileges.Should().Contain(pp => pp.Privilege.Name == "Teleconsultation" && pp.Value == 10);
            retrievedPlan.PlanPrivileges.Should().Contain(pp => pp.Privilege.Name == "VideoCall" && pp.Value == -1);
        }

        [Fact]
        public async Task Test_Update_Subscription_Plan_Privileges()
        {
            // Arrange - Create a specific plan privilege to update
            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");

            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Update Test Plan",
                Description = "Plan for testing privilege updates",
                Price = 29.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                CreatedBy = patientRole.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            var planPrivilege = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = subscriptionPlan.Id,
                PrivilegeId = consultationPrivilege.Id,
                Value = 5, // Initial value
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = patientRole.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlanPrivileges.Add(planPrivilege);
            await _context.SaveChangesAsync();

            var originalValue = planPrivilege.Value;
            planPrivilege.Value = 20; // Increase limit
            planPrivilege.UpdatedDate = DateTime.UtcNow;
            planPrivilege.UpdatedBy = patientRole.Id;

            // Act
            await _planPrivilegeRepo.UpdateAsync(planPrivilege);
            await _context.SaveChangesAsync();

            // Assert
            var updatedPlanPrivilege = await _context.SubscriptionPlanPrivileges
                .Include(spp => spp.Privilege)
                .FirstAsync(spp => spp.Id == planPrivilege.Id);

            updatedPlanPrivilege.Value.Should().Be(20);
            updatedPlanPrivilege.Value.Should().NotBe(originalValue);
            updatedPlanPrivilege.UpdatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_Disable_Subscription_Plan_Privilege()
        {
            // Arrange
            var planPrivilege = await _context.SubscriptionPlanPrivileges.FirstAsync();
            planPrivilege.IsActive = false;
            planPrivilege.UpdatedDate = DateTime.UtcNow;
            planPrivilege.UpdatedBy = 1;

            // Act
            await _planPrivilegeRepo.UpdateAsync(planPrivilege);
            await _context.SaveChangesAsync();

            // Assert
            var disabledPlanPrivilege = await _context.SubscriptionPlanPrivileges.FindAsync(planPrivilege.Id);
            disabledPlanPrivilege.Should().NotBeNull();
            disabledPlanPrivilege.IsActive.Should().BeFalse();
        }

        #endregion

        #region 3. ADVANCED USAGE SCENARIOS TESTS

        [Fact]
        public async Task Test_Complex_Privilege_Usage_Scenarios()
        {
            // Arrange - Create test user and subscription
            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var testUser = new User
            {
                UserName = "privilegetest@test.com",
                Email = "privilegetest@test.com",
                FirstName = "Privilege",
                LastName = "Test",
                DateOfBirth = DateTime.Now.AddYears(-30),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");

            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Complex Test Plan",
                Description = "Plan for testing complex privilege scenarios",
                Price = 49.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            var planPrivilege = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = subscriptionPlan.Id,
                PrivilegeId = consultationPrivilege.Id,
                Value = 5, // 5 consultations per month
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlanPrivileges.Add(planPrivilege);
            await _context.SaveChangesAsync();

            var subscription = new Subscription
            {
                UserId = testUser.Id,
                SubscriptionPlanId = subscriptionPlan.Id,
                BillingCycleId = monthlyBillingCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Act & Assert - Test privilege usage incrementally
            var tokenModel = new TokenModel { UserID = testUser.Id, RoleID = patientRole.Id };

            // Test 1: Initial privilege usage
            var initialRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            initialRemaining.Should().BeGreaterThan(0);

            var used1 = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 1, tokenModel);
            used1.Should().BeTrue();

            var remaining1 = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            remaining1.Should().Be(4);

            // Test 2: Multiple privilege usage
            var used2 = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 2, tokenModel);
            used2.Should().BeTrue();

            var remaining2 = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            remaining2.Should().Be(2);

            // Test 3: Try to use more than remaining
            var used3 = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 3, tokenModel);
            used3.Should().BeFalse(); // Should fail - only 2 remaining

            var remaining3 = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            remaining3.Should().Be(2); // Should still be 2

            // Test 4: Use exactly remaining amount
            var used4 = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 2, tokenModel);
            used4.Should().BeTrue();

            var remaining4 = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            remaining4.Should().Be(0);

            // Test 5: Try to use when exhausted
            var used5 = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 1, tokenModel);
            used5.Should().BeFalse();

            var remaining5 = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);
            remaining5.Should().Be(0);
        }

        [Fact]
        public async Task Test_Unlimited_Privilege_Usage()
        {
            // Arrange - Create plan with unlimited privilege
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var videoCallPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "VideoCall");

            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Unlimited Test Plan",
                Description = "Plan with unlimited video calls",
                Price = 79.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            var planPrivilege = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = subscriptionPlan.Id,
                PrivilegeId = videoCallPrivilege.Id,
                Value = -1, // Unlimited
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlanPrivileges.Add(planPrivilege);
            await _context.SaveChangesAsync();

            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var testUser = new User
            {
                UserName = "unlimitedtest@test.com",
                Email = "unlimitedtest@test.com",
                FirstName = "Unlimited",
                LastName = "Test",
                DateOfBirth = DateTime.Now.AddYears(-25),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            var subscription = new Subscription
            {
                UserId = testUser.Id,
                SubscriptionPlanId = subscriptionPlan.Id,
                BillingCycleId = monthlyBillingCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Act & Assert
            var tokenModel = new TokenModel { UserID = testUser.Id, RoleID = patientRole.Id };

            // Test unlimited usage
            for (int i = 0; i < 100; i++) // Use privilege 100 times
            {
                var used = await _privilegeService.UsePrivilegeAsync(subscription.Id, "VideoCall", 1, tokenModel);
                used.Should().BeTrue();
            }

            var unlimitedRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "VideoCall", tokenModel);
            unlimitedRemaining.Should().Be(int.MaxValue); // Should still be unlimited

            var finalRemaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "VideoCall", tokenModel);
            finalRemaining.Should().Be(int.MaxValue); // Should always be unlimited
        }

        #endregion

        #region 4. EDGE CASES & ERROR HANDLING TESTS

        [Fact]
        public async Task Test_Invalid_Privilege_Name()
        {
            // Arrange
            var subscription = await _context.Subscriptions.FirstAsync();
            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "NonExistentPrivilege", tokenModel);

            // Assert
            remaining.Should().Be(0); // Should return 0 for non-existent privilege
        }

        [Fact]
        public async Task Test_Negative_Usage_Amount()
        {
            // Arrange
            var subscription = await _context.Subscriptions.FirstAsync();
            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act
            var used = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", -1, tokenModel);

            // Assert
            used.Should().BeFalse(); // Should fail for negative amount
        }

        [Fact]
        public async Task Test_Zero_Usage_Amount()
        {
            // Arrange
            var subscription = await _context.Subscriptions.FirstAsync();
            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act
            var used = await _privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 0, tokenModel);

            // Assert
            used.Should().BeFalse(); // Should fail for zero amount
        }

        [Fact]
        public async Task Test_Usage_Without_Active_Subscription()
        {
            // Arrange
            var inactiveSubscription = await _context.Subscriptions.FirstAsync();
            inactiveSubscription.Status = Subscription.SubscriptionStatuses.Cancelled;
            inactiveSubscription.UpdatedDate = DateTime.UtcNow;
            inactiveSubscription.UpdatedBy = 1;
            await _context.SaveChangesAsync();

            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act
            var used = await _privilegeService.UsePrivilegeAsync(inactiveSubscription.Id, "Teleconsultation", 1, tokenModel);

            // Assert
            used.Should().BeFalse(); // Should fail for inactive subscription
        }

        [Fact]
        public async Task Test_Usage_With_Disabled_Plan_Privilege()
        {
            // Arrange
            var planPrivilege = await _context.SubscriptionPlanPrivileges.FirstAsync();
            planPrivilege.IsActive = false;
            planPrivilege.UpdatedDate = DateTime.UtcNow;
            planPrivilege.UpdatedBy = 1;
            await _context.SaveChangesAsync();

            var subscription = await _context.Subscriptions.FirstAsync();
            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act
            var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscription.Id, "Teleconsultation", tokenModel);

            // Assert
            remaining.Should().Be(0); // Should return 0 for disabled plan privilege
        }

        #endregion

        #region 5. USAGE ANALYTICS & REPORTING TESTS

        [Fact]
        public async Task Test_Privilege_Usage_Statistics()
        {
            // Clean up existing usage data to avoid interference
            _context.UserSubscriptionPrivilegeUsages.RemoveRange(_context.UserSubscriptionPrivilegeUsages);
            await _context.SaveChangesAsync();
            
            // Arrange - Create multiple users with different usage patterns
            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");

            // User 1: Heavy usage
            var user1 = new User
            {
                UserName = "heavyuser@test.com",
                Email = "heavyuser@test.com",
                FirstName = "Heavy",
                LastName = "User",
                DateOfBirth = DateTime.Now.AddYears(-35),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };
            _context.Users.Add(user1);

            // User 2: Light usage
            var user2 = new User
            {
                UserName = "lightuser@test.com",
                Email = "lightuser@test.com",
                FirstName = "Light",
                LastName = "User",
                DateOfBirth = DateTime.Now.AddYears(-28),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };
            _context.Users.Add(user2);

            await _context.SaveChangesAsync();

            // Create subscription plans
            var plan1 = new SubscriptionPlan
            {
                Name = "Heavy Usage Plan",
                Description = "Plan for heavy users",
                Price = 99.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(plan1);

            var plan2 = new SubscriptionPlan
            {
                Name = "Light Usage Plan",
                Description = "Plan for light users",
                Price = 49.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = false,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlans.Add(plan2);

            await _context.SaveChangesAsync();

            // Create plan privileges
            var planPrivilege1 = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = plan1.Id,
                PrivilegeId = consultationPrivilege.Id,
                Value = 20, // 20 consultations per month
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlanPrivileges.Add(planPrivilege1);

            var planPrivilege2 = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = plan2.Id,
                PrivilegeId = consultationPrivilege.Id,
                Value = 5, // 5 consultations per month
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.SubscriptionPlanPrivileges.Add(planPrivilege2);

            await _context.SaveChangesAsync();

            // Create subscriptions
            var subscription1 = new Subscription
            {
                UserId = user1.Id,
                SubscriptionPlanId = plan1.Id,
                BillingCycleId = monthlyBillingCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedBy = user1.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription1);

            var subscription2 = new Subscription
            {
                UserId = user2.Id,
                SubscriptionPlanId = plan2.Id,
                BillingCycleId = monthlyBillingCycle.Id,
                Status = Subscription.SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedBy = user2.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription2);

            await _context.SaveChangesAsync();

            // Create usage records
            var usage1 = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription1.Id,
                SubscriptionPlanPrivilegeId = planPrivilege1.Id,
                UsedValue = 15, // Heavy usage: 15 out of 20
                AllowedValue = 20,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = user1.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.UserSubscriptionPrivilegeUsages.Add(usage1);

            var usage2 = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription2.Id,
                SubscriptionPlanPrivilegeId = planPrivilege2.Id,
                UsedValue = 2, // Light usage: 2 out of 5
                AllowedValue = 5,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = user2.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.UserSubscriptionPrivilegeUsages.Add(usage2);

            await _context.SaveChangesAsync();

            // Act & Assert - Test analytics queries
            var tokenModel1 = new TokenModel { UserID = user1.Id, RoleID = patientRole.Id };
            var tokenModel2 = new TokenModel { UserID = user2.Id, RoleID = patientRole.Id };

            // Test individual usage statistics
            var remaining1 = await _privilegeService.GetRemainingPrivilegeAsync(subscription1.Id, "Teleconsultation", tokenModel1);
            remaining1.Should().Be(5); // 20 - 15 = 5

            var remaining2 = await _privilegeService.GetRemainingPrivilegeAsync(subscription2.Id, "Teleconsultation", tokenModel2);
            remaining2.Should().Be(3); // 5 - 2 = 3

            // Test usage percentage calculations
            var usagePercentage1 = (decimal)15 / 20 * 100; // 75%
            var usagePercentage2 = (decimal)2 / 5 * 100; // 40%

            // Test aggregate usage statistics
            var totalUsage = await _context.UserSubscriptionPrivilegeUsages
                .Where(u => u.SubscriptionPlanPrivilege.Privilege.Name == "Teleconsultation")
                .SumAsync(u => u.UsedValue);

            totalUsage.Should().Be(17); // 15 + 2 = 17

            var totalAllowed = await _context.UserSubscriptionPrivilegeUsages
                .Where(u => u.SubscriptionPlanPrivilege.Privilege.Name == "Teleconsultation")
                .SumAsync(u => u.AllowedValue);

            totalAllowed.Should().Be(25); // 20 + 5 = 25

            var overallUsagePercentage = (decimal)totalUsage / totalAllowed * 100;
            overallUsagePercentage.Should().Be(68); // (17/25) * 100 = 68%
        }

        [Fact]
        public async Task Test_Privilege_Usage_By_Plan_Type()
        {
            // Arrange - Get existing data
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");
            var videoCallPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "VideoCall");

            // Act - Query usage by privilege type
            var consultationUsage = await _context.UserSubscriptionPrivilegeUsages
                .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
                .Where(u => u.SubscriptionPlanPrivilege.Privilege.Name == "Teleconsultation")
                .ToListAsync();

            var videoCallUsage = await _context.UserSubscriptionPrivilegeUsages
                .Include(u => u.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
                .Where(u => u.SubscriptionPlanPrivilege.Privilege.Name == "VideoCall")
                .ToListAsync();

            // Assert
            consultationUsage.Should().NotBeNull();
            videoCallUsage.Should().NotBeNull();

            // Test that we can aggregate usage by privilege type
            var totalConsultationUsage = consultationUsage.Sum(u => u.UsedValue);
            var totalVideoCallUsage = videoCallUsage.Sum(u => u.UsedValue);

            // Test usage efficiency (used vs allowed)
            var consultationEfficiency = consultationUsage
                .Where(u => u.AllowedValue > 0)
                .Select(u => (decimal)u.UsedValue / u.AllowedValue * 100)
                .ToList();

            consultationEfficiency.Should().NotBeEmpty();
        }

        #endregion

        #region 6. CONCURRENT OPERATIONS TESTS

        [Fact]
        public async Task Test_Concurrent_Privilege_Usage()
        {
            // Arrange
            var subscription = await _context.Subscriptions.FirstAsync();
            var tokenModel = new TokenModel { UserID = 1, RoleID = 1 };

            // Act - Simulate concurrent privilege usage
            var tasks = new List<Task<bool>>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(_privilegeService.UsePrivilegeAsync(subscription.Id, "Teleconsultation", 1, tokenModel));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(5);
            // Note: In a real scenario, you'd want to test actual concurrency handling
            // This test verifies the basic functionality works under load
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
