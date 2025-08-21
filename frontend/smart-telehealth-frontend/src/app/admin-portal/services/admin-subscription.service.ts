import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CommonService } from '../../core/services/common.service';
import { 
  Subscription, 
  CreateSubscriptionDto, 
  UpdateSubscriptionDto, 
  SubscriptionListParams,
  BulkActionRequest,
  SubscriptionStatus,
  SubscriptionStatusHistory,
  SubscriptionPayment,
  SubscriptionPlan,
  User
} from '../models/subscription.interface';
import { JsonModel } from '../../core/models/json-model.interface';

export interface SubscriptionListResponse {
  data: Subscription[];
  meta: {
    totalRecords: number;
    pageSize: number;
    currentPage: number;
    totalPages: number;
    defaultPageSize: number;
  };
  message: string;
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminSubscriptionService {
  private subscriptionsSubject = new BehaviorSubject<Subscription[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public subscriptions$ = this.subscriptionsSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(private commonService: CommonService) {}

  // Get all subscriptions with pagination and filtering
  getAllSubscriptions(params: SubscriptionListParams = {}): Observable<JsonModel<SubscriptionListResponse>> {
    this.loadingSubject.next(true);

    return this.commonService.get<SubscriptionListResponse>(
      '/api/Subscriptions/admin/user-subscriptions',
      params
    ).pipe(
      tap(response => {
        if (response.statusCode === 200 && response.data) {
          this.subscriptionsSubject.next(response.data.data);
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          console.error('Failed to fetch subscriptions:', error);
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get subscription by ID
  getSubscriptionById(id: string): Observable<JsonModel<Subscription>> {
    return this.commonService.get<Subscription>(`/api/Subscriptions/${id}`);
  }

  // Create new subscription
  createSubscription(createDto: CreateSubscriptionDto): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(
      '/api/Subscriptions',
      createDto
    );
  }

  // Update subscription
  updateSubscription(id: string, updateDto: UpdateSubscriptionDto): Observable<JsonModel<Subscription>> {
    return this.commonService.put<Subscription>(
      `/api/Subscriptions/${id}`,
      updateDto
    );
  }

  // Delete subscription
  deleteSubscription(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.delete<boolean>(`/api/Subscriptions/${id}`);
  }

  // Lifecycle management methods
  activateSubscription(id: string, reason?: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/activate`,
      { reason }
    );
  }

  pauseSubscription(id: string, reason?: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/pause`,
      { reason }
    );
  }

  resumeSubscription(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/resume`,
      {}
    );
  }

  cancelSubscription(id: string, reason?: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/cancel`,
      { reason }
    );
  }

  suspendSubscription(id: string, reason?: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/suspend`,
      { reason }
    );
  }

  extendSubscription(id: string, additionalDays: number): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/extend`,
      { additionalDays }
    );
  }

  // Bulk operations
  performBulkAction(actions: BulkActionRequest[]): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      '/api/Subscriptions/admin/bulk-action',
      actions
    );
  }

  // Bulk action for single operation
  bulkAction(request: BulkActionRequest): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      '/api/Subscriptions/admin/bulk-action',
      request
    );
  }

  // Get subscription status history
  getStatusHistory(id: string): Observable<JsonModel<SubscriptionStatusHistory[]>> {
    return this.commonService.get<SubscriptionStatusHistory[]>(
      `/api/Subscriptions/${id}/status-history`
    );
  }

  // Get subscription billing history
  getBillingHistory(id: string): Observable<JsonModel<any[]>> {
    return this.commonService.get<any[]>(
      `/api/Subscriptions/${id}/billing-history`
    );
  }

  // Get subscription payments
  getPayments(id: string): Observable<JsonModel<SubscriptionPayment[]>> {
    return this.commonService.get<SubscriptionPayment[]>(
      `/api/Subscriptions/${id}/payments`
    );
  }

  // Get subscription usage statistics
  getUsageStatistics(id: string): Observable<JsonModel<any>> {
    return this.commonService.get<any>(
      `/api/Subscriptions/${id}/usage-statistics`
    );
  }

  // Get subscription analytics
  getSubscriptionAnalytics(id: string): Observable<JsonModel<any>> {
    return this.commonService.get<any>(
      `/api/Subscriptions/${id}/analytics`
    );
  }

  // Process payment for subscription
  processPayment(id: string, paymentRequest: any): Observable<JsonModel<any>> {
    return this.commonService.post<any>(
      `/api/Subscriptions/${id}/process-payment`,
      paymentRequest
    );
  }

  // Get subscriptions by status
  getSubscriptionsByStatus(status: SubscriptionStatus): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/status/${status}`
    );
  }

  // Get active subscriptions
  getActiveSubscriptions(): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      '/api/Subscriptions/active'
    );
  }

  // Get subscriptions by plan
  getSubscriptionsByPlan(planId: string): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/plan/${planId}`
    );
  }

  // Get subscriptions by user
  getSubscriptionsByUser(userId: number): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/user/${userId}`
    );
  }

  // Get subscriptions due for billing
  getSubscriptionsDueForBilling(): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      '/api/Subscriptions/due-for-billing'
    );
  }

  // Get subscriptions near expiration
  getSubscriptionsNearExpiration(daysThreshold: number = 7): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/near-expiration?days=${daysThreshold}`
    );
  }

  // Search subscriptions
  searchSubscriptions(query: string): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/search?q=${encodeURIComponent(query)}`
    );
  }

  // Export subscriptions
  exportSubscriptions(params: SubscriptionListParams, format: 'csv' | 'excel' = 'csv'): Observable<Blob> {
    const exportParams = { ...params, format };
    return this.commonService.getBlob('/api/Subscriptions/admin/export', exportParams);
  }

  // Clear error
  clearError(): void {
    // this.errorSubject.next(null); // This line was removed as per the new_code, as errorSubject is no longer defined.
  }

  // Refresh subscriptions
  refreshSubscriptions(): void {
    this.getAllSubscriptions().subscribe();
  }

  // Get subscription by Stripe ID
  getByStripeSubscriptionId(stripeId: string): Observable<JsonModel<Subscription>> {
    return this.commonService.get<Subscription>(
      `/api/Subscriptions/stripe/${stripeId}`
    );
  }

  // Get subscription by Stripe customer ID
  getByStripeCustomerId(stripeCustomerId: string): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(
      `/api/Subscriptions/stripe/customer/${stripeCustomerId}`
    );
  }

  // Get subscription plans management
  getSubscriptionPlans(params: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    categoryId?: string;
    isActive?: boolean;
  } = {}): Observable<JsonModel<SubscriptionPlan[]>> {
    return this.commonService.get<SubscriptionPlan[]>(
      '/api/Subscriptions/admin/plans/paged',
      params
    );
  }

  // Create subscription plan
  createSubscriptionPlan(createDto: any): Observable<JsonModel<SubscriptionPlan>> {
    return this.commonService.post<SubscriptionPlan>(
      '/api/Subscriptions/admin/plans',
      createDto
    );
  }

  // Update subscription plan
  updateSubscriptionPlan(id: string, updateDto: any): Observable<JsonModel<SubscriptionPlan>> {
    return this.commonService.put<SubscriptionPlan>(
      `/api/Subscriptions/admin/plans/${id}`,
      updateDto
    );
  }

  // Delete subscription plan
  deleteSubscriptionPlan(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.delete<boolean>(
      `/api/Subscriptions/admin/plans/${id}`
    );
  }

  // Activate subscription plan
  activateSubscriptionPlan(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/plans/${id}/activate`,
      {}
    );
  }

  // Deactivate subscription plan
  deactivateSubscriptionPlan(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/plans/${id}/deactivate`,
      {}
    );
  }

  // Get categories
  getCategories(params: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    isActive?: boolean;
  } = {}): Observable<JsonModel<any[]>> {
    return this.commonService.get<any[]>(
      '/api/Subscriptions/admin/categories',
      params
    );
  }

  // Create category
  createCategory(createDto: any): Observable<JsonModel<any>> {
    return this.commonService.post<any>(
      '/api/Subscriptions/admin/categories',
      createDto
    );
  }

  // Update category
  updateCategory(id: string, updateDto: any): Observable<JsonModel<any>> {
    return this.commonService.post<any>(
      `/api/Subscriptions/admin/categories/${id}`,
      updateDto
    );
  }

  // Delete category
  deleteCategory(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/categories/${id}/delete`,
      {}
    );
  }

  // Get analytics
  getAnalytics(params: {
    startDate?: Date;
    endDate?: Date;
    planId?: string;
  } = {}): Observable<JsonModel<any>> {
    // Use the enhanced plans endpoint with analytics parameter
    const queryParams = {
      ...params,
      includeAnalytics: true
    };
    return this.commonService.get<any>(
      '/api/Subscriptions/admin/plans/paged',
      queryParams
    );
  }

  // Extend user subscription
  extendUserSubscription(id: string, extendDto: { newEndDate: Date; reason?: string }): Observable<JsonModel<boolean>> {
    return this.commonService.post<boolean>(
      `/api/Subscriptions/admin/${id}/extend`,
      extendDto
    );
  }

  // Get all users
  getAllUsers(): Observable<JsonModel<User[]>> {
    return this.commonService.get<User[]>(
      '/api/Users',
      { page: 1, pageSize: 1000 } // Get all users with pagination
    );
  }
}
