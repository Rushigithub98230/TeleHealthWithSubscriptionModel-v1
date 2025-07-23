import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  status: string;
}

export interface Plan {
  id: string;
  name: string;
  price: number;
  duration: string;
  status: string;
}

export interface AdminDashboardStats {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  totalUsers: number;
  totalProviders: number;
  // Add more fields as needed from backend DTO
}

export interface Subscription {
  id: string;
  userId: string;
  planId: string;
  status: string;
  startDate: string;
  endDate?: string;
  nextBillingDate?: string;
  // Add more fields as needed from backend DTO
}

export interface BillingHistory {
  id: string;
  subscriptionId: string;
  amount: number;
  status: string;
  date: string;
  // Add more fields as needed
}

export interface BillingRecord {
  id: string;
  userId: string;
  subscriptionId?: string;
  amount: number;
  status: string;
  dueDate?: string;
  paidDate?: string;
  description?: string;
  // Add more fields as needed
}

export interface RevenueSummary {
  totalAccruedRevenue: number;
  totalCashRevenue: number;
  totalRefunded: number;
  asOf: string;
  // Add more fields as needed
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  createdAt: string;
  // Add more fields as needed
}

export interface AuditLog {
  id: string;
  userId: string;
  action: string;
  entityType: string;
  entityId: string;
  description?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface AuditLogSearch {
  userId?: string;
  action?: string;
  entityType?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface RevenueTrendDto {
  date: string;
  revenue: number;
  growth: number;
}
export interface UserActivityData {
  date: string;
  activeUsers: number;
  consultations: number;
  messages: number;
}
export interface SubscriptionsByPlan {
  [planName: string]: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private userBaseUrl = '/api/admin/users';
  private planBaseUrl = '/api/SubscriptionPlans';
  private dashboardUrl = '/api/Admin/dashboard';

  constructor(private http: HttpClient) { }

  // User management
  getUsers(): Observable<User[]> {
    return this.http.get<any>(`${this.userBaseUrl}`).pipe(map(res => res.data));
  }
  addUser(user: Partial<User>): Observable<User> {
    return this.http.post<any>(`${this.userBaseUrl}`, user).pipe(map(res => res.data));
  }
  updateUser(id: number, user: Partial<User>): Observable<User> {
    return this.http.put<any>(`${this.userBaseUrl}/${id}`, user).pipe(map(res => res.data));
  }
  deleteUser(id: number): Observable<void> {
    return this.http.delete<any>(`${this.userBaseUrl}/${id}`).pipe(map(res => res.data));
  }
  assignRole(id: number, role: string): Observable<User> {
    return this.http.patch<any>(`${this.userBaseUrl}/${id}/role`, { role }).pipe(map(res => res.data));
  }

  // Plan management
  getPlans(): Observable<Plan[]> {
    return this.http.get<any>(`${this.planBaseUrl}`).pipe(map(res => res.data));
  }
  addPlan(plan: Partial<Plan>): Observable<Plan> {
    return this.http.post<any>(`${this.planBaseUrl}`, plan).pipe(map(res => res.data));
  }
  updatePlan(id: string, plan: Partial<Plan>): Observable<Plan> {
    return this.http.put<any>(`${this.planBaseUrl}/${id}`, plan).pipe(map(res => res.data));
  }
  deletePlan(id: string): Observable<void> {
    return this.http.delete<any>(`${this.planBaseUrl}/${id}`).pipe(map(res => res.data));
  }

  // Dashboard statistics
  getAdminDashboardStats(): Observable<AdminDashboardStats> {
    return this.http.get<any>(this.dashboardUrl).pipe(map(res => res.data));
  }

  // Subscription management
  getSubscriptions(): Observable<Subscription[]> {
    // NOTE: This assumes a GET /api/subscriptions endpoint exists. If not, backend needs to add it.
    return this.http.get<any>(`/api/subscriptions`).pipe(map(res => res.data));
  }
  getSubscription(id: string): Observable<Subscription> {
    return this.http.get<any>(`/api/subscriptions/${id}`).pipe(map(res => res.data));
  }
  createSubscription(subscription: Partial<Subscription>): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions`, subscription).pipe(map(res => res.data));
  }
  cancelSubscription(id: string, reason: string): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions/${id}/cancel`, reason).pipe(map(res => res.data));
  }
  pauseSubscription(id: string): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions/${id}/pause`, {}).pipe(map(res => res.data));
  }
  resumeSubscription(id: string): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions/${id}/resume`, {}).pipe(map(res => res.data));
  }
  upgradeSubscription(id: string, newPlanId: string): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions/${id}/upgrade`, newPlanId).pipe(map(res => res.data));
  }
  reactivateSubscription(id: string): Observable<Subscription> {
    return this.http.post<any>(`/api/subscriptions/${id}/reactivate`, {}).pipe(map(res => res.data));
  }
  getSubscriptionBillingHistory(id: string): Observable<BillingHistory[]> {
    return this.http.get<any>(`/api/subscriptions/${id}/billing-history`).pipe(map(res => res.data));
  }

  // Billing management
  getPendingBillingRecords(): Observable<BillingRecord[]> {
    return this.http.get<any>(`/api/Billing/pending`).pipe(map(res => res.data));
  }
  getOverdueBillingRecords(): Observable<BillingRecord[]> {
    return this.http.get<any>(`/api/Billing/overdue`).pipe(map(res => res.data));
  }
  getBillingRecord(id: string): Observable<BillingRecord> {
    return this.http.get<any>(`/api/Billing/${id}`).pipe(map(res => res.data));
  }
  processRefund(id: string, amount: number): Observable<BillingRecord> {
    return this.http.post<any>(`/api/Billing/${id}/refund`, { amount }).pipe(map(res => res.data));
  }
  getRevenueSummary(): Observable<RevenueSummary> {
    return this.http.get<any>(`/api/Billing/revenue-summary`).pipe(map(res => res.data));
  }
  exportRevenue(format: string = 'csv'): Observable<Blob> {
    return this.http.get(`/api/Billing/export-revenue?format=${format}`, { responseType: 'blob' });
  }
  getBillingAnalytics(): Observable<{ revenueTrend: RevenueTrendDto[] }> {
    return this.http.get<any>(`/api/Analytics/billing`).pipe(map(res => res.data));
  }
  getUserAnalytics(): Observable<{ userActivityBreakdown: UserActivityData[] }> {
    return this.http.get<any>(`/api/Analytics/users`).pipe(map(res => res.data));
  }
  getSubscriptionAnalytics(): Observable<{ subscriptionsByPlan: SubscriptionsByPlan }> {
    return this.http.get<any>(`/api/Analytics/subscriptions`).pipe(map(res => res.data));
  }
  exportSubscriptionReport(start: Date, end: Date, format: string = 'csv'): Observable<Blob> {
    const startStr = start.toISOString().split('T')[0];
    const endStr = end.toISOString().split('T')[0];
    return this.http.get(`/api/Analytics/reports/subscriptions?startDate=${startStr}&endDate=${endStr}&format=${format}`, { responseType: 'blob' });
  }

  // Notifications
  getNotifications(): Observable<Notification[]> {
    return this.http.get<any>(`/api/Notifications`).pipe(map(res => res.data));
  }
  getNotification(id: string): Observable<Notification> {
    return this.http.get<any>(`/api/Notifications/${id}`).pipe(map(res => res.data));
  }
  // Audit logs
  getAuditLogs(filters?: AuditLogSearch): Observable<AuditLog[]> {
    let params = '';
    if (filters) {
      const query = Object.entries(filters)
        .filter(([_, v]) => v !== undefined && v !== null && v !== '')
        .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v as string)}`)
        .join('&');
      if (query) params = '?' + query;
    }
    return this.http.get<any>(`/api/Audit${params}`).pipe(map(res => res.data));
  }
  getAuditLog(id: string): Observable<AuditLog> {
    return this.http.get<any>(`/api/Audit/${id}`).pipe(map(res => res.data));
  }
}
