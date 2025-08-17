using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Core.Interfaces;
using AutoMapper; // Added for AutoMapper

namespace SmartTelehealth.Tests
{
    public class EndToEndIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public EndToEndIntegrationTests()
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
            
            // Add HttpContextAccessor for AuditService
            services.AddHttpContextAccessor();
            
            // Use SQL Server for integration testing
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer("Server=localhost\\SQLEXPRESS2022;Database=SmartTeleHealthTestDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"));
            
            // Register repositories only (no complex services)
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
            services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
            services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
            services.AddScoped<IBillingRepository, BillingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created and seeded
            _context.Database.EnsureCreated();
            SeedTestDataAsync().Wait();
        }

        private async Task SeedTestDataAsync()
        {
            // Create master data
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
                _context.MasterPrivilegeTypes.Add(new MasterPrivilegeType { Name = "General", Description = "General privileges", IsActive = true, CreatedDate = DateTime.UtcNow, CreatedBy = 1 });
                await _context.SaveChangesAsync();
            }

            if (!_context.Privileges.Any())
            {
                var privilegeTypeId = await _context.MasterPrivilegeTypes.Select(pt => pt.Id).FirstAsync();
                _context.Privileges.AddRange(
                    new Privilege { Name = "Teleconsultation", Description = "Video consultation with healthcare providers", IsActive = true, PrivilegeTypeId = privilegeTypeId, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "VideoCall", Description = "Video call functionality", IsActive = true, PrivilegeTypeId = privilegeTypeId, CreatedDate = DateTime.UtcNow, CreatedBy = 1 },
                    new Privilege { Name = "Prescription", Description = "Digital prescription service", IsActive = true, PrivilegeTypeId = privilegeTypeId, CreatedDate = DateTime.UtcNow, CreatedBy = 1 }
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

        [Fact]
        public async Task Test_Complete_Subscription_Lifecycle_With_Real_Database()
        {
            // Step 1: Get test data from database
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");
            var primaryCareCategory = await _context.Categories.FirstAsync(x => x.Name == "Primary Care");

            monthlyBillingCycle.Should().NotBeNull();
            usdCurrency.Should().NotBeNull();
            consultationPrivilege.Should().NotBeNull();
            primaryCareCategory.Should().NotBeNull();

            // Step 2: Create test user directly in database
            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var testUser = new User
            {
                UserName = "testuser1@test.com",
                Email = "testuser1@test.com",
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = DateTime.Now.AddYears(-25),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Step 3: Create subscription plan
            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Basic Healthcare Plan",
                Description = "Basic healthcare plan with limited features",
                Price = 29.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 7,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync();

            // Step 4: Create plan privileges
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

            // Step 5: Create subscription for test user
            var subscription = new Subscription
            {
                UserId = testUser.Id,
                SubscriptionPlanId = subscriptionPlan.Id,
                BillingCycleId = monthlyBillingCycle.Id,
                Status = Subscription.SubscriptionStatuses.TrialActive,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7), // Trial period
                NextBillingDate = DateTime.UtcNow.AddDays(7),
                CurrentPrice = subscriptionPlan.Price,
                AutoRenew = true,
                IsActive = true,
                IsTrialSubscription = true,
                TrialStartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(7),
                StripeSubscriptionId = "sub_test_" + Guid.NewGuid().ToString("N"),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Step 6: Test subscription retrieval
            var retrievedSubscription = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstAsync(s => s.Id == subscription.Id);

            retrievedSubscription.Should().NotBeNull();
            retrievedSubscription.Status.Should().Be(Subscription.SubscriptionStatuses.TrialActive);
            retrievedSubscription.IsTrialSubscription.Should().BeTrue();
            retrievedSubscription.SubscriptionPlan.Should().NotBeNull();
            retrievedSubscription.SubscriptionPlan.Name.Should().Be("Basic Healthcare Plan");

            // Step 7: Test privilege usage tracking
            var privilegeUsage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = 1,
                AllowedValue = 5,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserSubscriptionPrivilegeUsages.Add(privilegeUsage);
            await _context.SaveChangesAsync();

            // Step 8: Test billing record creation
            var billingRecord = new BillingRecord
            {
                UserId = testUser.Id,
                SubscriptionId = subscription.Id,
                Amount = subscriptionPlan.Price,
                TaxAmount = 0m,
                ShippingAmount = 0m,
                TotalAmount = subscriptionPlan.Price,
                Status = BillingRecord.BillingStatus.Pending,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                CurrencyId = usdCurrency.Id,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Step 9: Test payment processing simulation
            var paymentRecord = new SubscriptionPayment
            {
                SubscriptionId = subscription.Id,
                CurrencyId = usdCurrency.Id,
                Amount = subscriptionPlan.Price,
                NetAmount = subscriptionPlan.Price,
                Description = "Subscription payment",
                Status = SubscriptionPayment.PaymentStatus.Succeeded,
                Type = SubscriptionPayment.PaymentType.Subscription,
                DueDate = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow,
                BillingPeriodStart = DateTime.UtcNow,
                BillingPeriodEnd = DateTime.UtcNow.AddMonths(1),
                StripePaymentIntentId = "pi_test_" + Guid.NewGuid().ToString("N"),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPayments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            // Step 10: Update billing record status
            billingRecord.Status = BillingRecord.BillingStatus.Paid;
            billingRecord.PaidAt = DateTime.UtcNow;
            billingRecord.UpdatedBy = testUser.Id;
            billingRecord.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Step 11: Test subscription status update after trial
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.IsTrialSubscription = false;
            subscription.TrialEndDate = DateTime.UtcNow;
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = DateTime.UtcNow.AddMonths(1);
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscription.UpdatedBy = testUser.Id;
            subscription.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Step 12: Verify final state
            var finalSubscription = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Include(s => s.User)
                .FirstAsync(s => s.Id == subscription.Id);

            finalSubscription.Should().NotBeNull();
            finalSubscription.Status.Should().Be(Subscription.SubscriptionStatuses.Active);
            finalSubscription.IsTrialSubscription.Should().BeFalse();
            finalSubscription.User.Should().NotBeNull();
            finalSubscription.User.Email.Should().Be("testuser1@test.com");

            // Step 13: Test privilege usage query
            var userPrivileges = await _context.UserSubscriptionPrivilegeUsages
                .Include(up => up.SubscriptionPlanPrivilege)
                .ThenInclude(spp => spp.Privilege)
                .Where(up => up.SubscriptionId == subscription.Id)
                .ToListAsync();

            userPrivileges.Should().HaveCount(1);
            userPrivileges[0].SubscriptionPlanPrivilege.Privilege.Name.Should().Be("Teleconsultation");
            userPrivileges[0].RemainingValue.Should().Be(4);

            // Step 14: Test billing history
            var billingHistory = await _context.BillingRecords
                .Include(b => b.Subscription)
                .Where(b => b.UserId == testUser.Id)
                .ToListAsync();

            billingHistory.Should().HaveCount(1);
            billingHistory[0].Status.Should().Be(BillingRecord.BillingStatus.Paid);
            billingHistory[0].Amount.Should().Be(29.99m);

            // Step 15: Test payment history
            var paymentHistory = await _context.SubscriptionPayments
                .Where(p => p.SubscriptionId == subscription.Id)
                .ToListAsync();

            paymentHistory.Should().HaveCount(1);
            paymentHistory[0].Status.Should().Be(SubscriptionPayment.PaymentStatus.Succeeded);
            paymentHistory[0].Amount.Should().Be(29.99m);
        }

        [Fact]
        public async Task Test_Subscription_Plan_Management_With_Real_Database()
        {
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");

            // Create a new subscription plan
            var newPlan = new SubscriptionPlan
            {
                Name = "Premium Healthcare Plan",
                Description = "Premium healthcare plan with unlimited features",
                Price = 79.99m,
                BillingCycleId = monthlyBillingCycle.Id,
                CurrencyId = usdCurrency.Id,
                IsActive = true,
                IsTrialAllowed = true,
                TrialDurationInDays = 14,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(newPlan);
            await _context.SaveChangesAsync();

            // Create plan privileges
            var planPrivilege = new SubscriptionPlanPrivilege
            {
                SubscriptionPlanId = newPlan.Id,
                PrivilegeId = consultationPrivilege.Id,
                Value = -1, // Unlimited consultations
                UsagePeriodId = monthlyBillingCycle.Id,
                DurationMonths = 1,
                IsActive = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPlanPrivileges.Add(planPrivilege);
            await _context.SaveChangesAsync();

            // Verify plan creation
            var retrievedPlan = await _context.SubscriptionPlans
                .Include(sp => sp.PlanPrivileges)
                .ThenInclude(spp => spp.Privilege)
                .FirstAsync(sp => sp.Id == newPlan.Id);

            retrievedPlan.Should().NotBeNull();
            retrievedPlan.Name.Should().Be("Premium Healthcare Plan");
            retrievedPlan.Price.Should().Be(79.99m);
            retrievedPlan.PlanPrivileges.Should().HaveCount(1);
            retrievedPlan.PlanPrivileges.First().Privilege.Name.Should().Be("Teleconsultation");
            retrievedPlan.PlanPrivileges.First().Value.Should().Be(-1); // Unlimited
        }

        [Fact]
        public async Task Test_User_Subscription_Privilege_Usage_With_Real_Database()
        {
            // Create a unique user for this test
            var patientRole = await _context.UserRoles.FirstAsync(x => x.Name == "Patient");
            var testUser = new User
            {
                UserName = "testuser2@test.com",
                Email = "testuser2@test.com",
                FirstName = "Test",
                LastName = "User2",
                DateOfBirth = DateTime.Now.AddYears(-25),
                EmailConfirmed = true,
                IsActive = true,
                UserRoleId = patientRole.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1
            };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Create a unique subscription for this test
            var monthlyBillingCycle = await _context.MasterBillingCycles.FirstAsync(x => x.Name == "Monthly");
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");
            var consultationPrivilege = await _context.Privileges.FirstAsync(x => x.Name == "Teleconsultation");
            
            var subscriptionPlan = new SubscriptionPlan
            {
                Name = "Test Plan 2",
                Description = "Test plan for privilege usage",
                Price = 19.99m,
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
                Value = 10,
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

            // Test privilege usage tracking
            var initialUsage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = 2,
                AllowedValue = 5,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserSubscriptionPrivilegeUsages.Add(initialUsage);
            await _context.SaveChangesAsync();

            // Test privilege limit reached
            var limitReachedUsage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = 5,
                AllowedValue = 5,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.UserSubscriptionPrivilegeUsages.Add(limitReachedUsage);
            await _context.SaveChangesAsync();

            // Verify usage tracking
            var totalUsage = await _context.UserSubscriptionPrivilegeUsages
                .Where(up => up.SubscriptionId == subscription.Id)
                .SumAsync(up => up.UsedValue);

            totalUsage.Should().Be(7); // 2 + 5 = 7 (only from this test)

            // Test privilege availability check
            var canUsePrivilege = await _context.UserSubscriptionPrivilegeUsages
                .Where(up => up.SubscriptionId == subscription.Id)
                .OrderByDescending(up => up.CreatedDate)
                .Select(up => up.RemainingValue > 0)
                .FirstOrDefaultAsync();

            canUsePrivilege.Should().BeFalse(); // No privileges remaining
        }

        [Fact]
        public async Task Test_Billing_And_Payment_Flow_With_Real_Database()
        {
            var testUser = await _context.Users.FirstAsync();
            var subscription = await _context.Subscriptions.FirstAsync();
            var usdCurrency = await _context.MasterCurrencies.FirstAsync(x => x.Code == "USD");

            // Create billing record
            var billingRecord = new BillingRecord
            {
                UserId = testUser.Id,
                SubscriptionId = subscription.Id,
                Amount = 29.99m,
                TaxAmount = 2.99m,
                ShippingAmount = 0m,
                TotalAmount = 32.98m,
                Status = BillingRecord.BillingStatus.Pending,
                BillingDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                CurrencyId = usdCurrency.Id,
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.BillingRecords.Add(billingRecord);
            await _context.SaveChangesAsync();

            // Process payment
            var payment = new SubscriptionPayment
            {
                SubscriptionId = subscription.Id,
                CurrencyId = usdCurrency.Id,
                Amount = 32.98m,
                NetAmount = 32.98m,
                Description = "Subscription payment",
                Status = SubscriptionPayment.PaymentStatus.Succeeded,
                Type = SubscriptionPayment.PaymentType.Subscription,
                DueDate = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow,
                BillingPeriodStart = DateTime.UtcNow,
                BillingPeriodEnd = DateTime.UtcNow.AddMonths(1),
                StripePaymentIntentId = "pi_test_" + Guid.NewGuid().ToString("N"),
                CreatedBy = testUser.Id,
                CreatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPayments.Add(payment);
            await _context.SaveChangesAsync();

            // Update billing status
            billingRecord.Status = BillingRecord.BillingStatus.Paid;
            billingRecord.PaidAt = DateTime.UtcNow;
            billingRecord.UpdatedBy = testUser.Id;
            billingRecord.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Verify payment flow
            var finalBillingRecord = await _context.BillingRecords
                .Include(b => b.Subscription)
                .FirstAsync(b => b.Id == billingRecord.Id);

            finalBillingRecord.Status.Should().Be(BillingRecord.BillingStatus.Paid);
            finalBillingRecord.PaidAt.Should().NotBeNull();

            var paymentRecord = await _context.SubscriptionPayments
                .FirstAsync(p => p.Id == payment.Id);

            paymentRecord.Status.Should().Be(SubscriptionPayment.PaymentStatus.Succeeded);
        }

        [Fact]
        public async Task Test_Subscription_Lifecycle_Management_With_Real_Database()
        {
            var testUser = await _context.Users.FirstAsync();
            var subscription = await _context.Subscriptions.FirstAsync();

            // Test subscription pause
            subscription.Status = Subscription.SubscriptionStatuses.Paused;
            subscription.UpdatedBy = testUser.Id;
            subscription.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var pausedSubscription = await _context.Subscriptions
                .FirstAsync(s => s.Id == subscription.Id);
            pausedSubscription.Status.Should().Be(Subscription.SubscriptionStatuses.Paused);

            // Test subscription resume
            subscription.Status = Subscription.SubscriptionStatuses.Active;
            subscription.UpdatedBy = testUser.Id;
            subscription.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var resumedSubscription = await _context.Subscriptions
                .FirstAsync(s => s.Id == subscription.Id);
            resumedSubscription.Status.Should().Be(Subscription.SubscriptionStatuses.Active);

            // Test subscription cancellation
            subscription.Status = Subscription.SubscriptionStatuses.Cancelled;
            subscription.UpdatedBy = testUser.Id;
            subscription.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var cancelledSubscription = await _context.Subscriptions
                .FirstAsync(s => s.Id == subscription.Id);
            cancelledSubscription.Status.Should().Be(Subscription.SubscriptionStatuses.Cancelled);
        }

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
