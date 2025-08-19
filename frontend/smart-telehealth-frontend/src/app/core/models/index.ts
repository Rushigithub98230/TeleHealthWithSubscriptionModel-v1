export * from './json-model.interface';

export type SubscriptionStatus = 'Active' | 'Inactive' | 'Suspended' | 'Cancelled' | 'Expired' | 'Pending';
export type BillingCycle = 'Monthly' | 'Quarterly' | 'Yearly' | 'OneTime' | 'SemiAnnually' | 'Annually';

export interface UserRole {
	id: number;
	name: string;
	description?: string;
	sortOrder?: number;
}

export interface User {
	id: number;
	firstName: string;
	lastName: string;
	email: string;
	role?: string;
	profilePicture?: string;
}

export interface SubscriptionPlanPrivilegeRef {
	privilege?: { name: string };
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
	privileges: SubscriptionPlanPrivilegeRef[];
	createdAt?: Date | string;
	updatedAt?: Date | string;
}

export interface Subscription {
	id: number;
	userId: number;
	subscriptionPlanId: number;
	status: SubscriptionStatus;
	startDate: string | Date;
	endDate: string | Date;
	billingCycle: BillingCycle;
	amount: number;
	discountPercentage?: number;
	notes?: string;
	autoRenew?: boolean;
	sendNotifications?: boolean;
	user?: User;
	subscriptionPlan?: SubscriptionPlan;
}
