import { PaymentMethod } from './payment.interface';

export interface Subscription {
  id: string;
  userId: string;
  subscriptionPlanId: string;
  subscriptionPlan?: SubscriptionPlan;
  status: SubscriptionStatus;
  startDate: Date;
  endDate?: Date;
  nextBillingDate?: Date;
  lastBillingDate?: Date;
  currentPrice: number;
  originalPrice: number;
  billingCycleId: number;
  billingCycle?: BillingCycle;
  autoRenew: boolean;
  trialStartDate?: Date;
  trialEndDate?: Date;
  isTrialActive: boolean;
  cancellationDate?: Date;
  cancellationReason?: string;
  pausedDate?: Date;
  pausedReason?: string;
  resumedDate?: Date;
  paymentMethodId?: string;
  paymentMethod?: PaymentMethod;
  privileges?: SubscriptionPrivilege[];
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export type SubscriptionStatus = 'Active' | 'Paused' | 'Cancelled' | 'Expired' | 'Suspended' | 'TrialActive' | 'TrialExpired' | 'PaymentFailed' | 'Pending' | 'Overdue';

export interface SubscriptionPlan {
  id: string;
  name: string;
  shortDescription: string;
  longDescription: string;
  price: number;
  discountedPrice?: number;
  billingCycleId: number;
  billingCycle?: BillingCycle;
  category?: SubscriptionCategory;
  isActive: boolean;
  isFeatured: boolean;
  isMostPopular: boolean;
  isTrialAllowed: boolean;
  trialDurationInDays?: number;
  maxUsers?: number;
  maxStorage?: number;
  privileges?: SubscriptionPrivilege[];
  terms?: string;
  discountValidUntil?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface BillingCycle {
  id: number;
  name: string;
  description: string;
  months: number;
  discountPercentage?: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionCategory {
  id: string;
  name: string;
  description: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionPrivilege {
  id: string;
  subscriptionId?: string;
  subscriptionPlanId?: string;
  privilegeId: string;
  privilege?: Privilege;
  value?: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface Privilege {
  id: string;
  name: string;
  description: string;
  type: PrivilegeType;
  unit?: string;
  defaultValue?: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export type PrivilegeType = 'Feature' | 'Limit' | 'Access' | 'Service' | 'Resource';

export interface CreateSubscriptionDto {
  subscriptionPlanId: string;
  paymentMethodId: string;
  billingCycleId: number;
  autoRenew?: boolean;
  metadata?: any;
}

export interface UpdateSubscriptionDto {
  autoRenew?: boolean;
  paymentMethodId?: string;
  billingCycleId?: number;
  metadata?: any;
}

export interface SubscriptionUsage {
  id: string;
  subscriptionId: string;
  privilegeId: string;
  privilege?: Privilege;
  usedValue: number;
  remainingValue: number;
  resetDate: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface SubscriptionAnalytics {
  totalSubscriptions: number;
  activeSubscriptions: number;
  trialSubscriptions: number;
  cancelledSubscriptions: number;
  pausedSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  averageSubscriptionValue: number;
  churnRate: number;
  growthRate: number;
  planDistribution: PlanDistribution[];
  statusDistribution: StatusDistribution[];
  monthlyTrends: MonthlyTrend[];
  privilegeUsage: PrivilegeUsage[];
}

export interface PlanDistribution {
  planId: string;
  planName: string;
  count: number;
  percentage: number;
  revenue: number;
}

export interface StatusDistribution {
  status: SubscriptionStatus;
  count: number;
  percentage: number;
}

export interface MonthlyTrend {
  month: string;
  year: number;
  newSubscriptions: number;
  cancelledSubscriptions: number;
  revenue: number;
  activeSubscriptions: number;
}

export interface PrivilegeUsage {
  privilegeId: string;
  privilegeName: string;
  totalAllocated: number;
  totalUsed: number;
  utilizationRate: number;
}

export interface SubscriptionHistory {
  id: string;
  subscriptionId: string;
  action: HistoryAction;
  oldValue?: any;
  newValue?: any;
  reason?: string;
  performedBy: string;
  performedAt: Date;
  metadata?: any;
}

export type HistoryAction = 'created' | 'activated' | 'paused' | 'resumed' | 'cancelled' | 'expired' | 'upgraded' | 'downgraded' | 'price_changed' | 'billing_cycle_changed' | 'payment_method_changed';

export interface SubscriptionNotification {
  id: string;
  subscriptionId: string;
  type: NotificationType;
  title: string;
  message: string;
  priority: NotificationPriority;
  read: boolean;
  scheduledAt?: Date;
  sentAt?: Date;
  createdAt: Date;
}

export type NotificationType = 'trial_ending' | 'trial_expired' | 'payment_due' | 'payment_failed' | 'subscription_expiring' | 'subscription_expired' | 'privilege_limit_reached' | 'billing_cycle_changed' | 'price_changed';

export type NotificationPriority = 'low' | 'medium' | 'high' | 'urgent';

export interface SubscriptionReport {
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

export type ReportType = 'subscription_summary' | 'revenue_analysis' | 'churn_analysis' | 'usage_analysis' | 'plan_comparison' | 'custom';

export type ReportFormat = 'pdf' | 'csv' | 'excel' | 'json';

export type ReportStatus = 'pending' | 'processing' | 'completed' | 'failed';

export interface SubscriptionExport {
  id: string;
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

export type ExportType = 'subscriptions' | 'usage' | 'analytics' | 'history' | 'notifications';

export type ExportFormat = 'csv' | 'json' | 'xml' | 'pdf';

export type ExportStatus = 'pending' | 'processing' | 'completed' | 'failed';

export interface SubscriptionAudit {
  id: string;
  subscriptionId: string;
  action: AuditAction;
  userId: string;
  ipAddress?: string;
  userAgent?: string;
  changes?: any;
  metadata?: any;
  createdAt: Date;
}

export type AuditAction = 'created' | 'updated' | 'deleted' | 'status_changed' | 'price_changed' | 'billing_cycle_changed' | 'payment_method_changed' | 'privileges_updated';

export interface SubscriptionCompliance {
  id: string;
  subscriptionId: string;
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

export type ComplianceType = 'gdpr' | 'hipaa' | 'sox' | 'iso_27001' | 'pci_dss' | 'other';

export type ComplianceStatus = 'compliant' | 'non_compliant' | 'pending_review' | 'under_review';

export interface SubscriptionIntegration {
  id: string;
  subscriptionId: string;
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

export type IntegrationType = 'crm' | 'erp' | 'accounting' | 'billing' | 'support' | 'other';

export type IntegrationStatus = 'active' | 'inactive' | 'error' | 'syncing' | 'maintenance';

export interface SubscriptionWebhook {
  id: string;
  url: string;
  events: WebhookEvent[];
  status: WebhookStatus;
  secretKey: string;
  lastTriggeredAt?: Date;
  failureCount: number;
  createdAt: Date;
  updatedAt: Date;
}

export type WebhookEvent = 'subscription.created' | 'subscription.updated' | 'subscription.cancelled' | 'subscription.paused' | 'subscription.resumed' | 'subscription.expired' | 'payment.failed' | 'trial.ending' | 'trial.expired';

export type WebhookStatus = 'active' | 'inactive' | 'error' | 'disabled';

export interface SubscriptionTemplate {
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

export type TemplateType = 'welcome_email' | 'trial_ending' | 'payment_failed' | 'subscription_expiring' | 'cancellation_confirmation' | 'reactivation_offer';

export interface SubscriptionRule {
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

export type RuleType = 'auto_pause' | 'auto_cancel' | 'price_adjustment' | 'privilege_adjustment' | 'notification' | 'integration_trigger';

export interface RuleCondition {
  field: string;
  operator: ConditionOperator;
  value: any;
}

export type ConditionOperator = 'equals' | 'not_equals' | 'greater_than' | 'less_than' | 'contains' | 'not_contains' | 'in' | 'not_in' | 'is_null' | 'is_not_null';

export interface RuleAction {
  type: ActionType;
  parameters: any;
}

export type ActionType = 'send_notification' | 'adjust_price' | 'adjust_privileges' | 'pause_subscription' | 'cancel_subscription' | 'trigger_integration' | 'create_adjustment';

export interface SubscriptionMetrics {
  id: string;
  subscriptionId: string;
  date: Date;
  activeUsers: number;
  totalUsage: number;
  revenue: number;
  churnRisk: number;
  engagementScore: number;
  createdAt: Date;
}

export interface SubscriptionForecast {
  id: string;
  subscriptionId: string;
  forecastDate: Date;
  predictedRevenue: number;
  churnProbability: number;
  growthPotential: number;
  confidence: number;
  factors: string[];
  createdAt: Date;
}

export interface SubscriptionComparison {
  plan1: SubscriptionPlan;
  plan2: SubscriptionPlan;
  differences: PlanDifference[];
  recommendations: string[];
}

export interface PlanDifference {
  feature: string;
  plan1Value: any;
  plan2Value: any;
  difference: string;
  impact: 'positive' | 'negative' | 'neutral';
}
