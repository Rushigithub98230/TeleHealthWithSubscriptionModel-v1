export interface BillingRecord {
  id: string;
  userId: number;
  subscriptionId?: string;
  consultationId?: string;
  medicationDeliveryId?: string;
  billingCycleId?: string;
  currencyId: string;
  
  // Billing details
  status: BillingStatus;
  type: BillingType;
  amount: number;
  taxAmount: number;
  shippingAmount: number;
  totalAmount: number;
  billingDate: Date;
  paidAt?: Date;
  dueDate?: Date;
  
  // Invoice details
  invoiceNumber?: string;
  description?: string;
  failureReason?: string;
  errorMessage?: string;
  
  // Payment integration
  stripePaymentIntentId?: string;
  stripeInvoiceId?: string;
  paymentMethod?: string;
  transactionId?: string;
  
  // Processing
  processedAt?: Date;
  isRecurring: boolean;
  nextBillingDate?: Date;
  paymentIntentId?: string;
  
  // Accrual
  accruedAmount?: number;
  accrualStartDate?: Date;
  accrualEndDate?: Date;
  
  // Navigation properties
  user?: User;
  subscription?: Subscription;
  consultation?: Consultation;
  medicationDelivery?: MedicationDelivery;
  currency?: MasterCurrency;
  adjustments?: BillingAdjustment[];
  
  // Computed properties
  isPaid: boolean;
  isFailed: boolean;
  isRefunded: boolean;
  isOverdue: boolean;
  
  // Timestamps
  createdAt: Date;
  updatedAt: Date;
}

export type BillingStatus = 'Pending' | 'Paid' | 'Failed' | 'Cancelled' | 'Refunded' | 'Overdue';
export type BillingType = 'Subscription' | 'Consultation' | 'Medication' | 'LateFee' | 'Refund' | 'Recurring' | 'Upfront' | 'Bundle' | 'Invoice' | 'Cycle';

export interface BillingAdjustment {
  id: string;
  billingRecordId: string;
  type: AdjustmentType;
  amount: number;
  description: string;
  reason?: string;
  isPercentage: boolean;
  percentage?: number;
  appliedAt: Date;
  appliedBy?: number;
  isApproved: boolean;
  approvalNotes?: string;
  
  // Navigation properties
  billingRecord?: BillingRecord;
  appliedByUser?: User;
  
  // Computed properties
  isCredit: boolean;
  isDiscount: boolean;
  isRefund: boolean;
}

export type AdjustmentType = 'Discount' | 'Credit' | 'Refund' | 'LateFee' | 'ServiceFee' | 'TaxAdjustment';

export interface CreateBillingRecordDto {
  userId: number;
  subscriptionId?: string;
  consultationId?: string;
  medicationDeliveryId?: string;
  amount: number;
  currency: string;
  paymentMethod?: string;
  description?: string;
  dueDate?: Date;
  type: BillingType;
  isRecurring?: boolean;
  nextBillingDate?: Date;
  taxAmount?: number;
  shippingAmount?: number;
}

export interface UpdateBillingRecordDto {
  id: string;
  status?: BillingStatus;
  amount?: number;
  taxAmount?: number;
  shippingAmount?: number;
  totalAmount?: number;
  dueDate?: Date;
  description?: string;
  failureReason?: string;
  errorMessage?: string;
}

export interface BillingListParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: BillingStatus[];
  type?: BillingType[];
  userId?: string[];
  subscriptionId?: string[];
  dateFrom?: Date;
  dateTo?: Date;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaymentProcessingDto {
  billingRecordId: string;
  paymentMethodId: string;
  amount: number;
  currency: string;
  description?: string;
}

export interface RefundRequestDto {
  billingRecordId: string;
  amount: number;
  reason: string;
  refundMethod?: string;
}

export interface InvoiceDto {
  id: string;
  invoiceNumber: string;
  userId: number;
  subscriptionId?: string;
  amount: number;
  currency: string;
  status: BillingStatus;
  dueDate: Date;
  paidAt?: Date;
  description?: string;
  user?: User;
  subscription?: Subscription;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateInvoiceDto {
  userId: number;
  subscriptionId?: string;
  amount: number;
  currency: string;
  description: string;
  dueDate: Date;
  invoiceNumber?: string;
}

export interface BillingAnalytics {
  totalRevenue: number;
  pendingAmount: number;
  overdueAmount: number;
  failedAmount: number;
  refundedAmount: number;
  subscriptionRevenue: number;
  consultationRevenue: number;
  medicationRevenue: number;
  monthlyTrend: RevenueTrend[];
  statusDistribution: StatusDistribution[];
  topUsers: TopUserRevenue[];
}

export interface RevenueTrend {
  month: string;
  revenue: number;
  subscriptionCount: number;
  consultationCount: number;
}

export interface StatusDistribution {
  status: BillingStatus;
  count: number;
  amount: number;
  percentage: number;
}

export interface TopUserRevenue {
  userId: number;
  userName: string;
  totalRevenue: number;
  subscriptionCount: number;
  lastPaymentDate?: Date;
}

// Supporting interfaces
export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  profilePicture?: string;
  isActive: boolean;
}

export interface Subscription {
  id: string;
  userId: number;
  subscriptionPlanId: string;
  status: string;
  currentPrice: number;
  startDate: Date;
  nextBillingDate: Date;
}

export interface Consultation {
  id: string;
  userId: number;
  providerId?: number;
  scheduledAt: Date;
  durationMinutes: number;
  fee: number;
  status: string;
}

export interface MedicationDelivery {
  id: string;
  userId: number;
  medicationId: string;
  quantity: number;
  deliveryDate: Date;
  fee: number;
  status: string;
}

export interface MasterCurrency {
  id: string;
  code: string;
  name: string;
  symbol: string;
  isActive: boolean;
}
