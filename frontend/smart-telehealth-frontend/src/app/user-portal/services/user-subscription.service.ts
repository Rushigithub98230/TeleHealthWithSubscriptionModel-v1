import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CommonService } from '../../core/services/common.service';
import { AuthService } from '../../core/services/auth.service';
import { JsonModel } from '../../core/models/json-model.interface';
import { Subscription, SubscriptionPlan, SubscriptionUsage, SubscriptionAnalytics } from '../models/subscription.interface';

@Injectable({
  providedIn: 'root'
})
export class UserSubscriptionService {
  private subscriptionsSubject = new BehaviorSubject<Subscription[]>([]);
  private plansSubject = new BehaviorSubject<SubscriptionPlan[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public subscriptions$ = this.subscriptionsSubject.asObservable();
  public plans$ = this.plansSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(
    private commonService: CommonService,
    private authService: AuthService
  ) {}

  // Get current user ID from auth service
  private getCurrentUserId(): number {
    const user = this.authService.getCurrentUser();
    return user?.id || 1; // Fallback to 1 if user not found
  }

  // Get user subscriptions
  getUserSubscriptions(): Observable<JsonModel<Subscription[]>> {
    const userId = this.getCurrentUserId();
    return this.commonService.get<Subscription[]>(`/api/Subscriptions/user/${userId}`);
  }

  // Get subscription by ID
  getSubscriptionById(id: string): Observable<JsonModel<Subscription>> {
    return this.commonService.get<Subscription>(`/api/Subscriptions/${id}`);
  }

  // Get available subscription plans
  getAvailablePlans(): Observable<JsonModel<SubscriptionPlan[]>> {
    return this.commonService.get<SubscriptionPlan[]>('/api/Subscriptions/plans/public');
  }

  // Create new subscription
  createSubscription(subscriptionData: any): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>('/api/Subscriptions', subscriptionData);
  }

  // Update subscription
  updateSubscription(id: string, subscriptionData: any): Observable<JsonModel<Subscription>> {
    return this.commonService.put<Subscription>(`/api/Subscriptions/${id}`, subscriptionData);
  }

  // Cancel subscription
  cancelSubscription(id: string, reason: string): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(`/api/Subscriptions/${id}/cancel`, reason);
  }

  // Pause subscription
  pauseSubscription(id: string): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(`/api/Subscriptions/${id}/pause`, {});
  }

  // Resume subscription
  resumeSubscription(id: string): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(`/api/Subscriptions/${id}/resume`, {});
  }

  // Upgrade subscription
  upgradeSubscription(id: string, newPlanId: string): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(`/api/Subscriptions/${id}/upgrade`, newPlanId);
  }

  // Downgrade subscription (maps to upgrade with different logic)
  downgradeSubscription(id: string, newPlanId: string): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(`/api/Subscriptions/${id}/upgrade`, newPlanId);
  }

  // Get subscription usage statistics
  getSubscriptionUsage(id: string): Observable<JsonModel<SubscriptionUsage>> {
    return this.commonService.get<SubscriptionUsage>(`/api/Subscriptions/${id}/usage-statistics`);
  }

  // Get subscription billing history (maps to existing endpoint)
  getSubscriptionPayments(id: string): Observable<JsonModel<any[]>> {
    return this.commonService.get<any[]>(`/api/Subscriptions/${id}/billing-history`);
  }

  // Get subscription analytics (maps to usage statistics)
  getSubscriptionAnalytics(id: string): Observable<JsonModel<SubscriptionAnalytics>> {
    return this.commonService.get<SubscriptionAnalytics>(`/api/Subscriptions/${id}/usage-statistics`);
  }

  // Refresh subscriptions data
  refreshSubscriptions(): void {
    this.getUserSubscriptions().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.subscriptionsSubject.next(response.data);
        }
      },
      error: (error) => {
        console.error('Failed to refresh subscriptions:', error);
      }
    });
  }

  // Refresh plans data
  refreshPlans(): void {
    this.getAvailablePlans().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.plansSubject.next(response.data);
        }
      },
      error: (error) => {
        console.error('Failed to refresh plans:', error);
      }
    });
  }

  // Set loading state
  setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }
}
