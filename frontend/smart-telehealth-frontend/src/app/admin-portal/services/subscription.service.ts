import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService } from '../../core/services/common.service';
import { JsonModel } from '../../core/models/index';

export interface Subscription {
  id: number;
  userId: number;
  subscriptionPlanId: number;
  status: SubscriptionStatus;
  startDate: Date;
  endDate: Date;
  billingCycle: BillingCycle;
  amount: number;
  user?: User;
  subscriptionPlan?: SubscriptionPlan;
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionPlan {
  id: number;
  name: string;
  description: string;
  price: number;
  billingCycle: BillingCycle;
  duration: number;
  features: string[];
  isActive: boolean;
  maxUsers?: number;
  privileges: SubscriptionPlanPrivilege[];
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionPlanPrivilege {
  id: number;
  subscriptionPlanId: number;
  privilegeId: number;
  privilege: Privilege;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  profilePicture?: string;
}

export interface Privilege {
  id: number;
  name: string;
  description: string;
  category: string;
  isActive: boolean;
  sortOrder: number;
}

export type SubscriptionStatus = 'Active' | 'Inactive' | 'Suspended' | 'Cancelled' | 'Expired' | 'Pending';
export type BillingCycle = 'Monthly' | 'Quarterly' | 'Yearly' | 'OneTime';

export interface CreateSubscriptionRequest {
  userId: number;
  subscriptionPlanId: number;
  startDate: Date;
  endDate: Date;
  billingCycle: BillingCycle;
  amount: number;
}

export interface UpdateSubscriptionRequest extends Partial<CreateSubscriptionRequest> {
  id: number;
  status?: SubscriptionStatus;
}

export interface CreateSubscriptionPlanRequest {
  name: string;
  description: string;
  price: number;
  billingCycle: BillingCycle;
  duration: number;
  features: string[];
  isActive: boolean;
  maxUsers?: number;
  privilegeIds: number[];
}

export interface UpdateSubscriptionPlanRequest extends Partial<CreateSubscriptionPlanRequest> {
  id: number;
}

export interface SubscriptionListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: SubscriptionStatus[];
  planId?: number[];
  dateFrom?: Date;
  dateTo?: Date;
}

export interface SubscriptionPlanListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  billingCycle?: BillingCycle[];
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private readonly API_PATHS = {
    SUBSCRIPTIONS: '/subscriptions',
    SUBSCRIPTIONS_EXPORT: '/subscriptions/export',
    SUBSCRIPTION_PLANS: '/subscription-plans',
    SUBSCRIPTION_PLANS_EXPORT: '/subscription-plans/export'
  } as const;

  constructor(private commonService: CommonService) {}

  // Subscription methods
  getSubscriptions(params: SubscriptionListParams = {}): Observable<JsonModel<Subscription[]>> {
    return this.commonService.get<Subscription[]>(this.API_PATHS.SUBSCRIPTIONS, params);
  }

  getSubscriptionById(id: number): Observable<JsonModel<Subscription>> {
    const path = `${this.API_PATHS.SUBSCRIPTIONS}/${id}`;
    return this.commonService.get<Subscription>(path);
  }

  createSubscription(subscriptionData: CreateSubscriptionRequest): Observable<JsonModel<Subscription>> {
    return this.commonService.post<Subscription>(this.API_PATHS.SUBSCRIPTIONS, subscriptionData);
  }

  updateSubscription(id: number, subscriptionData: UpdateSubscriptionRequest): Observable<JsonModel<Subscription>> {
    const path = `${this.API_PATHS.SUBSCRIPTIONS}/${id}`;
    return this.commonService.put<Subscription>(path, subscriptionData);
  }

  deleteSubscription(id: number): Observable<JsonModel<any>> {
    const path = `${this.API_PATHS.SUBSCRIPTIONS}/${id}`;
    return this.commonService.delete<any>(path);
  }

  exportSubscriptions(params: Omit<SubscriptionListParams, 'page' | 'pageSize'> = {}): Observable<Blob> {
    return this.commonService.getBlob(this.API_PATHS.SUBSCRIPTIONS_EXPORT, params);
  }

  // Subscription Plan methods
  getSubscriptionPlans(params: SubscriptionPlanListParams = {}): Observable<JsonModel<SubscriptionPlan[]>> {
    return this.commonService.get<SubscriptionPlan[]>(this.API_PATHS.SUBSCRIPTION_PLANS, params);
  }

  getSubscriptionPlanById(id: number): Observable<JsonModel<SubscriptionPlan>> {
    const path = `${this.API_PATHS.SUBSCRIPTION_PLANS}/${id}`;
    return this.commonService.get<SubscriptionPlan>(path);
  }

  createSubscriptionPlan(planData: CreateSubscriptionPlanRequest): Observable<JsonModel<SubscriptionPlan>> {
    return this.commonService.post<SubscriptionPlan>(this.API_PATHS.SUBSCRIPTION_PLANS, planData);
  }

  updateSubscriptionPlan(id: number, planData: UpdateSubscriptionPlanRequest): Observable<JsonModel<SubscriptionPlan>> {
    const path = `${this.API_PATHS.SUBSCRIPTION_PLANS}/${id}`;
    return this.commonService.put<SubscriptionPlan>(path, planData);
  }

  deleteSubscriptionPlan(id: number): Observable<JsonModel<any>> {
    const path = `${this.API_PATHS.SUBSCRIPTION_PLANS}/${id}`;
    return this.commonService.delete<any>(path);
  }

  exportSubscriptionPlans(params: Omit<SubscriptionPlanListParams, 'page' | 'pageSize'> = {}): Observable<Blob> {
    return this.commonService.getBlob(this.API_PATHS.SUBSCRIPTION_PLANS_EXPORT, params);
  }

  // Utility methods
  getSubscriptionStatusOptions(): Array<{ value: SubscriptionStatus; label: string; color: string }> {
    return [
      { value: 'Active', label: 'Active', color: '#10b981' },
      { value: 'Inactive', label: 'Inactive', color: '#6b7280' },
      { value: 'Suspended', label: 'Suspended', color: '#f59e0b' },
      { value: 'Cancelled', label: 'Cancelled', color: '#ef4444' },
      { value: 'Expired', label: 'Expired', color: '#8b5cf6' },
      { value: 'Pending', label: 'Pending', color: '#3b82f6' }
    ];
  }

  getBillingCycleOptions(): Array<{ value: BillingCycle; label: string; description: string }> {
    return [
      { value: 'Monthly', label: 'Monthly', description: 'Billed every month' },
      { value: 'Quarterly', label: 'Quarterly', description: 'Billed every 3 months' },
      { value: 'Yearly', label: 'Yearly', description: 'Billed annually' },
      { value: 'OneTime', label: 'One Time', description: 'Single payment' }
    ];
  }

  getStatusColor(status: SubscriptionStatus): string {
    const statusOption = this.getSubscriptionStatusOptions().find(option => option.value === status);
    return statusOption?.color || '#6b7280';
  }

  getStatusLabel(status: SubscriptionStatus): string {
    const statusOption = this.getSubscriptionStatusOptions().find(option => option.value === status);
    return statusOption?.label || status;
  }

  getBillingCycleLabel(billingCycle: BillingCycle): string {
    const cycleOption = this.getBillingCycleOptions().find(option => option.value === billingCycle);
    return cycleOption?.label || billingCycle;
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount || 0);
  }

  formatDate(date: string | Date): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  getDaysUntilExpiry(endDate: string | Date): number {
    if (!endDate) return 0;
    const today = new Date();
    const expiry = new Date(endDate);
    const diffTime = expiry.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  isExpiringSoon(endDate: string | Date): boolean {
    const daysUntilExpiry = this.getDaysUntilExpiry(endDate);
    return daysUntilExpiry <= 30 && daysUntilExpiry > 0;
  }

  isExpired(endDate: string | Date): boolean {
    const daysUntilExpiry = this.getDaysUntilExpiry(endDate);
    return daysUntilExpiry < 0;
  }
}
