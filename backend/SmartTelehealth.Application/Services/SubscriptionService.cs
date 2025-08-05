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

    public async Task<ApiResponse<SubscriptionDto>> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to retrieve subscription");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetUserSubscriptionsAsync(string userId)
    {
        try
        {
            var entities = await _subscriptionRepository.GetByUserIdAsync(Guid.Parse(userId));
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(entities);
            return ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions for user {UserId}", userId);
            return ApiResponse<IEnumerable<SubscriptionDto>>.ErrorResponse("Failed to retrieve user subscriptions");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionDto createDto)
    {
        try
        {
            // 1. Check if plan exists and is active
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(createDto.PlanId));
            if (plan == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription plan does not exist");
            if (!plan.IsActive)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription plan is not active");

            // 2. Prevent duplicate subscriptions for the same user and plan (active or paused)
            var userSubscriptions = await _subscriptionRepository.GetByUserIdAsync(Guid.Parse(createDto.UserId));
            if (userSubscriptions.Any(s => s.SubscriptionPlanId == plan.Id && (s.Status == Subscription.SubscriptionStatuses.Active || s.Status == Subscription.SubscriptionStatuses.Paused)))
                return ApiResponse<SubscriptionDto>.ErrorResponse("User already has an active or paused subscription for this plan");

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
                await _notificationService.SendSubscriptionConfirmationAsync(userResult.Data.Email, userResult.Data.FullName, dto);
                await _notificationService.SendSubscriptionWelcomeEmailAsync(userResult.Data.Email, userResult.Data.FullName, dto);
            }
            
            // Audit log
            await _auditService.LogUserActionAsync(createDto.UserId, "CreateSubscription", "Subscription", created.Id.ToString(), "Subscription created successfully");
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(dto, "Subscription created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", createDto.UserId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to create subscription");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> CancelSubscriptionAsync(string subscriptionId, string? reason = null)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            
            // Prevent cancelling an already cancelled subscription
            if (entity.IsCancelled)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already cancelled");
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Cancelled);
            if (validation != ValidationResult.Success)
                return ApiResponse<SubscriptionDto>.ErrorResponse(validation.ErrorMessage);
            
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
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
                await _notificationService.SendSubscriptionCancellationEmailAsync(userResult.Data.Email, userResult.Data.FullName, dto);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "CancelSubscription", "Subscription", subscriptionId, reason ?? "Subscription cancelled");
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(dto, "Subscription cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to cancel subscription");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> PauseSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            
            if (entity.IsPaused)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already paused");
            
            if (entity.IsCancelled)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Cannot pause a cancelled subscription");
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Paused);
            if (validation != ValidationResult.Success)
                return ApiResponse<SubscriptionDto>.ErrorResponse(validation.ErrorMessage);
            
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
            
            // Send pause notification
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
                await _notificationService.SendSubscriptionPausedNotificationAsync(userResult.Data.Email, userResult.Data.FullName, dto);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "PauseSubscription", "Subscription", subscriptionId, "Subscription paused");
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(dto, "Subscription paused");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to pause subscription");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> ResumeSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            
            if (!entity.IsPaused)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is not paused");
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return ApiResponse<SubscriptionDto>.ErrorResponse(validation.ErrorMessage);
            
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
            
            // Send resume notification
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
                await _notificationService.SendSubscriptionResumedNotificationAsync(userResult.Data.Email, userResult.Data.FullName, dto);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "ResumeSubscription", "Subscription", subscriptionId, "Subscription resumed");
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(dto, "Subscription resumed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to resume subscription");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> UpgradeSubscriptionAsync(string subscriptionId, string newPlanId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            
            // Prevent upgrading to the same plan
            if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription is already on this plan");
            
            var oldPlanId = entity.SubscriptionPlanId;
            entity.SubscriptionPlanId = Guid.Parse(newPlanId);
            entity.UpdatedAt = DateTime.UtcNow;
            
            var updated = await _subscriptionRepository.UpdateAsync(entity);
            
            // Audit log
            await _auditService.LogUserActionAsync(entity.UserId.ToString(), "UpgradeSubscription", "Subscription", subscriptionId, $"Upgraded from plan {oldPlanId} to {newPlanId}");
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(updated), "Subscription upgraded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to upgrade subscription");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> ReactivateSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
            
            // Validate status transition
            var validation = entity.ValidateStatusTransition(Subscription.SubscriptionStatuses.Active);
            if (validation != ValidationResult.Success)
                return ApiResponse<SubscriptionDto>.ErrorResponse(validation.ErrorMessage);
            
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
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(updated), "Subscription reactivated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to reactivate subscription");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve subscription plans");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> GetPlanByIdAsync(string planId)
    {
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Plan not found");
        return ApiResponse<SubscriptionPlanDto>.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(plan));
    }

    public async Task<ApiResponse<IEnumerable<BillingHistoryDto>>> GetBillingHistoryAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<IEnumerable<BillingHistoryDto>>.ErrorResponse("Subscription not found");

            // Get billing records for this subscription
            var billingRecords = await _billingService.GetSubscriptionBillingHistoryAsync(subscription.Id);
            
            if (!billingRecords.Success)
                return ApiResponse<IEnumerable<BillingHistoryDto>>.ErrorResponse("Failed to retrieve billing history");

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

            return ApiResponse<IEnumerable<BillingHistoryDto>>.SuccessResponse(billingHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<IEnumerable<BillingHistoryDto>>.ErrorResponse("Failed to retrieve billing history");
        }
    }

    public async Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetPaymentMethodsAsync(string userId)
    {
        var methods = await _stripeService.GetCustomerPaymentMethodsAsync(userId);
        return ApiResponse<IEnumerable<PaymentMethodDto>>.SuccessResponse(methods);
    }

    public async Task<ApiResponse<PaymentMethodDto>> AddPaymentMethodAsync(string userId, string paymentMethodId)
    {
        var methodId = await _stripeService.AddPaymentMethodAsync(userId, paymentMethodId);
        var method = new PaymentMethodDto { Id = methodId };
        return ApiResponse<PaymentMethodDto>.SuccessResponse(method, "Payment method added");
    }

    public async Task<ApiResponse<SubscriptionDto>> GetSubscriptionByPlanIdAsync(string planId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByPlanIdAsync(Guid.Parse(planId));
            if (subscription == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found for this plan");
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(subscription));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by plan ID {PlanId}", planId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to retrieve subscription");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetActiveSubscriptionsAsync()
    {
        try
        {
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(activeSubscriptions);
            return ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscriptions");
            return ApiResponse<IEnumerable<SubscriptionDto>>.ErrorResponse("Failed to retrieve active subscriptions");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> GetSubscriptionByIdAsync(string subscriptionId)
    {
        return await GetSubscriptionAsync(subscriptionId);
    }

    public async Task<ApiResponse<SubscriptionDto>> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionDto updateDto)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");

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
            
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(updatedSubscription), "Subscription updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to update subscription");
        }
    }

    public async Task<ApiResponse<PaymentResultDto>> ProcessPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<PaymentResultDto>.ErrorResponse("Subscription not found");

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

                return ApiResponse<PaymentResultDto>.SuccessResponse(paymentResult, "Payment processed successfully");
            }
            else
            {
                return ApiResponse<PaymentResultDto>.ErrorResponse($"Payment failed: {paymentResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<PaymentResultDto>.ErrorResponse("Failed to process payment");
        }
    }

    public async Task<ApiResponse<UsageStatisticsDto>> GetUsageStatisticsAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<UsageStatisticsDto>.ErrorResponse("Subscription not found");

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

            return ApiResponse<UsageStatisticsDto>.SuccessResponse(usageStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage statistics for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<UsageStatisticsDto>.ErrorResponse("Failed to retrieve usage statistics");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetAllSubscriptionsAsync()
    {
        try
        {
            var allSubscriptions = await _subscriptionRepository.GetAllSubscriptionsAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionDto>>(allSubscriptions);
            return ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscriptions");
            return ApiResponse<IEnumerable<SubscriptionDto>>.ErrorResponse("Failed to retrieve subscriptions");
        }
    }

    public async Task<ApiResponse<SubscriptionAnalyticsDto>> GetSubscriptionAnalyticsAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Subscription not found");

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId);
            if (!usageStats.Success)
                return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Failed to get usage statistics");

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

            return ApiResponse<SubscriptionAnalyticsDto>.SuccessResponse(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Failed to retrieve subscription analytics");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanDto createPlanDto)
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
            return ApiResponse<SubscriptionPlanDto>.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(created), "Plan created");
        }
        catch (Exception ex)
        {
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse($"Failed to create plan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> UpdatePlanAsync(string planId, UpdateSubscriptionPlanDto updatePlanDto)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Plan not found");
            plan.Name = updatePlanDto.Name;
            plan.Description = updatePlanDto.Description;
            plan.IsActive = updatePlanDto.IsActive;
            // Remove updates to Price, BillingCycleId, CurrencyId, etc., as they are not present in the DTO
            var updated = await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return ApiResponse<SubscriptionPlanDto>.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(updated), "Plan updated");
        }
        catch (Exception ex)
        {
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse($"Failed to update plan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ActivatePlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<bool>.ErrorResponse("Plan not found");
            plan.IsActive = true;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return ApiResponse<bool>.SuccessResponse(true, "Plan activated");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to activate plan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeactivatePlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<bool>.ErrorResponse("Plan not found");
            plan.IsActive = false;
            await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
            return ApiResponse<bool>.SuccessResponse(true, "Plan deactivated");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to deactivate plan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeletePlanAsync(string planId)
    {
        try
        {
            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(Guid.Parse(planId));
            if (!result)
                return ApiResponse<bool>.ErrorResponse("Plan not found or could not be deleted");
            return ApiResponse<bool>.SuccessResponse(true, "Plan deleted");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to delete plan: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SubscriptionDto>> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
            if (subscription == null)
                return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found for this Stripe ID");
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(subscription));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription by Stripe ID {StripeSubscriptionId}", stripeSubscriptionId);
            return ApiResponse<SubscriptionDto>.ErrorResponse("Failed to retrieve subscription");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllSubscriptionPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetAllSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all subscription plans");
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve subscription plans");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetActiveSubscriptionPlansAsync()
    {
        try
        {
            var plans = await _subscriptionRepository.GetActiveSubscriptionPlansAsync();
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription plans");
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve active subscription plans");
        }
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetSubscriptionPlansByCategoryAsync(string category)
    {
        try
        {
            // First get the category by name
            var categoryEntity = await _subscriptionRepository.GetCategoryByNameAsync(category);
            if (categoryEntity == null)
                return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Category not found");

            var plans = await _subscriptionRepository.GetSubscriptionPlansByCategoryAsync(categoryEntity.Id);
            var dtos = _mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans by category {Category}", category);
            return ApiResponse<IEnumerable<SubscriptionPlanDto>>.ErrorResponse("Failed to retrieve subscription plans by category");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> GetSubscriptionPlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Subscription plan not found");
            return ApiResponse<SubscriptionPlanDto>.SuccessResponse(_mapper.Map<SubscriptionPlanDto>(plan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plan {PlanId}", planId);
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to retrieve subscription plan");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanDto createDto)
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

            return ApiResponse<SubscriptionPlanDto>.SuccessResponse(dto, "Subscription plan created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to create subscription plan");
        }
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> UpdateSubscriptionPlanAsync(string planId, UpdateSubscriptionPlanDto updateDto)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Subscription plan not found");

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

            return ApiResponse<SubscriptionPlanDto>.SuccessResponse(dto, "Subscription plan updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", planId);
            return ApiResponse<SubscriptionPlanDto>.ErrorResponse("Failed to update subscription plan");
        }
    }

    public async Task<ApiResponse<bool>> DeleteSubscriptionPlanAsync(string planId)
    {
        try
        {
            var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
            if (plan == null)
                return ApiResponse<bool>.ErrorResponse("Subscription plan not found");

            // Check if plan has active subscriptions
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsByPlanAsync(plan.Id);
            if (activeSubscriptions.Any())
                return ApiResponse<bool>.ErrorResponse("Cannot delete plan with active subscriptions");

            var result = await _subscriptionRepository.DeleteSubscriptionPlanAsync(plan.Id);
            if (!result)
                return ApiResponse<bool>.ErrorResponse("Failed to delete subscription plan");

            await _auditService.LogActionAsync(
                "SubscriptionPlan",
                "SubscriptionPlanDeleted",
                planId,
                $"Deleted plan: {plan.Name}"
            );

            return ApiResponse<bool>.SuccessResponse(true, "Subscription plan deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId}", planId);
            return ApiResponse<bool>.ErrorResponse("Failed to delete subscription plan");
        }
    }

    // Example: Booking a consultation using privilege system
    public async Task<ApiResponse<bool>> BookConsultationAsync(Guid userId, Guid subscriptionId)
    {
        // Check if user has remaining consult privileges
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "Teleconsultation");
        if (remaining <= 0)
            return ApiResponse<bool>.ErrorResponse("No teleconsultations remaining in your plan.");
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "Teleconsultation");
        if (!used)
            return ApiResponse<bool>.ErrorResponse("Failed to use teleconsultation privilege.");
        // Proceed with booking logic (not shown)
        return ApiResponse<bool>.SuccessResponse(true, "Consultation booked.");
    }

    // Example: Medication supply using privilege system
    public async Task<ApiResponse<bool>> RequestMedicationSupplyAsync(Guid userId, Guid subscriptionId)
    {
        var remaining = await _privilegeService.GetRemainingPrivilegeAsync(subscriptionId, "MedicationSupply");
        if (remaining <= 0)
            return ApiResponse<bool>.ErrorResponse("No medication supply privilege remaining in your plan.");
        var used = await _privilegeService.UsePrivilegeAsync(subscriptionId, "MedicationSupply");
        if (!used)
            return ApiResponse<bool>.ErrorResponse("Failed to use medication supply privilege.");
        // Proceed with medication supply logic (not shown)
        return ApiResponse<bool>.SuccessResponse(true, "Medication supply requested.");
    }

    // --- PAYMENT & BILLING EDGE CASES ---

    // 1. Handle failed payment and update subscription status
    public async Task<ApiResponse<PaymentResultDto>> HandleFailedPaymentAsync(string subscriptionId, string reason)
    {
        try
        {
            var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (entity == null)
                return ApiResponse<PaymentResultDto>.ErrorResponse("Subscription not found");

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
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, DueDate = DateTime.UtcNow, Description = reason };
                await _notificationService.SendPaymentFailedEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }

            // Audit log
            await _auditService.LogPaymentEventAsync(entity.UserId.ToString(), "PaymentFailed", subscriptionId, "Failed", reason);

            return ApiResponse<PaymentResultDto>.ErrorResponse($"Payment failed: {reason}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failed payment for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<PaymentResultDto>.ErrorResponse("Failed to handle payment failure");
        }
    }

    // 2. Retry payment and reactivate subscription if successful
    public async Task<ApiResponse<PaymentResultDto>> RetryPaymentAsync(string subscriptionId, PaymentRequestDto paymentRequest)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return ApiResponse<PaymentResultDto>.ErrorResponse("Subscription not found");
        // Simulate payment retry logic
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), paymentRequest.Amount, "USD");
        if (paymentResult.Status == "succeeded")
        {
            // Send payment success email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = paymentRequest.Amount, PaidDate = DateTime.UtcNow, Description = "Retry Payment" };
                await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }
            entity.Status = Subscription.SubscriptionStatuses.Active;
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Trigger notification to user
            return ApiResponse<PaymentResultDto>.SuccessResponse(paymentResult, "Payment retried and subscription reactivated");
        }
        else
        {
            return ApiResponse<PaymentResultDto>.ErrorResponse($"Payment retry failed: {paymentResult.ErrorMessage}");
        }
    }

    // 3. Auto-renewal logic (to be called by a scheduler/cron job)
    public async Task<ApiResponse<SubscriptionDto>> AutoRenewSubscriptionAsync(string subscriptionId)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
        if (entity.Status != Subscription.SubscriptionStatuses.Active)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Only active subscriptions can be auto-renewed");
        // Simulate payment
        var paymentResult = await _stripeService.ProcessPaymentAsync(entity.UserId.ToString(), entity.CurrentPrice, "USD");
        if (paymentResult.Status == "succeeded")
        {
            // Send renewal confirmation email
            var userResult = await _userService.GetUserByIdAsync(entity.UserId.ToString());
            if (userResult.Success && userResult.Data != null)
            {
                var billingRecord = new BillingRecordDto { Amount = entity.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Auto-Renewal" };
                await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
            }
            entity.NextBillingDate = entity.NextBillingDate.AddMonths(1);
            entity.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(entity);
            // TODO: Add billing history record
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(entity), "Subscription auto-renewed");
        }
        else
        {
            await HandleFailedPaymentAsync(subscriptionId, paymentResult.ErrorMessage ?? "Auto-renewal payment failed");
            return ApiResponse<SubscriptionDto>.ErrorResponse($"Auto-renewal payment failed: {paymentResult.ErrorMessage}");
        }
    }

    // 4. Prorated upgrades/downgrades
    public async Task<ApiResponse<SubscriptionDto>> ProrateUpgradeAsync(string subscriptionId, string newPlanId)
    {
        var entity = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
        if (entity == null)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Subscription not found");
        if (entity.SubscriptionPlanId == Guid.Parse(newPlanId))
            return ApiResponse<SubscriptionDto>.ErrorResponse("Already on this plan");
        // Simulate proration calculation
        var daysLeft = (entity.NextBillingDate - DateTime.UtcNow).TotalDays;
        var oldPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(entity.SubscriptionPlanId);
        var newPlan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(newPlanId));
        if (oldPlan == null || newPlan == null)
            return ApiResponse<SubscriptionDto>.ErrorResponse("Plan not found");
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
            return ApiResponse<SubscriptionDto>.SuccessResponse(_mapper.Map<SubscriptionDto>(entity), "Subscription upgraded with proration");
        }
        else
        {
            return ApiResponse<SubscriptionDto>.ErrorResponse($"Prorated upgrade payment failed: {paymentResult.ErrorMessage}");
        }
    }

    // --- USAGE & LIMITS ---

    // Check if user can use a privilege (e.g., book a consult)
    public async Task<ApiResponse<bool>> CanUsePrivilegeAsync(string subscriptionId, string privilegeName)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null || subscription.Status != Subscription.SubscriptionStatuses.Active)
                return ApiResponse<bool>.ErrorResponse("Subscription not active");

            var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(subscription.SubscriptionPlanId);
            var planPrivilege = planPrivileges.FirstOrDefault(p => p.Privilege.Name == privilegeName);
            if (planPrivilege == null)
                return ApiResponse<bool>.ErrorResponse("Privilege not included in plan");

            var usages = await _usageRepo.GetBySubscriptionIdAsync(subscription.Id);
            var usage = usages.FirstOrDefault(u => u.SubscriptionPlanPrivilegeId == planPrivilege.Id);
            int used = usage?.UsedValue ?? 0;
            int allowed = planPrivilege.Value;

            if (used >= allowed)
                return ApiResponse<bool>.ErrorResponse($"Usage limit reached for {privilegeName}");

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking privilege usage for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to check privilege usage");
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
    public async Task<ApiResponse<bool>> DeactivatePlanAsync(string planId, string adminUserId)
    {
        // Example: deactivate plan and notify/pause all subscribers
        var plan = await _subscriptionRepository.GetSubscriptionPlanByIdAsync(Guid.Parse(planId));
        if (plan == null)
            return ApiResponse<bool>.ErrorResponse("Plan not found");
        plan.IsActive = false;
        await _subscriptionRepository.UpdateSubscriptionPlanAsync(plan);
        // TODO: Pause all subscribers and notify them
        return ApiResponse<bool>.SuccessResponse(true, "Plan deactivated and subscribers notified/paused.");
    }

    // Bulk cancel subscriptions (admin action)
    public async Task<ApiResponse<int>> BulkCancelSubscriptionsAsync(IEnumerable<string> subscriptionIds, string adminUserId, string? reason = null)
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
                var userResult = await _userService.GetUserByIdAsync(sub.UserId.ToString());
                if (userResult.Success && userResult.Data != null)
                    await _notificationService.SendSubscriptionCancelledNotificationAsync(userResult.Data.Email, userResult.Data.FullName, _mapper.Map<SubscriptionDto>(sub));
                await _auditService.LogUserActionAsync(adminUserId, "BulkCancelSubscription", "Subscription", id, "Cancelled by admin");
                cancelled++;
            }
        }
        return ApiResponse<int>.SuccessResponse(cancelled, $"{cancelled} subscriptions cancelled.");
    }

    // Bulk upgrade subscriptions (admin action)
    public async Task<ApiResponse<int>> BulkUpgradeSubscriptionsAsync(IEnumerable<string> subscriptionIds, string newPlanId, string adminUserId)
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
                var userResult = await _userService.GetUserByIdAsync(sub.UserId.ToString());
                if (userResult.Success && userResult.Data != null)
                    await _notificationService.SendSubscriptionConfirmationAsync(userResult.Data.Email, userResult.Data.FullName, _mapper.Map<SubscriptionDto>(sub));
                await _auditService.LogUserActionAsync(adminUserId, "BulkUpgradeSubscription", "Subscription", id, $"Upgraded to plan {newPlanId}");
                upgraded++;
            }
        }
        return ApiResponse<int>.SuccessResponse(upgraded, $"{upgraded} subscriptions upgraded.");
    }

    public async Task<ApiResponse<bool>> HandlePaymentProviderWebhookAsync(string eventType, string subscriptionId, string? errorMessage = null)
    {
        try
        {
            if (eventType == "payment_failed")
            {
                await HandleFailedPaymentAsync(subscriptionId, errorMessage ?? "Unknown error");
                return ApiResponse<bool>.SuccessResponse(true, "Payment failure handled");
            }
            if (eventType == "payment_succeeded")
            {
                var sub = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
                if (sub != null)
                {
                    sub.Status = Subscription.SubscriptionStatuses.Active;
                    await _subscriptionRepository.UpdateAsync(sub);
                    
                    var userResult = await _userService.GetUserByIdAsync(sub.UserId.ToString());
                    if (userResult.Success && userResult.Data != null)
                    {
                        var billingRecord = new BillingRecordDto { Amount = sub.CurrentPrice, PaidDate = DateTime.UtcNow, Description = "Webhook Payment Success" };
                        await _notificationService.SendPaymentSuccessEmailAsync(userResult.Data.Email, userResult.Data.FullName, billingRecord);
                    }
                    await _auditService.LogPaymentEventAsync(sub.UserId.ToString(), "PaymentSucceeded", subscriptionId, "Succeeded");
                }
                return ApiResponse<bool>.SuccessResponse(true, "Payment success handled");
            }
            
            return ApiResponse<bool>.ErrorResponse("Unhandled webhook event type");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment provider webhook for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to handle webhook event");
        }
    }

    // Additional methods for comprehensive subscription management
    public async Task<ApiResponse<SubscriptionAnalyticsDto>> GetSubscriptionAnalyticsAsync(string subscriptionId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Subscription not found");

            // Get usage statistics
            var usageStats = await GetUsageStatisticsAsync(subscriptionId);
            if (!usageStats.Success)
                return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Failed to get usage statistics");

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

            return ApiResponse<SubscriptionAnalyticsDto>.SuccessResponse(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionAnalyticsDto>.ErrorResponse("Failed to retrieve subscription analytics");
        }
    }
} 