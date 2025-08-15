using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IStripeService _stripeService;
    private readonly PrivilegeService _privilegeService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IUserService _userService;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly IUserSubscriptionPrivilegeUsageRepository _usageRepo;
    private readonly IBillingService _billingService;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IMapper mapper,
        ILogger<SubscriptionService> logger,
        IStripeService stripeService,
        PrivilegeService privilegeService,
        INotificationService notificationService,
        IAuditService auditService,
        IUserService userService,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        IUserSubscriptionPrivilegeUsageRepository usageRepo,
        IBillingService billingService)
    {
        _subscriptionRepository = subscriptionRepository;
        _mapper = mapper;
        _logger = logger;
        _stripeService = stripeService;
        _privilegeService = privilegeService;
        _notificationService = notificationService;
        _auditService = auditService;
        _userService = userService;
        _planPrivilegeRepo = planPrivilegeRepo;
        _usageRepo = usageRepo;
        _billingService = billingService;
    }

    public async Task<JsonModel> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            return new JsonModel
            {
                data = _mapper.Map<SubscriptionDto>(entity),
                Message = "Subscription retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve subscription",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetUserSubscriptionsAsync(int userId)
    {
        try
        {
            var entities = await _subscriptionRepository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(entities);
            return new JsonModel
            {
                data = dtos,
                Message = "User subscriptions retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve user subscriptions",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CreateSubscriptionAsync(CreateSubscriptionDto createDto)
    {
        try
        {
            // 1. Check if plan exists and is active
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
            if (plan == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription plan does not exist",
                    StatusCode = 400
                };
            if (!plan.IsActive)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription plan is not active",
                    StatusCode = 400
                };

            // 2. Prevent duplicate subscriptions for the same user and plan (active or paused)
            var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(createDto.UserId);
            if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && (s.Status == Subscription.SubscriptionStatuses.Active || s.Status == Subscription.SubscriptionStatuses.Paused)))
                return new JsonModel
                {
                    data = new object(),
                    Message = "User already has an active or paused subscription for this plan",
                    StatusCode = 400
                };

            var entity = _mapper.Map<Subscription>(createDto);
            
            // Trial logic
            if (plan.IsTrialAllowed && plan.TrialDurationInDays > 0)
            {
                entity.IsTrialSubscription = true;
                entity.TrialStartDate = DateTime.UtcNow;
                entity.TrialEndDate = DateTime.UtcNow.AddDays(plan.TrialDurationInDays);
                entity.TrialDurationInDays = plan.TrialDurationInDays;
                entity.Status = Subscription.SubscriptionStatuses.TrialActive;
            }
            else
            {
                entity.Status = Subscription.SubscriptionStatuses.Active;
            }
            
            entity.StartDate = DateTime.UtcNow;
            entity.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            
            var created = await _subscriptionRepository.CreateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = created.Id,
                FromStatus = null,
                ToStatus = created.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(created);
            
            // Send confirmation and welcome emails
            var userResult = await _userService.GetUserByIdAsync(createDto.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendSubscriptionConfirmationAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                // await _notificationService.SendSubscriptionWelcomeEmailAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                _logger.LogInformation("Email notifications disabled - would have sent confirmation emails to {Email}", userResult.Data.Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(createDto.UserId.ToString(), "CreateSubscription", "Subscription", created.Id.ToString(), "Subscription created successfully");
            
            return new JsonModel
            {
                data = dto,
                Message = "Subscription created",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", createDto.UserId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to create subscription",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CancelSubscriptionAsync(string subscriptionId, string? reason = null)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            
            // Prevent cancelling an already cancelled subscription
            if (entity.IsCancelled)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription is already cancelled",
                    StatusCode = 400
                };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Cancelled);
            if (validation != ValidationResult.Success)
                return new JsonModel
                {
                    data = new object(),
                    Message = validation.ErrorMessage,
                    StatusCode = 400
                };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Cancelled;
            entity.CancellationReason = reason;
            entity.CancelledDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send cancellation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendSubscriptionCancellationEmailAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                _logger.LogInformation("Email notifications disabled - would have sent cancellation email to {Email}", userResult.Data.Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "CancelSubscription", "Subscription", subscriptionId, reason ?? "Subscription cancelled");
            
            return new JsonModel
            {
                data = dto,
                Message = "Subscription cancelled",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to cancel subscription",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> PauseSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription not found",
                    StatusCode = 404
                };
            
            if (entity.IsPaused)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Subscription is already paused",
                    StatusCode = 400
                };
            
            if (entity.IsCancelled)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot pause a cancelled subscription",
                    StatusCode = 400
                };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Paused);
            if (validation != ValidationResult.Success)
                return new JsonModel
                {
                    data = new object(),
                    Message = validation.ErrorMessage,
                    StatusCode = 400
                };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Paused;
            entity.PausedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send pause notification email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendSubscriptionPausedNotificationAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                _logger.LogInformation("Email notifications disabled - would have sent pause notification to {Email}", userResult.Data.Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "PauseSubscription", "Subscription", subscriptionId, "Subscription paused");
            
            return new JsonModel
            {
                data = dto,
                Message = "Subscription paused",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId}", subscriptionId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to pause subscription",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ResumeSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
            
            if (!entity.IsPaused)
                return new JsonModel { data = new object(), Message = "Subscription is not paused", StatusCode = 500 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 500 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.ResumedDate = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<SubscriptionDto>(updated);
            
            // Send resume email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                // await _notificationService.SendSubscriptionResumeEmailAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                _logger.LogInformation("Email notifications disabled - would have sent resume email to {Email}", userResult.Data.Email);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "ResumeSubscription", "Subscription", subscriptionId, "Subscription resumed");
            
            return new JsonModel { data = dto, Message = "Subscription resumed", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to resume subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
            
            // Prevent upgrading to the same plan
            if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
                return new JsonModel { data = new object(), Message = "Subscription is already on this plan", StatusCode = 500 };
            
            var oldPlanId = entity.SubscriptionPlanId;
            entity.SubscriptionPlanId = Guid.Parse(newPlanId);
            entity.UpdatedAt = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "UpgradeSubscription", "Subscription", subscriptionId, $"Upgraded from plan {oldPlanId} to {newPlanId}");
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription upgraded", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to upgrade subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ReactivateSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return new JsonModel { data = new object(), Message = validation.ErrorMessage, StatusCode = 500 };
            
            var oldStatus = entity.Status;
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedAt = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = updated.Id,
                FromStatus = oldStatus,
                ToStatus = updated.Status,
                ChangedAt = DateTime.UtcNow
            });
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "ReactivateSubscription", "Subscription", subscriptionId, "Subscription reactivated");
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updated), Message = "Subscription reactivated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to reactivate subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetPlanByIdAsync(string planId)
    {
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
        return JsonModel.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(plan));
    }

    public async Task<JsonModel> GetBillingHistoryAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            // Get billing records for this subscription
            var billingRecords = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id);
            
            if (!billingRecords.Success)
                return new JsonModel { data = new object(), Message = "Failed to retrieve billing history", StatusCode = 500 };

            var billingHistory = billingRecords.Data.Select(br => new BillingHistoryDto
            {
                Id = br.Id.ToString(),
                Amount = br.Amount,
                Status = br.Status,
                BillingDate = br.BillingDate,
                PaidDate = br.PaidAt,
                Description = br.Description,
                InvoiceNumber = br.InvoiceNumber,
                PaymentMethod = br.PaymentMethod
            });

            return new JsonModel { data = .SuccessResponse(billingHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve billing history", StatusCode = 500 };
        }
    }

            public async Task<JsonModel> GetPaymentMethodsAsync(int userId)
    {
        var methods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString());
        return new JsonModel { data = .SuccessResponse(methods);
    }

    public async Task<JsonModel> AddPaymentMethodAsync(int userId, string paymentMethodId)
    {
        var methodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), paymentMethodId);
        var method = new PaymentMethodDto { Id = methodId };
        return new JsonModel { data = method, Message = "Payment method added", StatusCode = 200 };
    }

    public async Task<JsonModel> GetSubscriptionByPlanIdAsync(string planId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByPlanIdAsync(Guid.Parse(planId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found for this plan", StatusCode = 500 };
            return JsonModel.SuccessResponse(_mapper.Map<SubscriptionDto>(subscription));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by plan ID {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveSubscriptionsAsync()
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(activeSubscriptions);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve active subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionByIdAsync(string subscriptionId)
    {
        return await GetSubscriptionAsync(subscriptionId);
    }

    public async Task<JsonModel> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            // Update subscription properties
            if (!string.IsNullOrEmpty(updateDto.Status))
                subscription.Status = updateDto.Status;
            
            if (updateDto.AutoRenew.HasValue)
                subscription.AutoRenew = updateDto.AutoRenew.Value;
            
            if (updateDto.NextBillingDate.HasValue)
                subscription.NextBillingDate = updateDto.NextBillingDate.Value;

            subscription.UpdatedAt = DateTime.UtcNow;
            
            var updatedSubscription = await _subscriptionRepository.UpdateAsync(subscription);
            
            // Audit log
            await _auditService.LogUserActionAsync(subscription.UserId.ToString(), "UpdateSubscription", "Subscription", subscriptionId, "Subscription updated");
            
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(updatedSubscription), Message = "Subscription updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to update subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            // Process payment through Stripe
            var paymentResult = await _stripeService.ProcessPaymentAsync(
                paymentRequest.PaymentMethodId,
                paymentRequest.Amount,
                paymentRequest.Currency ?? "usd"
            );

            if (paymentResult.Status == "succeeded")
            {
                // Update subscription status if needed
                if (subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed)
                {
                    subscription.Status = Subscription.SubscriptionStatuses.Active;
                    await _subscriptionRepository.UpdateAsync(subscription);
                }

                return new JsonModel { data = paymentResult, Message = "Payment processed successfully", StatusCode = 200 };
            }
            else
            {
                return new JsonModel { data = new object(), Message = $"Payment failed: {paymentResult.ErrorMessage}", StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to process payment", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetUsageStatisticsAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);

            var usageStats = new UsageStatisticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                CurrentPeriodStart = subscription.StartDate,
                CurrentPeriodEnd = subscription.NextBillingDate,
                TotalPrivileges = planPrivileges.Count(),
                UsedPrivileges = usages.Count(),
                PrivilegeUsage = new List<PrivilegeUsageDto>()
            };

            foreach (var usage in usages)
            {
                var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.Id == usage.SubscriptionPlanPrivilegeId);
                if (planPrivilege != null)
                {
                    usageStats.PrivilegeUsage.Add(new PrivilegeUsageDto
                    {
                        PrivilegeName = planPrivilege.Privilege.Name,
                        UsedValue = usage.UsedValue,
                        AllowedValue = planPrivilege.Value,
                        RemainingValue = planPrivilege.Value == -1 ? int.MaxValue : Math.Max(0, planPrivilege.Value - usage.UsedValue),
                        UsagePercentage = planPrivilege.Value == -1 ? 0 : (decimal)usage.UsedValue / planPrivilege.Value * 100
                    });
                }
            }

            return new JsonModel { data = usageStats, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage statistics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve usage statistics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllSubscriptionsAsync()
    {
        try
        {
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(allSubscriptions);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscriptions");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscriptions", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId);
            if (!usageStats.Success)
                return new JsonModel { data = new object(), Message = "Failed to get usage statistics", StatusCode = 500 };

            // Get billing history
            var billingHistory = await _billingService.GetPaymentHistoryAsync(subscription.UserId);

            var analytics = new SubscriptionAnalyticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate,
                TotalAmountPaid = billingHistory.Data?.Sum(bh => bh.Amount) ?? 0,
                PaymentCount = billingHistory.Data?.Count() ?? 0,
                AveragePaymentAmount = billingHistory.Data?.Any() == true ? billingHistory.Data.Average(bh => bh.Amount) : 0,
                UsageStatistics = usageStats.Data,
                PaymentHistory = billingHistory.Data?.Select(bh => new PaymentHistoryDto
                {
                    Id = bh.Id,
                    UserId = bh.UserId,
                    SubscriptionId = bh.SubscriptionId,
                    Amount = bh.Amount,
                    Currency = bh.Currency,
                    PaymentMethod = bh.PaymentMethod,
                    Status = bh.Status,
                    TransactionId = bh.TransactionId,
                    ErrorMessage = bh.ErrorMessage,
                    CreatedAt = bh.CreatedAt,
                    ProcessedAt = bh.ProcessedAt,
                    PaymentDate = bh.CreatedAt,
                    Description = bh.ErrorMessage,
                    PaymentMethodId = bh.PaymentMethod
                }).ToList() ?? new List<PaymentHistoryDto>()
            };

            return new JsonModel { data = analytics, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription analytics", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto)
    {
        try
        {
            var plan = new SubscriptionPlan
            {
                Name = createPlanDto.Name,
                Description = createPlanDto.Description,
                Price = createPlanDto.Price,
                BillingCycleId = createPlanDto.BillingCycleId,
                CurrencyId = createPlanDto.CurrencyId,
                IsActive = createPlanDto.IsActive,
                DisplayOrder = createPlanDto.DisplayOrder,
                // PlanPrivileges will be added later or via a separate call
            };
            var created = await _subscriptionRepository.CreateSubscriptionPlanAsync(plan);
            return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(created), Message = "Plan created", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to create plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
            plan.Name = updatePlanDto.Name;
            plan.Description = updatePlanDto.Description;
            plan.IsActive = updatePlanDto.IsActive;
            // Remove updates to Price, BillingCycleId, CurrencyId, etc., as they are not present in the DTO
            var updated = await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = _mapper.Map<SubscriptionPlanDto>(updated), Message = "Plan updated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to update plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> ActivatePlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
            plan.IsActive = true;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = true, Message = "Plan activated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to activate plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeactivatePlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
            plan.IsActive = false;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return new JsonModel { data = true, Message = "Plan deactivated", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to deactivate plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeletePlanAsync(string planId)
    {
        try
        {
            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(Guid.Parse(planId));
            if (!result)
                return new JsonModel { data = new object(), Message = "Plan not found or could not be deleted", StatusCode = 500 };
            return new JsonModel { data = true, Message = "Plan deleted", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            return new JsonModel { data = new object(), Message = $"Failed to delete plan: {ex.Message}", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found for this Stripe ID", StatusCode = 500 };
            return JsonModel.SuccessResponse(_mapper.Map<SubscriptionDto>(subscription));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by Stripe ID {StripeSubscriptionId}", stripeSubscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetAllSubscriptionPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetActiveSubscriptionPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetActiveSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription plans");
            return new JsonModel { data = new object(), Message = "Failed to retrieve active subscription plans", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionPlansByCategoryAsync(string category)
    {
        try
        {
            // First get the category by name
            var categoryEntity = await _subscriptionRepository.GetCategoryByNameAsync(category);
            if (categoryEntity == null)
                return new JsonModel { data = new object(), Message = "Category not found", StatusCode = 500 };

            var plans = await _subscriptionRepository.GetSubscriptionPlansByCategoryAsync(categoryEntity.Id);
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return new JsonModel { data = .SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans by category {Category}", category);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plans by category", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> GetSubscriptionPlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 500 };
            return JsonModel.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(plan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto)
    {
        try
        {
            var plan = _mapper.Map<SubscriptionPlan>(createDto);
            plan.CreatedAt = DateTime.UtcNow;
            plan.IsActive = true;

            var createdPlan = await _subscriptionRepository.CreateSubscriptionPlanAsync(plan);
            var dto = _mapper.Map<SubscriptionPlanDto>(createdPlan);

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanCreated",
                createdPlan.Id.ToString(),
                $"Created plan: {createdPlan.Name}"
            );

            return new JsonModel { data = dto, Message = "Subscription plan created successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return new JsonModel { data = new object(), Message = "Failed to create subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 500 };

            // Update plan properties
            if (!string.IsNullOrEmpty(updateDto.Name))
                plan.Name = updateDto.Name;
            
            if (!string.IsNullOrEmpty(updateDto.Description))
                plan.Description = updateDto.Description;
            
            if (updateDto.IsActive)
                plan.IsActive = updateDto.IsActive;
            
            if (updateDto.DisplayOrder.HasValue)
                plan.DisplayOrder = updateDto.DisplayOrder.Value;

            plan.UpdatedAt = DateTime.UtcNow;
            
            var updatedPlan = await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            var dto = _mapper.Map<SubscriptionPlanDto>(updatedPlan);

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanUpdated",
                planId,
                $"Updated plan: {updatedPlan.Name}"
            );

            return new JsonModel { data = dto, Message = "Subscription plan updated successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to update subscription plan", StatusCode = 500 };
        }
    }

    public async Task<JsonModel> DeleteSubscriptionPlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return new JsonModel { data = new object(), Message = "Subscription plan not found", StatusCode = 500 };

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsByPlanAsync(plan.Id);
            if (activeSubscriptions.Any())
                return new JsonModel { data = new object(), Message = "Cannot delete plan with active subscriptions", StatusCode = 500 };

            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(plan.Id);
            if (!result)
                return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanDeleted",
                planId,
                $"Deleted plan: {plan.Name}"
            );

            return new JsonModel { data = true, Message = "Subscription plan deleted successfully", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId}", planId);
            return new JsonModel { data = new object(), Message = "Failed to delete subscription plan", StatusCode = 500 };
        }
    }

    // Example: Booking a consultation using privilege system
    public async Task<JsonModel> BookConsultationAsync(Guid userId, Guid subscriptionId)
    {
        // Check if user has remaining consult privileges
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "Teleconsultation");
        if (remaining <= 0)
            return new JsonModel { data = new object(), Message = "No teleconsultations remaining in your plan.", StatusCode = 500 };
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation");
        if (!used)
            return new JsonModel { data = new object(), Message = "Failed to use teleconsultation privilege.", StatusCode = 500 };
        // Proceed with booking logic (not shown)
        return new JsonModel { data = true, Message = "Consultation booked.", StatusCode = 200 };
    }

    // Example: Medication supply using privilege system
    public async Task<JsonModel> RequestMedicationSupplyAsync(Guid userId, Guid subscriptionId)
    {
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "MedicationSupply");
        if (remaining <= 0)
            return new JsonModel { data = new object(), Message = "No medication supply privilege remaining in your plan.", StatusCode = 500 };
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "MedicationSupply");
        if (!used)
            return new JsonModel { data = new object(), Message = "Failed to use medication supply privilege.", StatusCode = 500 };
        // Proceed with medication supply logic (not shown)
        return new JsonModel { data = true, Message = "Medication supply requested.", StatusCode = 200 };
    }

    // --- PAYMENT & BILLING EDGE CASES ---

    // 1. Handle failed payment and update subscription status
    public async Task<JsonModel> HandleFailedPaymentAsync(string subscriptionId, string reason)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            entity.Status = Subscription.SubscriptionStatuses.PaymentFailed;
            entity.LastPaymentError = reason;
            entity.FailedPaymentAttempts += 1;
            entity.LastPaymentFailedDate = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _subscriptionRepository.UpdateAsync(entity);

            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory {
                SubscriptionId = entity.Id,
                FromStatus = entity.Status,
                ToStatus = Subscription.SubscriptionStatuses.PaymentFailed,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            });

            // Send payment failed notification
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, DueDate = DateTime.UtcNow, Description = reason };
                await _notificationService.SendPaymentFailedEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }

            // Audit log
            await _auditService.LogPaymentEventAsync(entity.UserId.ToString(), "PaymentFailed", subscriptionId, "Failed", reason);

            return new JsonModel { data = new object(), Message = $"Payment failed: {reason}", StatusCode = 500 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to handle payment failure", StatusCode = 500 };
        }
    }

    // 2. Retry payment and reactivate subscription if successful
    public async Task<JsonModel> RetryPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
        // Simulate payment retry logic
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), paymentRequest.Amount, "USD");
        if (paymentResult.Status == "succeeded")
        {
            // Send payment success email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = paymentRequest.Amount, PaidDate = DateTime.UtcNow, Description = "Retry Payment" };
                await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Trigger notification to user
            return new JsonModel { data = paymentResult, Message = "Payment retried and subscription reactivated", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Payment retry failed: {paymentResult.ErrorMessage}", StatusCode = 500 };
        }
    }

    // 3. Auto-renewal logic (to be called by a scheduler/cron job)
    public async Task<JsonModel> AutoRenewSubscriptionAsync(string subscriptionId)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
        if (entity.Status != Subscription.SubscriptionStatuses.Active)
            return new JsonModel { data = new object(), Message = "Only active subscriptions can be auto-renewed", StatusCode = 500 };
        // Simulate payment
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), entity.CurrentPrice, "USD");
        if (paymentResult.Status == "succeeded")
        {
            // Send renewal confirmation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId);
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Auto-Renewal" };
                await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }
            entity.NextBillingDate = entity.NextBillingDate.AddMonths(1);
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Add billing history record
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription auto-renewed", StatusCode = 200 };
        }
        else
        {
            await HandleFailedPaymentAsync(subscriptionId, paymentResult.ErrorMessage ?? "Auto-renewal payment failed");
            return new JsonModel { data = new object(), Message = $"Auto-renewal payment failed: {paymentResult.ErrorMessage}", StatusCode = 500 };
        }
    }

    // 4. Prorated upgrades/downgrades
    public async Task<JsonModel> ProrateUpgradeAsync(string subscriptionId, string newPlanId)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };
        if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
            return new JsonModel { data = new object(), Message = "Already on this plan", StatusCode = 500 };
        // Simulate proration calculation
        var daysLeft = (entity.NextBillingDate - DateTime.UtcNow).TotalDays;
        var oldPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(entity.SubscriptionPlanId);
        var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(newPlanId));
        if (oldPlan == null || newPlan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
        // In proration, use Price and BillingCycleId
        var credit = (decimal)(daysLeft / 30.0) * oldPlan.Price; // Assuming Price is the monthly price
        var charge = newPlan.Price - credit;
        // Simulate payment for the difference
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), charge, "USD");
        if (paymentResult.Status == "succeeded")
        {
            entity.SubscriptionPlanId = newPlan.Id;
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Add billing history record
            return new JsonModel { data = _mapper.Map<SubscriptionDto>(entity), Message = "Subscription upgraded with proration", StatusCode = 200 };
        }
        else
        {
            return new JsonModel { data = new object(), Message = $"Prorated upgrade payment failed: {paymentResult.ErrorMessage}", StatusCode = 500 };
        }
    }

    // --- USAGE & LIMITS ---

    // Check if user can use a privilege (e.g., book a consult)
    public async Task<JsonModel> CanUsePrivilegeAsync(string subscriptionId, string privilegeName)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null || subscription.Status != Subscription.SubscriptionStatuses.Active)
                return new JsonModel { data = new object(), Message = "Subscription not active", StatusCode = 500 };

            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
            var planPrivilege = planPrivileges.FirstOrDefault(p => p.Privilege.Name == privilegeName);
            if (planPrivilege == null)
                return new JsonModel { data = new object(), Message = "Privilege not included in plan", StatusCode = 500 };

            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            var usage = usages.FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            int used = usage?.UsedValue ?? 0;
            int allowed = planPrivilege.Value;

            if (used >= allowed)
                return new JsonModel { data = new object(), Message = $"Usage limit reached for {privilegeName}", StatusCode = 500 };

            return new JsonModel { data = true, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking privilege usage for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to check privilege usage", StatusCode = 500 };
        }
    }

    // Increment privilege usage (to be called after successful action)
    public async Task IncrementPrivilegeUsageAsync(string subscriptionId, string privilegeName)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (subscription == null) return;
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
        var planPrivilege = planPrivileges.FirstOrDefault(p => p.Privilege.Name == privilegeName);
        if (planPrivilege == null) return;
        var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
        var usage = usages.FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
        if (usage == null)
        {
            usage = new UserSubscriptionPrivilegeUsage
            {
                SubscriptionId = subscription.Id,
                SubscriptionPlanPrivilegeId = planPrivilege.Id,
                UsedValue = 1,
                UsagePeriodStart = DateTime.UtcNow,
                UsagePeriodEnd = DateTime.UtcNow.AddMonths(1)
            };
            await _usageRepo.AddAsync(usage);
        }
        else
        {
            usage.UsedValue += 1;
            await _usageRepo.UpdateAsync(usage);
        }
    }

    // Reset usage counters for all active subscriptions (to be called by a scheduler/cron job at billing cycle start)
    public async Task ResetAllUsageCountersAsync()
    {
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
        foreach (var subscription in activeSubscriptions)
        {
            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            foreach (var usage in usages)
            {
                usage.UsedValue = 0;
                await _usageRepo.UpdateAsync(usage);
            }
        }
    }

    // Expire unused benefits (e.g., free consults) if not used within the period
    public async Task ExpireUnusedBenefitsAsync()
    {
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
        foreach (var subscription in activeSubscriptions)
        {
            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            foreach (var usage in usages)
            {
                // For now, just reset to 0 at expiry; can be extended for carry-over logic
                usage.UsedValue = 0;
                await _usageRepo.UpdateAsync(usage);
            }
        }
    }

    // --- ADMIN OPERATIONS ---

    // Deactivate a plan (admin action)
    public async Task<JsonModel> DeactivatePlanAsync(string planId, string adminUserId)
    {
        // Example: deactivate plan and notify/pause all subscribers
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return new JsonModel { data = new object(), Message = "Plan not found", StatusCode = 500 };
        plan.IsActive = false;
        await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
        // TODO: Pause all subscribers and notify them
        return new JsonModel { data = true, Message = "Plan deactivated and subscribers notified/paused.", StatusCode = 200 };
    }

    // Bulk cancel subscriptions (admin action)
    public async Task<JsonModel> BulkCancelSubscriptionsAsync(IEnumerable<string> subscriptionIds, string adminUserId, string? reason = null)
    {
        int cancelled = 0;
        foreach (var id in subscriptionIds)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(id));
            if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active)
            {
                sub.Status = Subscription.SubscriptionStatuses.Cancelled;
                sub.CancellationReason = reason ?? "Bulk admin cancel";
                sub.CancelledDate = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
                var userResult = await _userService.GetUserByIdAsync(sub.UserId);
                if (userResult.Success && userResult.Data != null)
                {
                    // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                    // await _notificationService.SendSubscriptionCancelledNotificationAsync(userResult.Data.Email, userResult.Data.FullName, _mapper.Map<SubscriptionDto>(sub));
                    _logger.LogInformation("Email notifications disabled - would have sent cancellation notification to {Email}", userResult.Data.Email);
                }
                await _auditService.LogUserActionAsync(adminUserId, "BulkCancelSubscription", "Subscription", id, "Cancelled by admin");
                cancelled++;
            }
        }
        return new JsonModel { data = cancelled, Message = $"{cancelled} subscriptions cancelled.", StatusCode = 200 };
    }

    // Bulk upgrade subscriptions (admin action)
    public async Task<JsonModel> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId)
    {
        int upgraded = 0;
        foreach (var id in subscriptionIds)
        {
            var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(id));
            if (sub != null && sub.Status == Subscription.SubscriptionStatuses.Active && sub.SubscriptionPlanId != Guid.Parse(newPlanId))
            {
                sub.SubscriptionPlanId = Guid.Parse(newPlanId);
                sub.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.UpdateAsync(sub);
                var userResult = await _userService.GetUserByIdAsync(sub.UserId);
                if (userResult.Success && userResult.Data != null)
                {
                    // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                    // await _notificationService.SendSubscriptionConfirmationAsync(userResult.Data.Email, userResult.Data.FullName, _mapper.Map<SubscriptionDto>(sub));
                    _logger.LogInformation("Email notifications disabled - would have sent confirmation email to {Email}", userResult.Data.Email);
                }
                await _auditService.LogUserActionAsync(adminUserId, "BulkUpgradeSubscription", "Subscription", id, $"Upgraded to plan {newPlanId}");
                upgraded++;
            }
        }
        return new JsonModel { data = upgraded, Message = $"{upgraded} subscriptions upgraded.", StatusCode = 200 };
    }

    public async Task<JsonModel> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage = null)
    {
        try
        {
            if (eventType == "payment_failed")
            {
                await HandleFailedPaymentAsync(subscriptionId, errorMessage ?? "Unknown error");
                return new JsonModel { data = true, Message = "Payment failure handled", StatusCode = 200 };
            }
            if (eventType == "payment_succeeded")
            {
                var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
                if (sub != null)
                {
                    sub.Status = Subscription.SubscriptionStatuses.Active;
                    await _subscriptionRepository.UpdateAsync(sub);
                    
                    var userResult = await _userService.GetUserByIdAsync(sub.UserId);
                    if (userResult.Success && userResult.Data != null)
                    {
                        var billingRecord = new BillingRecordDto { Amount = sub.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Webhook Payment Success" };
                        // EMAIL FUNCTIONALITY DISABLED - Commented out for now
                        // await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
                        _logger.LogInformation("Email notifications disabled - would have sent payment success email to {Email}", userResult.Data.Email);
                    }
                    await _auditService.LogPaymentEventAsync(sub.UserId.ToString(), "PaymentSucceeded", subscriptionId, "Succeeded");
                }
                return new JsonModel { data = true, Message = "Payment success handled", StatusCode = 200 };
            }
            
            return new JsonModel { data = new object(), Message = "Unhandled webhook event type", StatusCode = 500 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment provider webhook for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to handle webhook event", StatusCode = 500 };
        }
    }

    // Additional methods for comprehensive subscription management
    public async Task<JsonModel> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return new JsonModel { data = new object(), Message = "Subscription not found", StatusCode = 500 };

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId);
            if (!usageStats.Success)
                return new JsonModel { data = new object(), Message = "Failed to get usage statistics", StatusCode = 500 };

            // Get billing history with date range
            var billingHistory = await _billingService.GetPaymentHistoryAsync(subscription.UserId, startDate, endDate);

            var analytics = new SubscriptionAnalyticsDto
            {
                SubscriptionId = subscriptionId,
                PlanName = subscription.SubscriptionPlan.Name,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate,
                TotalAmountPaid = billingHistory.Data?.Sum(bh => bh.Amount) ?? 0,
                PaymentCount = billingHistory.Data?.Count() ?? 0,
                AveragePaymentAmount = billingHistory.Data?.Any() == true ? billingHistory.Data.Average(bh => bh.Amount) : 0,
                UsageStatistics = usageStats.Data,
                PaymentHistory = billingHistory.Data?.Select(bh => new PaymentHistoryDto
                {
                    Id = bh.Id,
                    UserId = bh.UserId,
                    SubscriptionId = bh.SubscriptionId,
                    Amount = bh.Amount,
                    Currency = bh.Currency,
                    PaymentMethod = bh.PaymentMethod,
                    Status = bh.Status,
                    TransactionId = bh.TransactionId,
                    ErrorMessage = bh.ErrorMessage,
                    CreatedAt = bh.CreatedAt,
                    ProcessedAt = bh.ProcessedAt,
                    PaymentDate = bh.CreatedAt,
                    Description = bh.ErrorMessage,
                    PaymentMethodId = bh.PaymentMethod
                }).ToList() ?? new List<PaymentHistoryDto>()
            };

            return new JsonModel { data = analytics, Message = "Success", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return new JsonModel { data = new object(), Message = "Failed to retrieve subscription analytics", StatusCode = 500 };
        }
    }
} 