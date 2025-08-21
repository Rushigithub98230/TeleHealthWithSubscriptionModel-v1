import { Subscription } from './subscription.interface';
import { PaymentMethod } from './payment.interface';

export interface BillingRecord {
  id: string;
  userId: string;
  subscriptionId?: string;
  subscription?: Subscription;
  paymentMethodId?: string;
  paymentMethod?: PaymentMethod;
  invoiceNumber: string;
  billingDate: Date;
  dueDate: Date;
  amount: number; // Base amount before tax/discount
  totalAmount: number;
  taxAmount: number;
  discountAmount: number;
  shippingAmount?: number; // Optional shipping amount
  type: BillingType; // Billing type (subscription, consultation, etc.)
  status: BillingStatus;
  description?: string;
  notes?: string;
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export type BillingStatus = 'Pending' | 'Paid' | 'Failed' | 'Overdue' | 'Cancelled' | 'Refunded' | 'PartiallyPaid';

export type BillingType = 'Subscription' | 'Consultation' | 'Medication' | 'LateFee' | 'Adjustment' | 'Refund' | 'Other';

export interface PaymentProcessingDto {
  billingRecordId: string;
  paymentMethodId: string;
  amount: number;
  currency: string;
  description?: string;
  metadata?: any;
}

export interface BillingAnalytics {
  totalRecords: number;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  overdueAmount: number;
  failedAmount: number;
  refundedAmount: number;
  averageAmount: number;
  paymentSuccessRate: number;
  monthlyTrends: MonthlyTrend[];
  statusDistribution: StatusDistribution[];
  typeDistribution: TypeDistribution[];
  paymentMethodUsage: PaymentMethodUsage[];
}

export interface MonthlyTrend {
  month: string;
  year: number;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  overdueAmount: number;
  recordCount: number;
}

export interface StatusDistribution {
  status: BillingStatus;
  count: number;
  amount: number;
  percentage: number;
}

export interface TypeDistribution {
  type: BillingType;
  count: number;
  amount: number;
  percentage: number;
}

export interface PaymentMethodUsage {
  paymentMethodId: string;
  paymentMethodType: string;
  count: number;
  amount: number;
  percentage: number;
}

export interface BillingPreferences {
  id: string;
  userId: string;
  autoPay: boolean;
  paymentReminders: boolean;
  overdueNotifications: boolean;
  invoiceDelivery: InvoiceDeliveryMethod;
  billingCycle: BillingCycle;
  taxExempt: boolean;
  currency: string;
  timezone: string;
  createdAt: Date;
  updatedAt: Date;
}

export type InvoiceDeliveryMethod = 'email' | 'postal' | 'both' | 'none';

export type BillingCycle = 'monthly' | 'quarterly' | 'yearly' | 'custom';

export interface BillingSchedule {
  id: string;
  userId: string;
  subscriptionId: string;
  billingCycle: BillingCycle;
  nextBillingDate: Date;
  lastBillingDate?: Date;
  billingDay: number;
  billingMonth?: number;
  autoRenew: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface BillingAdjustment {
  id: string;
  billingRecordId: string;
  type: AdjustmentType;
  amount: number;
  reason: string;
  description?: string;
  appliedBy: string;
  appliedAt: Date;
  createdAt: Date;
}

export type AdjustmentType = 'discount' | 'credit' | 'fee' | 'tax' | 'shipping' | 'other';

export interface BillingRefund {
  id: string;
  billingRecordId: string;
  amount: number;
  reason: string;
  status: RefundStatus;
  refundMethod: RefundMethod;
  processedAt?: Date;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

export type RefundStatus = 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled';

export type RefundMethod = 'original_payment_method' | 'credit' | 'bank_transfer' | 'check';

export interface BillingInvoice {
  id: string;
  billingRecordId: string;
  invoiceNumber: string;
  invoiceDate: Date;
  dueDate: Date;
  amount: number;
  taxAmount: number;
  totalAmount: number;
  currency: string;
  status: InvoiceStatus;
  pdfUrl?: string;
  emailSent: boolean;
  emailSentAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export type InvoiceStatus = 'draft' | 'sent' | 'viewed' | 'paid' | 'overdue' | 'cancelled';

export interface BillingPayment {
  id: string;
  billingRecordId: string;
  paymentMethodId: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  transactionId?: string;
  gatewayResponse?: any;
  processedAt?: Date;
  failureReason?: string;
  createdAt: Date;
  updatedAt: Date;
}

export type PaymentStatus = 'pending' | 'processing' | 'succeeded' | 'failed' | 'cancelled' | 'refunded';

export interface BillingTax {
  id: string;
  billingRecordId: string;
  taxType: TaxType;
  rate: number;
  amount: number;
  description?: string;
  createdAt: Date;
}

export type TaxType = 'sales_tax' | 'vat' | 'gst' | 'hst' | 'pst' | 'other';

export interface BillingDiscount {
  id: string;
  billingRecordId: string;
  discountType: DiscountType;
  amount: number;
  percentage?: number;
  code?: string;
  description?: string;
  validFrom: Date;
  validTo: Date;
  createdAt: Date;
}

export type DiscountType = 'percentage' | 'fixed_amount' | 'free_shipping' | 'buy_one_get_one' | 'other';

export interface BillingNotification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  billingRecordId?: string;
  read: boolean;
  priority: NotificationPriority;
  scheduledAt?: Date;
  sentAt?: Date;
  createdAt: Date;
}

export type NotificationType = 'payment_due' | 'payment_overdue' | 'payment_received' | 'invoice_ready' | 'refund_processed' | 'billing_error';

export type NotificationPriority = 'low' | 'medium' | 'high' | 'urgent';

export interface BillingReport {
  id: string;
  name: string;
  description: string;
  type: ReportType;
  filters: any;
  format: ReportFormat;
  status: ReportStatus;
  downloadUrl?: string;
  createdAt: Date;
  completedAt?: Date;
}

export type ReportType = 'revenue' | 'outstanding' | 'payment_history' | 'tax_summary' | 'customer_summary' | 'custom';

export type ReportFormat = 'pdf' | 'csv' | 'excel' | 'json';

export type ReportStatus = 'pending' | 'processing' | 'completed' | 'failed';

export interface BillingExport {
  id: string;
  userId: string;
  type: ExportType;
  format: ExportFormat;
  filters: any;
  status: ExportStatus;
  fileUrl?: string;
  fileSize?: number;
  expiresAt: Date;
  createdAt: Date;
  completedAt?: Date;
}

export type ExportType = 'billing_records' | 'invoices' | 'payments' | 'adjustments' | 'refunds' | 'analytics';

export type ExportFormat = 'csv' | 'json' | 'xml' | 'pdf';

export type ExportStatus = 'pending' | 'processing' | 'completed' | 'failed';

export interface BillingAudit {
  id: string;
  billingRecordId: string;
  action: AuditAction;
  userId: string;
  ipAddress?: string;
  userAgent?: string;
  changes?: any;
  metadata?: any;
  createdAt: Date;
}

export type AuditAction = 'created' | 'updated' | 'deleted' | 'status_changed' | 'amount_changed' | 'payment_processed' | 'refund_processed';

export interface BillingCompliance {
  id: string;
  billingRecordId: string;
  complianceType: ComplianceType;
  status: ComplianceStatus;
  requirements: string[];
  lastAuditDate?: Date;
  nextAuditDate?: Date;
  findings?: string[];
  remediationSteps?: string[];
  createdAt: Date;
  updatedAt: Date;
}

export type ComplianceType = 'pci_dss' | 'gdpr' | 'sox' | 'hipaa' | 'iso_27001' | 'other';

export type ComplianceStatus = 'compliant' | 'non_compliant' | 'pending_review' | 'under_review';

export interface BillingIntegration {
  id: string;
  userId: string;
  integrationType: IntegrationType;
  externalSystemId: string;
  externalSystemName: string;
  status: IntegrationStatus;
  configuration: any;
  lastSyncAt?: Date;
  nextSyncAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export type IntegrationType = 'accounting' | 'erp' | 'crm' | 'payment_gateway' | 'banking' | 'other';

export type IntegrationStatus = 'active' | 'inactive' | 'error' | 'syncing' | 'maintenance';

export interface BillingWebhook {
  id: string;
  userId: string;
  url: string;
  events: WebhookEvent[];
  status: WebhookStatus;
  secretKey: string;
  lastTriggeredAt?: Date;
  failureCount: number;
  createdAt: Date;
  updatedAt: Date;
}

export type WebhookEvent = 'billing.created' | 'billing.updated' | 'billing.paid' | 'billing.overdue' | 'payment.processed' | 'refund.processed';

export type WebhookStatus = 'active' | 'inactive' | 'error' | 'disabled';

export interface BillingTemplate {
  id: string;
  name: string;
  description: string;
  type: TemplateType;
  content: any;
  variables: string[];
  isDefault: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export type TemplateType = 'invoice' | 'receipt' | 'payment_reminder' | 'overdue_notice' | 'refund_notice';

export interface BillingRule {
  id: string;
  name: string;
  description: string;
  type: RuleType;
  conditions: RuleCondition[];
  actions: RuleAction[];
  priority: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export type RuleType = 'auto_payment' | 'late_fee' | 'discount' | 'notification' | 'status_change';

export interface RuleCondition {
  field: string;
  operator: ConditionOperator;
  value: any;
}

export type ConditionOperator = 'equals' | 'not_equals' | 'greater_than' | 'less_than' | 'contains' | 'not_contains' | 'in' | 'not_in';

export interface RuleAction {
  type: ActionType;
  parameters: any;
}

export type ActionType = 'send_notification' | 'apply_discount' | 'add_late_fee' | 'change_status' | 'process_payment' | 'create_adjustment';
