using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Application.Services;

public class SubscriptionLifecycleService : ISubscriptionLifecycleService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionStatusHistoryRepository _statusHistoryRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionLifecycleService> _logger;

    public SubscriptionLifecycleService(
        ISubscriptionRepository subscriptionRepository,
        ISubscriptionStatusHistoryRepository statusHistoryRepository,
        IAuditService auditService,
        ILogger<SubscriptionLifecycleService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> ActivateSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Activating subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription activated",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully activated subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> PauseSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Pausing subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Paused"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Paused for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Paused";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Paused",
                Reason = reason ?? "Subscription paused",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully paused subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> ResumeSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Resuming subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription resumed",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully resumed subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Cancelling subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Cancelled"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Cancelled for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Cancelled";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Cancelled",
                Reason = reason ?? "Subscription cancelled",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully cancelled subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> SuspendSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Suspending subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Suspended"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Suspended for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Suspended";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Suspended",
                Reason = reason ?? "Subscription suspended",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully suspended subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> RenewSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = subscription.EndDate?.AddDays(subscription.BillingCycle.DurationInDays);
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Subscription renewed",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully renewed subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> ExpireSubscriptionAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Expiring subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Expired"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Expired for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Expired";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Expired",
                Reason = reason ?? "Subscription expired",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully expired subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> MarkPaymentFailedAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Marking payment failed for subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "PaymentFailed"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to PaymentFailed for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "PaymentFailed";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "PaymentFailed",
                Reason = reason ?? "Payment failed",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully marked payment failed for subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment failed for subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> MarkPaymentSucceededAsync(Guid subscriptionId, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Marking payment succeeded for subscription {SubscriptionId}", subscriptionId);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, "Active"))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to Active for subscription {SubscriptionId}", subscription.Status, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = "Active";
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = "Active",
                Reason = reason ?? "Payment succeeded",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully marked payment succeeded for subscription {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment succeeded for subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, string newStatus, string? reason = null)
    {
        try
        {
            _logger.LogInformation("Updating subscription {SubscriptionId} status to {NewStatus}", subscriptionId, newStatus);
            
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            if (!await ValidateStatusTransitionAsync(subscription.Status, newStatus))
            {
                _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for subscription {SubscriptionId}", subscription.Status, newStatus, subscriptionId);
                return false;
            }

            var oldStatus = subscription.Status;
            subscription.Status = newStatus;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Add status history
            await _statusHistoryRepository.CreateAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscriptionId,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                Reason = reason ?? $"Status updated to {newStatus}",
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);
            
            _logger.LogInformation("Successfully updated subscription {SubscriptionId} status to {NewStatus}", subscriptionId, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId} status to {NewStatus}", subscriptionId, newStatus);
            return false;
        }
    }

    public async Task<IEnumerable<SubscriptionStatusHistory>> GetStatusHistoryAsync(Guid subscriptionId)
    {
        try
        {
            return await _statusHistoryRepository.GetBySubscriptionIdAsync(subscriptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for subscription {SubscriptionId}", subscriptionId);
            return Enumerable.Empty<SubscriptionStatusHistory>();
        }
    }

    public async Task<bool> ValidateStatusTransitionAsync(string currentStatus, string newStatus)
    {
        try
        {
            var allowedTransitions = GetAllowedTransitions();
            
            if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                return allowedStates.Contains(newStatus);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating status transition from {CurrentStatus} to {NewStatus}", currentStatus, newStatus);
            return false;
        }
    }

    public async Task<string> GetNextValidStatusAsync(string currentStatus)
    {
        try
        {
            var allowedTransitions = GetAllowedTransitions();
            
            if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
            {
                return allowedStates.FirstOrDefault() ?? "Active";
            }

            return "Active";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next valid status for {CurrentStatus}", currentStatus);
            return "Active";
        }
    }

    /// <summary>
    /// Process subscription lifecycle state transitions
    /// </summary>
    public async Task<ApiResponse<bool>> ProcessStateTransitionAsync(string subscriptionId, string newStatus, string reason = null, string changedByUserId = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<bool>.ErrorResponse("Subscription not found");

            var oldStatus = subscription.Status;

            // Validate state transition
            var validationResult = ValidateStateTransition(oldStatus, newStatus);
            if (!validationResult.IsValid)
                return ApiResponse<bool>.ErrorResponse(validationResult.ErrorMessage);

            // Update subscription status
            subscription.Status = newStatus;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Update status-specific properties
            await UpdateStatusSpecificPropertiesAsync(subscription, newStatus, reason);

            // Add status history
            await _subscriptionRepository.AddStatusHistoryAsync(new SubscriptionStatusHistory
            {
                SubscriptionId = subscription.Id,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                Reason = reason,
                ChangedByUserId = changedByUserId,
                ChangedAt = DateTime.UtcNow
            });

            await _subscriptionRepository.UpdateAsync(subscription);

            // Audit log
            await _auditService.LogUserActionAsync(
                changedByUserId ?? "System",
                "SubscriptionStateChange",
                "Subscription",
                subscriptionId,
                $"Status changed from {oldStatus} to {newStatus}: {reason}"
            );

            _logger.LogInformation("Subscription {SubscriptionId} state changed from {OldStatus} to {NewStatus}", 
                subscriptionId, oldStatus, newStatus);

            return ApiResponse<bool>.SuccessResponse(true, "State transition processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing state transition for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to process state transition");
        }
    }

    /// <summary>
    /// Validate if a state transition is allowed
    /// </summary>
    private StateTransitionValidation ValidateStateTransition(string currentStatus, string newStatus)
    {
        var allowedTransitions = GetAllowedTransitions();
        
        if (allowedTransitions.TryGetValue(currentStatus, out var allowedStates))
        {
            if (allowedStates.Contains(newStatus))
            {
                return new StateTransitionValidation { IsValid = true };
            }
        }

        return new StateTransitionValidation 
        { 
            IsValid = false, 
            ErrorMessage = $"Invalid state transition from {currentStatus} to {newStatus}" 
        };
    }

    /// <summary>
    /// Get allowed state transitions
    /// </summary>
    private Dictionary<string, HashSet<string>> GetAllowedTransitions()
    {
        return new Dictionary<string, HashSet<string>>
        {
            [Subscription.SubscriptionStatuses.Pending] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialActive,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Active] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Paused,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.PaymentFailed,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.Paused] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Expired
            },
            [Subscription.SubscriptionStatuses.PaymentFailed] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled,
                Subscription.SubscriptionStatuses.Suspended
            },
            [Subscription.SubscriptionStatuses.Suspended] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialActive] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.TrialExpired,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.TrialExpired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active,
                Subscription.SubscriptionStatuses.Cancelled
            },
            [Subscription.SubscriptionStatuses.Cancelled] = new HashSet<string>
            {
                // No valid transitions from Cancelled (matches Subscription entity behavior)
            },
            [Subscription.SubscriptionStatuses.Expired] = new HashSet<string>
            {
                Subscription.SubscriptionStatuses.Active // Allow reactivation
            }
        };
    }

    /// <summary>
    /// Update status-specific properties
    /// </summary>
    private async Task UpdateStatusSpecificPropertiesAsync(Subscription subscription, string newStatus, string reason)
    {
        switch (newStatus)
        {
            case Subscription.SubscriptionStatuses.Active:
                subscription.ResumedDate = DateTime.UtcNow;
                subscription.PauseReason = null;
                subscription.CancellationReason = null;
                break;

            case Subscription.SubscriptionStatuses.Paused:
                subscription.PausedDate = DateTime.UtcNow;
                subscription.PauseReason = reason;
                break;

            case Subscription.SubscriptionStatuses.Cancelled:
                subscription.CancelledDate = DateTime.UtcNow;
                subscription.CancellationReason = reason;
                subscription.AutoRenew = false;
                break;

            case Subscription.SubscriptionStatuses.PaymentFailed:
                subscription.LastPaymentFailedDate = DateTime.UtcNow;
                subscription.LastPaymentError = reason;
                break;

            case Subscription.SubscriptionStatuses.Suspended:
                subscription.SuspendedDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.Expired:
                subscription.ExpirationDate = DateTime.UtcNow;
                break;

            case Subscription.SubscriptionStatuses.TrialExpired:
                subscription.TrialEndDate = DateTime.UtcNow;
                break;
        }
    }

    /// <summary>
    /// Process subscription expiration
    /// </summary>
    public async Task<ApiResponse<bool>> ProcessSubscriptionExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<bool>.ErrorResponse("Subscription not found");

            if (subscription.Status == Subscription.SubscriptionStatuses.Active && 
                subscription.NextBillingDate <= DateTime.UtcNow)
            {
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.Expired, 
                    "Subscription expired due to non-payment"
                );
            }

            return ApiResponse<bool>.SuccessResponse(true, "Subscription is not due for expiration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription expiration for {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to process subscription expiration");
        }
    }

    /// <summary>
    /// Process trial expiration
    /// </summary>
    public async Task<ApiResponse<bool>> ProcessTrialExpirationAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<bool>.ErrorResponse("Subscription not found");

            if (subscription.Status == Subscription.SubscriptionStatuses.TrialActive && 
                subscription.TrialEndDate <= DateTime.UtcNow)
            {
                return await ProcessStateTransitionAsync(
                    subscriptionId, 
                    Subscription.SubscriptionStatuses.TrialExpired, 
                    "Trial period expired"
                );
            }

            return ApiResponse<bool>.SuccessResponse(true, "Trial is not due for expiration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trial expiration for {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to process trial expiration");
        }
    }

    /// <summary>
    /// Reactivate a cancelled or expired subscription
    /// </summary>
    public async Task<ApiResponse<bool>> ReactivateSubscriptionAsync(string subscriptionId, string reason = null)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<bool>.ErrorResponse("Subscription not found");

            if (subscription.Status != Subscription.SubscriptionStatuses.Cancelled && 
                subscription.Status != Subscription.SubscriptionStatuses.Expired)
            {
                return ApiResponse<bool>.ErrorResponse("Subscription is not in a reactivatable state");
            }

            // Reset subscription dates
            subscription.StartDate = DateTime.UtcNow;
            subscription.NextBillingDate = CalculateNextBillingDate(subscription);
            subscription.CancelledDate = null;
            subscription.ExpirationDate = null;
            subscription.CancellationReason = null;
            subscription.AutoRenew = true;

            return await ProcessStateTransitionAsync(
                subscriptionId, 
                Subscription.SubscriptionStatuses.Active, 
                reason ?? "Subscription reactivated"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<bool>.ErrorResponse("Failed to reactivate subscription");
        }
    }

    /// <summary>
    /// Calculate next billing date based on billing cycle
    /// </summary>
    private DateTime CalculateNextBillingDate(Subscription subscription)
    {
        var billingCycle = subscription.BillingCycle;
        
        return billingCycle.Name.ToLower() switch
        {
            "monthly" => DateTime.UtcNow.AddMonths(1),
            "quarterly" => DateTime.UtcNow.AddMonths(3),
            "annual" => DateTime.UtcNow.AddYears(1),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "daily" => DateTime.UtcNow.AddDays(1),
            _ => DateTime.UtcNow.AddMonths(1) // Default to monthly
        };
    }

    /// <summary>
    /// Get subscription lifecycle status
    /// </summary>
    public async Task<ApiResponse<SubscriptionLifecycleStatus>> GetSubscriptionLifecycleStatusAsync(string subscriptionId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(Guid.Parse(subscriptionId));
            if (subscription == null)
                return ApiResponse<SubscriptionLifecycleStatus>.ErrorResponse("Subscription not found");

            var status = new SubscriptionLifecycleStatus
            {
                SubscriptionId = subscriptionId,
                CurrentStatus = subscription.Status,
                DaysUntilNextBilling = (subscription.NextBillingDate - DateTime.UtcNow).Days,
                IsActive = subscription.Status == Subscription.SubscriptionStatuses.Active,
                IsInTrial = subscription.Status == Subscription.SubscriptionStatuses.TrialActive,
                IsExpired = subscription.Status == Subscription.SubscriptionStatuses.Expired,
                IsCancelled = subscription.Status == Subscription.SubscriptionStatuses.Cancelled,
                IsPaused = subscription.Status == Subscription.SubscriptionStatuses.Paused,
                IsPaymentFailed = subscription.Status == Subscription.SubscriptionStatuses.PaymentFailed,
                CanBeReactivated = subscription.Status == Subscription.SubscriptionStatuses.Cancelled || 
                                  subscription.Status == Subscription.SubscriptionStatuses.Expired,
                CanBePaused = subscription.Status == Subscription.SubscriptionStatuses.Active,
                CanBeCancelled = subscription.Status == Subscription.SubscriptionStatuses.Active || 
                                subscription.Status == Subscription.SubscriptionStatuses.Paused
            };

            return ApiResponse<SubscriptionLifecycleStatus>.SuccessResponse(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lifecycle status for subscription {SubscriptionId}", subscriptionId);
            return ApiResponse<SubscriptionLifecycleStatus>.ErrorResponse("Failed to get lifecycle status");
        }
    }

    /// <summary>
    /// Process bulk state transitions
    /// </summary>
    public async Task<ApiResponse<BulkStateTransitionResult>> ProcessBulkStateTransitionsAsync(
        IEnumerable<string> subscriptionIds, string newStatus, string reason, string changedByUserId = null)
    {
        var result = new BulkStateTransitionResult
        {
            ProcessedAt = DateTime.UtcNow,
            TotalSubscriptions = 0,
            SuccessfulTransitions = 0,
            FailedTransitions = 0,
            Errors = new List<string>()
        };

        foreach (var subscriptionId in subscriptionIds)
        {
            result.TotalSubscriptions++;
            try
            {
                var transitionResult = await ProcessStateTransitionAsync(subscriptionId, newStatus, reason, changedByUserId);
                if (transitionResult.Success)
                {
                    result.SuccessfulTransitions++;
                }
                else
                {
                    result.FailedTransitions++;
                    result.Errors.Add($"Subscription {subscriptionId}: {transitionResult.Message}");
                }
            }
            catch (Exception ex)
            {
                result.FailedTransitions++;
                result.Errors.Add($"Subscription {subscriptionId}: {ex.Message}");
            }
        }

        return ApiResponse<BulkStateTransitionResult>.SuccessResponse(result);
    }
}

public class StateTransitionValidation
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class SubscriptionLifecycleStatus
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int DaysUntilNextBilling { get; set; }
    public bool IsActive { get; set; }
    public bool IsInTrial { get; set; }
    public bool IsExpired { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsPaused { get; set; }
    public bool IsPaymentFailed { get; set; }
    public bool CanBeReactivated { get; set; }
    public bool CanBePaused { get; set; }
    public bool CanBeCancelled { get; set; }
}

public class BulkStateTransitionResult
{
    public DateTime ProcessedAt { get; set; }
    public int TotalSubscriptions { get; set; }
    public int SuccessfulTransitions { get; set; }
    public int FailedTransitions { get; set; }
    public List<string> Errors { get; set; } = new();
}
