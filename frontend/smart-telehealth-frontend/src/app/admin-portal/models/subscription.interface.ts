export interface Subscription {
  id: string;
  userId: number;
  subscriptionPlanId: string;
  status: SubscriptionStatus;
  startDate: Date;
  endDate?: Date;
  nextBillingDate: Date;
  currentPrice: number;
  autoRenew: boolean;
  notes?: string;
  
  // Status-specific properties
  pausedDate?: Date;
  resumedDate?: Date;
  cancelledDate?: Date;
  expirationDate?: Date;
  suspendedDate?: Date;
  lastBillingDate?: Date;
  cancellationReason?: string;
  pauseReason?: string;
  
  // Payment integration
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  paymentMethodId?: string;
  lastPaymentDate?: Date;
  lastPaymentFailedDate?: Date;
  lastPaymentError?: string;
  failedPaymentAttempts: number;
  
  // Trial properties
  isTrialSubscription: boolean;
  trialStartDate?: Date;
  trialEndDate?: Date;
  trialDurationInDays: number;
  
  // Usage tracking
  lastUsedDate?: Date;
  totalUsageCount: number;
  
  // Computed properties
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
  usagePercentage: number;
  
  // Navigation properties
  user?: User;
  subscriptionPlan?: SubscriptionPlan;
  billingCycleId: string;
  currencyId: string;
  
  // Collections
  statusHistory?: SubscriptionStatusHistory[];
  payments?: SubscriptionPayment[];
  
  // Timestamps
  createdAt: Date;
  updatedAt: Date;
}

export type SubscriptionStatus = 
  | 'Pending' 
  | 'Active' 
  | 'Paused' 
  | 'Cancelled' 
  | 'Expired' 
  | 'PaymentFailed' 
  | 'TrialActive' 
  | 'TrialExpired' 
  | 'Suspended';

export interface CreateSubscriptionDto {
  userId: number;
  subscriptionPlanId: string;
  startDate?: Date;
  startImmediately: boolean;
  paymentMethodId?: string;
  autoRenew: boolean;
  notes?: string;
}

export interface UpdateSubscriptionDto {
  id: string;
  status?: SubscriptionStatus;
  currentPrice?: number;
  autoRenew?: boolean;
  notes?: string;
  endDate?: Date;
  nextBillingDate?: Date;
}

export interface SubscriptionListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: SubscriptionStatus[];
  planId?: string[];
  userId?: string[];
  dateFrom?: Date;
  dateTo?: Date;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface BulkActionRequest {
  subscriptionIds: string[];
  action: 'activate' | 'pause' | 'resume' | 'cancel' | 'suspend';
  reason?: string;
  effectiveDate?: Date;
}

export interface SubscriptionStatusHistory {
  id: string;
  subscriptionId: string;
  fromStatus: SubscriptionStatus;
  toStatus: SubscriptionStatus;
  reason?: string;
  changedByUserId?: number;
  changedAt: Date;
  changedByUser?: User;
}

export interface SubscriptionPayment {
  id: string;
  subscriptionId: string;
  amount: number;
  status: PaymentStatus;
  type: PaymentType;
  dueDate: Date;
  paidAt?: Date;
  failedAt?: Date;
  failureReason?: string;
  stripePaymentIntentId?: string;
  stripeInvoiceId?: string;
  createdAt: Date;
  updatedAt: Date;
}

export type PaymentStatus = 'Pending' | 'Processing' | 'Succeeded' | 'Failed' | 'Cancelled' | 'Refunded' | 'PartiallyRefunded';
export type PaymentType = 'Subscription' | 'Trial' | 'Setup' | 'Upgrade' | 'Downgrade' | 'Refund' | 'Adjustment';

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  profilePicture?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description?: string;
  shortDescription?: string;
  price: number;
  discountedPrice?: number;
  discountValidUntil?: Date;
  isActive: boolean;
  isFeatured: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays: number;
  isMostPopular: boolean;
  isTrending: boolean;
  displayOrder: number;
  billingCycleId: string;
  currencyId: string;
  features?: string;
  terms?: string;
  createdAt: Date;
  updatedAt: Date;
  
  // Navigation properties
  billingCycle?: MasterBillingCycle;
  currency?: MasterCurrency;
  privileges?: SubscriptionPlanPrivilege[];
}

export interface MasterBillingCycle {
  id: string;
  name: string;
  description?: string;
  durationInDays: number;
  isActive: boolean;
}

export interface MasterCurrency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  isActive: boolean;
}

export interface SubscriptionPlanPrivilege {
  id: string;
  subscriptionPlanId: string;
  privilegeId: string;
  value: number; // -1 for unlimited, 0 for disabled, >0 for limited
  usagePeriodId: string;
  durationMonths: number;
  description?: string;
  effectiveDate?: Date;
  expirationDate?: Date;
  isActive: boolean;
  
  // Navigation properties
  privilege?: Privilege;
  usagePeriod?: MasterBillingCycle;
}

export interface Privilege {
  id: string;
  name: string;
  description?: string;
  privilegeTypeId: string;
  isActive: boolean;
  sortOrder: number;
  
  // Navigation properties
  privilegeType?: MasterPrivilegeType;
}

export interface MasterPrivilegeType {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}
