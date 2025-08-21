import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface JsonModel<T = any> {
  data: T;
  message: string;
  statusCode: number;
  meta?: any;
}

export interface SubscriptionAnalytics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  newSubscriptionsThisMonth: number;
  cancelledSubscriptionsThisMonth: number;
  subscriptionGrowthRate: number;
  averageSubscriptionValue: number;
  topPlans: PlanAnalytics[];
  statusDistribution: StatusDistribution[];
  monthlyTrends: MonthlyTrend[];
  userRetentionRate: number;
  churnRate: number;
}

export interface PlanAnalytics {
  planId: string;
  planName: string;
  subscriptionCount: number;
  revenue: number;
  growthRate: number;
  averageLifetime: number;
}

export interface StatusDistribution {
  status: string;
  count: number;
  percentage: number;
}

export interface MonthlyTrend {
  month: string;
  subscriptions: number;
  revenue: number;
  cancellations: number;
}

export interface RevenueAnalytics {
  totalRevenue: number;
  monthlyRevenue: number;
  revenueGrowth: number;
  averageOrderValue: number;
  topRevenueSources: RevenueSource[];
  revenueByPlan: PlanRevenue[];
  monthlyRevenueTrend: MonthlyRevenueTrend[];
}

export interface RevenueSource {
  source: string;
  revenue: number;
  percentage: number;
}

export interface PlanRevenue {
  planId: string;
  planName: string;
  revenue: number;
  subscriptionCount: number;
  averageValue: number;
}

export interface MonthlyRevenueTrend {
  month: string;
  revenue: number;
  growth: number;
}

export interface UserAnalytics {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  userGrowthRate: number;
  userEngagementRate: number;
  averageSessionDuration: number;
  topUserSegments: UserSegment[];
  userRetentionByPlan: UserRetentionByPlan[];
}

export interface UserSegment {
  segment: string;
  count: number;
  percentage: number;
  averageValue: number;
}

export interface UserRetentionByPlan {
  planId: string;
  planName: string;
  retentionRate: number;
  averageLifetime: number;
  churnRate: number;
}

export interface PrivilegeUsageAnalytics {
  totalPrivileges: number;
  mostUsedPrivileges: PrivilegeUsage[];
  leastUsedPrivileges: PrivilegeUsage[];
  privilegeUsageByPlan: PlanPrivilegeUsage[];
  usageTrends: PrivilegeUsageTrend[];
}

export interface PrivilegeUsage {
  privilegeId: string;
  privilegeName: string;
  usageCount: number;
  usagePercentage: number;
  averageUsagePerUser: number;
}

export interface PlanPrivilegeUsage {
  planId: string;
  planName: string;
  privileges: PrivilegeUsage[];
  totalUsage: number;
}

export interface PrivilegeUsageTrend {
  month: string;
  privilegeId: string;
  privilegeName: string;
  usageCount: number;
  growthRate: number;
}

export interface SystemPerformanceAnalytics {
  systemUptime: number;
  averageResponseTime: number;
  errorRate: number;
  activeConnections: number;
  databasePerformance: DatabasePerformance;
  apiPerformance: ApiPerformance[];
}

export interface DatabasePerformance {
  averageQueryTime: number;
  slowQueries: number;
  connectionPoolUsage: number;
  cacheHitRate: number;
}

export interface ApiPerformance {
  endpoint: string;
  averageResponseTime: number;
  requestCount: number;
  errorCount: number;
  successRate: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminAnalyticsService {
  private readonly baseUrl = environment.apiUrl;
  private analyticsSubject = new BehaviorSubject<any>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);

  public analytics$ = this.analyticsSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get subscription analytics
  getSubscriptionAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<SubscriptionAnalytics>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let httpParams = new HttpParams();
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<SubscriptionAnalytics>>(
      `${this.baseUrl}/api/analytics/subscriptions`,
      { params: httpParams }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, subscriptions: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch subscription analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get revenue analytics
  getRevenueAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<RevenueAnalytics>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let httpParams = new HttpParams();
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<RevenueAnalytics>>(
      `${this.baseUrl}/api/analytics/revenue`,
      { params: httpParams }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, revenue: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch revenue analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get user analytics
  getUserAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<UserAnalytics>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let httpParams = new HttpParams();
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<UserAnalytics>>(
      `${this.baseUrl}/api/analytics/users`,
      { params: httpParams }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, users: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch user analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get privilege usage analytics
  getPrivilegeUsageAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<PrivilegeUsageAnalytics>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let httpParams = new HttpParams();
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<PrivilegeUsageAnalytics>>(
      `${this.baseUrl}/api/analytics/privileges`,
      { params: httpParams }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, privileges: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch privilege usage analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get system performance analytics
  getSystemPerformanceAnalytics(): Observable<JsonModel<SystemPerformanceAnalytics>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.get<JsonModel<SystemPerformanceAnalytics>>(
      `${this.baseUrl}/api/analytics/system-performance`
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, system: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch system performance analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get comprehensive dashboard analytics
  getDashboardAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<any>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let httpParams = new HttpParams();
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/dashboard`,
      { params: httpParams }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next(response.data);
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to fetch dashboard analytics');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get custom report
  getCustomReport(reportType: string, parameters: any): Observable<JsonModel<any>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.post<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/custom-report`,
      { reportType, parameters }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200) {
          this.analyticsSubject.next({ ...this.analyticsSubject.value, customReport: response.data });
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          this.errorSubject.next(error.message || 'Failed to generate custom report');
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Export analytics data
  exportAnalyticsData(reportType: string, format: 'csv' | 'excel' | 'pdf', parameters?: any): Observable<Blob> {
    let httpParams = new HttpParams()
      .set('reportType', reportType)
      .set('format', format);

    if (parameters) {
      Object.keys(parameters).forEach(key => {
        if (parameters[key] !== null && parameters[key] !== undefined) {
          httpParams = httpParams.set(key, parameters[key].toString());
        }
      });
    }

    return this.http.get(`${this.baseUrl}/api/analytics/export`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  // Get real-time metrics
  getRealTimeMetrics(): Observable<JsonModel<any>> {
    return this.http.get<JsonModel<any>>(`${this.baseUrl}/api/analytics/real-time`);
  }

  // Get subscription analytics by plan
  getSubscriptionAnalyticsByPlan(planId: string, startDate?: Date, endDate?: Date): Observable<JsonModel<any>> {
    let httpParams = new HttpParams().set('planId', planId);
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/subscriptions/plan/${planId}`,
      { params: httpParams }
    );
  }

  // Get user analytics by subscription
  getUserAnalyticsBySubscription(subscriptionId: string): Observable<JsonModel<any>> {
    return this.http.get<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/users/subscription/${subscriptionId}`
    );
  }

  // Get privilege usage by user
  getPrivilegeUsageByUser(userId: number, startDate?: Date, endDate?: Date): Observable<JsonModel<any>> {
    let httpParams = new HttpParams().set('userId', userId.toString());
    if (startDate) httpParams = httpParams.set('startDate', startDate.toISOString());
    if (endDate) httpParams = httpParams.set('endDate', endDate.toISOString());

    return this.http.get<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/privileges/user/${userId}`,
      { params: httpParams }
    );
  }

  // Get comparative analytics
  getComparativeAnalytics(metric: string, period1: { start: Date; end: Date }, period2: { start: Date; end: Date }): Observable<JsonModel<any>> {
    const params = {
      metric,
      period1: {
        start: period1.start.toISOString(),
        end: period1.end.toISOString()
      },
      period2: {
        start: period2.start.toISOString(),
        end: period2.end.toISOString()
      }
    };

    return this.http.post<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/comparative`,
      params
    );
  }

  // Get predictive analytics
  getPredictiveAnalytics(metric: string, forecastPeriod: number): Observable<JsonModel<any>> {
    const params = {
      metric,
      forecastPeriod
    };

    return this.http.post<JsonModel<any>>(
      `${this.baseUrl}/api/analytics/predictive`,
      params
    );
  }

  // Clear error
  clearError(): void {
    this.errorSubject.next(null);
  }

  // Refresh analytics
  refreshAnalytics(): void {
    this.getDashboardAnalytics().subscribe();
  }

  // Get cached analytics
  getCachedAnalytics(): any {
    return this.analyticsSubject.value;
  }
}
