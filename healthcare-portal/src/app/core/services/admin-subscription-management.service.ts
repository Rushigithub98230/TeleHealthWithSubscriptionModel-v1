import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

// Interfaces for Admin Subscription Management
export interface AdminSubscriptionPlan {
  id: string;
  name: string;
  description?: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: string;
  billingCycleId: string;
  currencyId: string;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  displayOrder: number;
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeQuarterlyPriceId?: string;
  stripeAnnualPriceId?: string;
  features?: string;
  terms?: string;
  effectiveDate?: string;
  expirationDate?: string;
  effectivePrice: number;
  hasActiveDiscount: boolean;
  isCurrentlyAvailable: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface AdminUserSubscription {
  id: string;
  userId: string;
  userName: string;
  planId: string;
  planName: string;
  planDescription: string;
  status: string;
  statusReason?: string;
  currentPrice: number;
  autoRenew: boolean;
  notes?: string;
  startDate: string;
  endDate?: string;
  nextBillingDate: string;
  pausedDate?: string;
  resumedDate?: string;
  cancelledDate?: string;
  expirationDate?: string;
  cancellationReason?: string;
  pauseReason?: string;
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  paymentMethodId?: string;
  lastPaymentDate?: string;
  lastPaymentFailedDate?: string;
  lastPaymentError?: string;
  failedPaymentAttempts: number;
  isTrialSubscription: boolean;
  trialStartDate?: string;
  trialEndDate?: string;
  trialDurationInDays: number;
  lastUsedDate?: string;
  totalUsageCount: number;
  isActive: boolean;
  isPaused: boolean;
  isCancelled: boolean;
  isExpired: boolean;
  hasPaymentIssues: boolean;
  isInTrial: boolean;
  daysUntilNextBilling: number;
  isNearExpiration: boolean;
  canPause: boolean;
  canResume: boolean;
  canCancel: boolean;
  canRenew: boolean;
  createdAt: string;
  updatedAt: string;
  billingCycleId: string;
  currencyId: string;
}

export interface AdminCategory {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  basePrice: number;
  consultationFee: number;
  oneTimeConsultationFee: number;
  requiresHealthAssessment: boolean;
  allowsMedicationDelivery: boolean;
  allowsFollowUpMessaging: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface AdminSubscriptionAnalytics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  pausedSubscriptions: number;
  cancelledSubscriptions: number;
  newSubscriptionsThisMonth: number;
  churnRate: number;
  averageSubscriptionValue: number;
  totalRevenue: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  topCategories: CategoryAnalytics[];
  monthlyGrowth: number;
  subscriptionsByPlan: { [key: string]: number };
  subscriptionsByStatus: { [key: string]: number };
}

export interface CategoryAnalytics {
  categoryId: string;
  categoryName: string;
  subscriptionCount: number;
  revenue: number;
  growthRate: number;
}

export interface BulkActionRequest {
  subscriptionIds: string[];
  action: 'cancel' | 'pause' | 'resume';
  reason?: string;
}

export interface BulkActionResult {
  totalCount: number;
  successCount: number;
  failureCount: number;
  results: BulkActionItemResult[];
}

export interface BulkActionItemResult {
  subscriptionId: string;
  success: boolean;
  message: string;
}

export interface ExtendSubscriptionRequest {
  newEndDate: string;
  reason?: string;
}

export interface PaginationMetadata {
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  statusCode?: number;
  pagination?: PaginationMetadata;
}

@Injectable({
  providedIn: 'root'
})
export class AdminSubscriptionManagementService {
  private baseUrl = `${environment.apiUrl}/webadmin/subscription-management`;
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor(private http: HttpClient) {}

  // ==================== SUBSCRIPTION PLANS ====================

  /**
   * Get all subscription plans for admin management
   */
  getAllPlans(
    searchTerm?: string,
    status?: string,
    page: number = 1,
    pageSize: number = 20
  ): Observable<ApiResponse<AdminSubscriptionPlan[]>> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<ApiResponse<AdminSubscriptionPlan[]>>(`${this.baseUrl}/plans`, { params })
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Create a new subscription plan
   */
  createPlan(plan: any): Observable<ApiResponse<AdminSubscriptionPlan>> {
    this.loadingSubject.next(true);
    
    return this.http.post<ApiResponse<AdminSubscriptionPlan>>(`${this.baseUrl}/plans`, plan)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Update an existing subscription plan
   */
  updatePlan(id: string, plan: any): Observable<ApiResponse<AdminSubscriptionPlan>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminSubscriptionPlan>>(`${this.baseUrl}/plans/${id}`, plan)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Delete a subscription plan
   */
  deletePlan(id: string): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);
    
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/plans/${id}`)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Activate a subscription plan
   */
  activatePlan(id: string): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);
    
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/plans/${id}/activate`, {})
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Deactivate a subscription plan
   */
  deactivatePlan(id: string): Observable<ApiResponse<boolean>> {
    this.loadingSubject.next(true);
    
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/plans/${id}/deactivate`, {})
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  // ==================== USER SUBSCRIPTIONS ====================

  /**
   * Get all user subscriptions for admin management
   */
  getAllUserSubscriptions(
    searchTerm?: string,
    status?: string,
    category?: string,
    plan?: string,
    startDate?: Date,
    endDate?: Date,
    page: number = 1,
    pageSize: number = 20
  ): Observable<ApiResponse<AdminUserSubscription[]>> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (status) {
      params = params.set('status', status);
    }
    if (category) {
      params = params.set('category', category);
    }
    if (plan) {
      params = params.set('plan', plan);
    }
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<ApiResponse<AdminUserSubscription[]>>(`${this.baseUrl}/user-subscriptions`, { params })
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Cancel a user subscription
   */
  cancelUserSubscription(id: string, reason?: string): Observable<ApiResponse<AdminUserSubscription>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminUserSubscription>>(`${this.baseUrl}/user-subscriptions/${id}/cancel`, reason)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Pause a user subscription
   */
  pauseUserSubscription(id: string, reason?: string): Observable<ApiResponse<AdminUserSubscription>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminUserSubscription>>(`${this.baseUrl}/user-subscriptions/${id}/pause`, reason)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Resume a user subscription
   */
  resumeUserSubscription(id: string): Observable<ApiResponse<AdminUserSubscription>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminUserSubscription>>(`${this.baseUrl}/user-subscriptions/${id}/resume`, {})
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Extend a user subscription
   */
  extendUserSubscription(id: string, extendRequest: ExtendSubscriptionRequest): Observable<ApiResponse<AdminUserSubscription>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminUserSubscription>>(`${this.baseUrl}/user-subscriptions/${id}/extend`, extendRequest)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  // ==================== CATEGORIES ====================

  /**
   * Get all categories for admin management
   */
  getAllCategories(
    searchTerm?: string,
    page: number = 1,
    pageSize: number = 20
  ): Observable<ApiResponse<AdminCategory[]>> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<ApiResponse<AdminCategory[]>>(`${this.baseUrl}/categories`, { params })
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Create a new category
   */
  createCategory(category: any): Observable<ApiResponse<AdminCategory>> {
    this.loadingSubject.next(true);
    
    return this.http.post<ApiResponse<AdminCategory>>(`${this.baseUrl}/categories`, category)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Update an existing category
   */
  updateCategory(id: string, category: any): Observable<ApiResponse<AdminCategory>> {
    this.loadingSubject.next(true);
    
    return this.http.put<ApiResponse<AdminCategory>>(`${this.baseUrl}/categories/${id}`, category)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  /**
   * Delete a category
   */
  deleteCategory(id: string): Observable<ApiResponse<any>> {
    this.loadingSubject.next(true);
    
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/categories/${id}`)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  // ==================== ANALYTICS ====================

  /**
   * Get subscription analytics for admin dashboard
   */
  getAnalytics(
    period: string = 'month',
    startDate?: Date,
    endDate?: Date
  ): Observable<ApiResponse<AdminSubscriptionAnalytics>> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams().set('period', period);

    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<ApiResponse<AdminSubscriptionAnalytics>>(`${this.baseUrl}/analytics`, { params })
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  // ==================== BULK OPERATIONS ====================

  /**
   * Perform bulk operations on subscriptions
   */
  performBulkAction(request: BulkActionRequest): Observable<ApiResponse<BulkActionResult>> {
    this.loadingSubject.next(true);
    
    return this.http.post<ApiResponse<BulkActionResult>>(`${this.baseUrl}/bulk-actions`, request)
      .pipe(
        map(response => {
          this.loadingSubject.next(false);
          return response;
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }

  // ==================== UTILITY METHODS ====================

  /**
   * Get loading state
   */
  getLoadingState(): Observable<boolean> {
    return this.loading$;
  }

  /**
   * Handle API errors
   */
  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    this.loadingSubject.next(false);
    throw error;
  }
}