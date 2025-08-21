export interface PaymentMethod {
  id: string;
  userId: string;
  type: PaymentMethodType;
  cardholderName: string;
  last4: string;
  expiryMonth: string;
  expiryYear: string;
  brand: string;
  isDefault: boolean;
  isActive: boolean;
  billingAddress?: BillingAddress;
  lastUsedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreatePaymentMethodDto {
  cardNumber: string;
  cardholderName: string;
  expiryMonth: string;
  expiryYear: string;
  cvv: string;
  billingAddress?: BillingAddress;
  isDefault?: boolean;
}

export interface UpdatePaymentMethodDto {
  cardholderName?: string;
  expiryMonth?: string;
  expiryYear?: string;
  billingAddress?: BillingAddress;
  isDefault?: boolean;
  isActive?: boolean;
}

export interface BillingAddress {
  line1: string;
  line2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

export type PaymentMethodType = 'visa' | 'mastercard' | 'amex' | 'discover' | 'jcb' | 'diners';

export interface PaymentTransaction {
  id: string;
  paymentMethodId: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  type: PaymentType;
  description?: string;
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export type PaymentStatus = 'pending' | 'processing' | 'succeeded' | 'failed' | 'cancelled' | 'refunded';

export type PaymentType = 'subscription' | 'one_time' | 'refund' | 'adjustment';

export interface PaymentIntent {
  id: string;
  amount: number;
  currency: string;
  status: PaymentIntentStatus;
  paymentMethodId?: string;
  clientSecret?: string;
  description?: string;
  metadata?: any;
  createdAt: Date;
  updatedAt: Date;
}

export type PaymentIntentStatus = 'requires_payment_method' | 'requires_confirmation' | 'requires_action' | 'processing' | 'requires_capture' | 'canceled' | 'succeeded';

export interface PaymentWebhook {
  id: string;
  type: string;
  data: any;
  createdAt: Date;
  processed: boolean;
}

export interface PaymentError {
  code: string;
  message: string;
  param?: string;
  type?: string;
}

export interface PaymentMethodValidation {
  isValid: boolean;
  errors?: PaymentError[];
  cardType?: PaymentMethodType;
  last4?: string;
}

export interface PaymentMethodTest {
  id: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  testMode: boolean;
  createdAt: Date;
}

export interface PaymentMethodSecurity {
  id: string;
  paymentMethodId: string;
  fraudScore?: number;
  riskLevel: RiskLevel;
  lastVerifiedAt?: Date;
  verificationMethod?: VerificationMethod;
  createdAt: Date;
  updatedAt: Date;
}

export type RiskLevel = 'low' | 'medium' | 'high';

export type VerificationMethod = '3d_secure' | 'cvv' | 'address' | 'identity';

export interface PaymentMethodPreferences {
  id: string;
  paymentMethodId: string;
  autoPay: boolean;
  notifications: boolean;
  fraudAlerts: boolean;
  spendingLimits?: SpendingLimits;
  createdAt: Date;
  updatedAt: Date;
}

export interface SpendingLimits {
  daily?: number;
  weekly?: number;
  monthly?: number;
  yearly?: number;
}

export interface PaymentMethodAnalytics {
  totalTransactions: number;
  totalAmount: number;
  averageAmount: number;
  successRate: number;
  failureRate: number;
  mostUsedFor: string[];
  lastUsedAt?: Date;
  createdAt: Date;
}

export interface PaymentMethodUsage {
  id: string;
  paymentMethodId: string;
  subscriptionId?: string;
  billingRecordId?: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  createdAt: Date;
}

export interface PaymentMethodCountry {
  code: string;
  name: string;
  supported: boolean;
  currency: string;
  paymentMethods: PaymentMethodType[];
}

export interface PaymentMethodTypeInfo {
  type: PaymentMethodType;
  name: string;
  description: string;
  logo: string;
  supportedCountries: string[];
  features: string[];
  securityFeatures: string[];
}

export interface PaymentMethodFormData {
  cardNumber: string;
  cardholderName: string;
  expiryMonth: string;
  expiryYear: string;
  cvv: string;
  billingAddress: BillingAddress;
  isDefault: boolean;
  acceptTerms: boolean;
}

export interface PaymentMethodSearchParams {
  type?: PaymentMethodType;
  isDefault?: boolean;
  isActive?: boolean;
  country?: string;
  searchTerm?: string;
}

export interface PaymentMethodBulkAction {
  action: 'activate' | 'deactivate' | 'delete' | 'setDefault';
  paymentMethodIds: string[];
}

export interface PaymentMethodExport {
  format: 'csv' | 'json' | 'pdf';
  filters?: PaymentMethodSearchParams;
  includeInactive?: boolean;
}

export interface PaymentMethodImport {
  file: File;
  format: 'csv' | 'json';
  updateExisting?: boolean;
  validateOnly?: boolean;
}

export interface PaymentMethodTemplate {
  id: string;
  name: string;
  description: string;
  type: PaymentMethodType;
  billingAddress: BillingAddress;
  isDefault: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PaymentMethodBackup {
  id: string;
  paymentMethodId: string;
  backupData: any;
  backupType: 'encrypted' | 'hashed';
  createdAt: Date;
  expiresAt: Date;
}

export interface PaymentMethodAudit {
  id: string;
  paymentMethodId: string;
  action: 'created' | 'updated' | 'deleted' | 'activated' | 'deactivated' | 'set_default';
  userId: string;
  ipAddress?: string;
  userAgent?: string;
  changes?: any;
  createdAt: Date;
}

export interface PaymentMethodNotification {
  id: string;
  paymentMethodId: string;
  type: 'expiry_warning' | 'security_alert' | 'usage_limit' | 'fraud_detected';
  title: string;
  message: string;
  priority: 'low' | 'medium' | 'high' | 'critical';
  read: boolean;
  createdAt: Date;
  readAt?: Date;
}

export interface PaymentMethodReport {
  id: string;
  name: string;
  description: string;
  type: 'usage' | 'security' | 'analytics' | 'compliance';
  filters: any;
  format: 'pdf' | 'csv' | 'json';
  status: 'pending' | 'processing' | 'completed' | 'failed';
  downloadUrl?: string;
  createdAt: Date;
  completedAt?: Date;
}

export interface PaymentMethodCompliance {
  id: string;
  paymentMethodId: string;
  complianceType: 'pci_dss' | 'gdpr' | 'sox' | 'hipaa';
  status: 'compliant' | 'non_compliant' | 'pending_review';
  lastAuditDate?: Date;
  nextAuditDate?: Date;
  findings?: string[];
  remediationSteps?: string[];
  createdAt: Date;
  updatedAt: Date;
}
