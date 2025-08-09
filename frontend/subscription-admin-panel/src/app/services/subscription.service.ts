import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { 
  Subscription, 
  SubscriptionPlan, 
  CreateSubscriptionDto, 
  CreateSubscriptionPlanDto,
  ApiResponse,
  DashboardStats,
  RevenueAnalytics,
  UsageStatistics
} from '../models/subscription.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  private getHeaders(): HttpHeaders {
    return this.authService.getAuthHeaders();
  }

  // Dashboard Statistics
  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<ApiResponse<DashboardStats>>(`${this.apiUrl}/api/admin/dashboard`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Subscription Plans
  getAllPlans(): Observable<SubscriptionPlan[]> {
    return this.http.get<ApiResponse<SubscriptionPlan[]>>(`${this.apiUrl}/api/subscriptions/plans`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  createPlan(plan: CreateSubscriptionPlanDto): Observable<SubscriptionPlan> {
    return this.http.post<ApiResponse<SubscriptionPlan>>(`${this.apiUrl}/api/subscriptions/plans`, plan, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  updatePlan(id: string, plan: Partial<SubscriptionPlan>): Observable<SubscriptionPlan> {
    return this.http.put<ApiResponse<SubscriptionPlan>>(`${this.apiUrl}/api/subscriptions/plans/${id}`, plan, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  deletePlan(id: string): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/api/subscriptions/plans/${id}`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.success),
        catchError(this.handleError)
      );
  }

  // Subscriptions
  getAllSubscriptions(): Observable<Subscription[]> {
    return this.http.get<ApiResponse<Subscription[]>>(`${this.apiUrl}/api/admin/subscriptions`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getSubscriptionById(id: string): Observable<Subscription> {
    return this.http.get<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions/${id}`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  createSubscription(subscription: CreateSubscriptionDto): Observable<Subscription> {
    return this.http.post<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions`, subscription, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  cancelSubscription(id: string, reason?: string): Observable<Subscription> {
    const payload = reason ? { reason } : {};
    return this.http.post<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions/${id}/cancel`, payload, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  pauseSubscription(id: string): Observable<Subscription> {
    return this.http.post<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions/${id}/pause`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  resumeSubscription(id: string): Observable<Subscription> {
    return this.http.post<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions/${id}/resume`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  upgradeSubscription(id: string, newPlanId: string): Observable<Subscription> {
    return this.http.post<ApiResponse<Subscription>>(`${this.apiUrl}/api/subscriptions/${id}/upgrade`, { newPlanId }, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Billing Records
  getAllBillingRecords(): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/api/billing/records`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getSubscriptionBillingHistory(subscriptionId: string): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/api/billing/subscription/${subscriptionId}`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Analytics
  getRevenueAnalytics(): Observable<RevenueAnalytics> {
    return this.http.get<ApiResponse<RevenueAnalytics>>(`${this.apiUrl}/api/subscription-analytics/revenue`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getUsageStatistics(subscriptionId: string): Observable<UsageStatistics> {
    return this.http.get<ApiResponse<UsageStatistics>>(`${this.apiUrl}/api/subscriptions/${subscriptionId}/usage`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Payment Processing
  processPayment(billingRecordId: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/api/billing/${billingRecordId}/process-payment`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  retryPayment(billingRecordId: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/api/payments/retry-payment/${billingRecordId}`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Automated Billing
  triggerAutomatedBilling(): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/api/subscription-automation/trigger-billing`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  triggerSubscriptionRenewal(): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/api/subscription-automation/trigger-renewal`, {}, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Search and Filter
  searchSubscriptions(query: string): Observable<Subscription[]> {
    return this.http.get<ApiResponse<Subscription[]>>(`${this.apiUrl}/api/subscription-management/subscriptions/search?q=${query}`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  getSubscriptionsByStatus(status: string): Observable<Subscription[]> {
    return this.http.get<ApiResponse<Subscription[]>>(`${this.apiUrl}/api/subscription-management/subscriptions/status/${status}`, { headers: this.getHeaders() })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  private handleError(error: any): Observable<never> {
    console.error('An error occurred:', error);
    return throwError(() => error);
  }
}
