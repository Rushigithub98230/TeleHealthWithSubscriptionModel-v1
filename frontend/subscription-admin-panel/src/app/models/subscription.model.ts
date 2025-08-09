export interface Subscription {
  id: string;
  userId: string;
  subscriptionPlanId: string;
  status: string;
  startDate: Date;
  endDate?: Date;
  nextBillingDate?: Date;
  currentPrice: number;
  autoRenew: boolean;
  pausedDate?: Date;
  resumedDate?: Date;
  cancelledDate?: Date;
  expirationDate?: Date;
  stripeSubscriptionId?: string;
  stripeCustomerId?: string;
  paymentMethodId?: string;
  lastPaymentDate?: Date;
  lastPaymentFailedDate?: Date;
  failedPaymentAttempts: number;
  isTrialSubscription: boolean;
  trialStartDate?: Date;
  trialEndDate?: Date;
  trialDurationInDays?: number;
  lastUsedDate?: Date;
  totalUsageCount: number;
  planName?: string;
  userName?: string;
  userEmail?: string;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays?: number;
  price: number;
  discountedPrice?: number;
  billingCycleId: string;
  currencyId: string;
  stripeProductId?: string;
  stripeMonthlyPriceId?: string;
  stripeYearlyPriceId?: string;
  currency?: string;
  billingCycle?: string;
}

export interface CreateSubscriptionDto {
  userId: string;
  subscriptionPlanId: string;
  paymentMethodId?: string;
  startDate?: Date;
}

export interface CreateSubscriptionPlanDto {
  name: string;
  description: string;
  price: number;
  discountedPrice?: number;
  billingCycleId: string;
  currencyId: string;
  isTrialAllowed: boolean;
  trialDurationInDays?: number;
}

export interface BillingRecord {
  id: string;
  userId: string;
  subscriptionId?: string;
  amount: number;
  status: string;
  type: string;
  billingDate: Date;
  dueDate?: Date;
  paidAt?: Date;
  description: string;
  currencyId: string;
  totalAmount: number;
  taxAmount: number;
  shippingAmount: number;
  stripePaymentIntentId?: string;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  stripeCustomerId?: string;
  createdAt: Date;
  lastLoginAt?: Date;
}

export interface PaymentMethod {
  id: string;
  userId: string;
  stripePaymentMethodId: string;
  type: string;
  last4: string;
  brand: string;
  expiryMonth: number;
  expiryYear: number;
  isDefault: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  statusCode: number;
}

export interface DashboardStats {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  totalUsers: number;
  pendingPayments: number;
  systemHealth: string;
}

export interface RevenueAnalytics {
  totalRevenue: number;
  monthlyRevenue: number;
  revenueGrowth: number;
  monthlyRevenueData: MonthlyRevenueData[];
}

export interface MonthlyRevenueData {
  month: string;
  revenue: number;
  subscriptions: number;
}

export interface UsageStatistics {
  subscriptionId: string;
  planName: string;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  totalPrivileges: number;
  usedPrivileges: number;
  privilegeUsage: PrivilegeUsage[];
}

export interface PrivilegeUsage {
  privilegeName: string;
  limit: number;
  used: number;
  remaining: number;
}
