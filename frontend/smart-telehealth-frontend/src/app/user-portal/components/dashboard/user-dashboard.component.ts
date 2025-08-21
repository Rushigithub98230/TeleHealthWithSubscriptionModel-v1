import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';

import { UserSubscriptionService } from '../../services/user-subscription.service';
import { UserBillingService } from '../../services/user-billing.service';
import { UserPaymentService } from '../../services/user-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { Subscription, SubscriptionStatus } from '../../models/subscription.interface';
import { BillingRecord, BillingStatus } from '../../models/billing.interface';
import { PaymentMethod } from '../../models/payment.interface';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatBadgeModule
  ],
  template: `
    <div class="user-dashboard-container">
      <!-- Welcome Section -->
      <div class="welcome-section">
        <h1>Welcome back, {{ user?.firstName || 'User' }}!</h1>
        <p>Here's what's happening with your telehealth services today</p>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <div class="action-card" (click)="navigateToSubscriptions()">
          <div class="action-icon">
            <i class="fas fa-subscription"></i>
          </div>
          <div class="action-content">
            <h3>My Subscriptions</h3>
            <p>Manage your subscription plans</p>
          </div>
          <div class="action-arrow">
            <i class="fas fa-chevron-right"></i>
          </div>
        </div>

        <div class="action-card" (click)="navigateToBilling()">
          <div class="action-icon">
            <i class="fas fa-credit-card"></i>
          </div>
          <div class="action-content">
            <h3>Billing & Payments</h3>
            <p>View invoices and payment history</p>
          </div>
          <div class="action-arrow">
            <i class="fas fa-chevron-right"></i>
          </div>
        </div>

        <div class="action-card" (click)="navigateToPaymentMethods()">
          <div class="action-icon">
            <i class="fas fa-wallet"></i>
          </div>
          <div class="action-content">
            <h3>Payment Methods</h3>
            <p>Manage your payment options</p>
          </div>
          <div class="action-arrow">
            <i class="fas fa-chevron-right"></i>
          </div>
        </div>

        <div class="action-card" (click)="navigateToSubscriptionPlans()">
          <div class="action-icon">
            <i class="fas fa-rocket"></i>
          </div>
          <div class="action-content">
            <h3>Browse Plans</h3>
            <p>Find the perfect plan for you</p>
          </div>
          <div class="action-arrow">
            <i class="fas fa-chevron-right"></i>
          </div>
        </div>
      </div>

      <!-- Subscription Summary -->
      <div class="subscription-summary" *ngIf="!loading && subscriptions.length > 0">
        <h2>Subscription Overview</h2>
        <div class="summary-cards">
          <div class="summary-card active">
            <div class="card-icon">
              <i class="fas fa-check-circle"></i>
            </div>
            <div class="card-content">
              <h3>Active Subscriptions</h3>
              <span class="count">{{ getActiveSubscriptionsCount() }}</span>
              <span class="label">Currently active</span>
            </div>
          </div>

          <div class="summary-card trial" *ngIf="getTrialSubscriptionsCount() > 0">
            <div class="card-icon">
              <i class="fas fa-gift"></i>
            </div>
            <div class="card-content">
              <h3>Trial Subscriptions</h3>
              <span class="count">{{ getTrialSubscriptionsCount() }}</span>
              <span class="label">Free trial period</span>
            </div>
          </div>

          <div class="summary-card monthly-cost">
            <div class="card-icon">
              <i class="fas fa-dollar-sign"></i>
            </div>
            <div class="card-content">
              <h3>Monthly Cost</h3>
              <span class="count">{{ getTotalMonthlyCost() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="label">Total monthly</span>
            </div>
          </div>

          <div class="summary-card next-payment">
            <div class="card-icon">
              <i class="fas fa-calendar"></i>
            </div>
            <div class="card-content">
              <h3>Next Payment</h3>
              <span class="count">{{ getNextPaymentDate() | date:'shortDate' }}</span>
              <span class="label">Due date</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Billing Summary -->
      <div class="billing-summary" *ngIf="!loading">
        <h2>Billing Summary</h2>
        <div class="summary-cards">
          <div class="summary-card pending">
            <div class="card-icon">
              <i class="fas fa-clock"></i>
            </div>
            <div class="card-content">
              <h3>Pending Payments</h3>
              <span class="count">{{ getPendingAmount() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="label">Awaiting payment</span>
            </div>
          </div>

          <div class="summary-card overdue" *ngIf="getOverdueAmount() > 0">
            <div class="card-icon">
              <i class="fas fa-exclamation-triangle"></i>
            </div>
            <div class="card-content">
              <h3>Overdue</h3>
              <span class="count">{{ getOverdueAmount() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="label">Past due</span>
            </div>
          </div>

          <div class="summary-card paid">
            <div class="card-icon">
              <i class="fas fa-check"></i>
            </div>
            <div class="card-content">
              <h3>This Month</h3>
              <span class="count">{{ getPaidAmount() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="label">Already paid</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="recent-activity" *ngIf="!loading">
        <h2>Recent Activity</h2>
        <div class="activity-list">
          <div class="activity-item" *ngFor="let activity of getRecentActivity()">
            <div class="activity-icon" [class]="getActivityIconClass(activity.type)">
              <i class="fas" [class]="getActivityIcon(activity.type)"></i>
            </div>
            <div class="activity-content">
              <h4>{{ activity.title }}</h4>
              <p>{{ activity.description }}</p>
              <span class="activity-time">{{ activity.time | date:'short' }}</span>
            </div>
            <div class="activity-action">
              <button class="btn btn-outline btn-sm" (click)="handleActivityAction(activity)">
                {{ getActivityActionText(activity.type) }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Payment Methods Summary -->
      <div class="payment-methods-summary" *ngIf="!loading && paymentMethods.length > 0">
        <h2>Payment Methods</h2>
        <div class="payment-methods-grid">
          <div class="payment-method-card" *ngFor="let method of paymentMethods.slice(0, 3)">
            <div class="method-icon">
              <i class="fas" [class]="getPaymentTypeIcon(method.type)"></i>
            </div>
            <div class="method-info">
              <h4>{{ maskCardNumber(method.last4) }}</h4>
              <p>{{ method.cardholderName }}</p>
              <span class="method-status" [class]="method.isDefault ? 'default' : 'active'">
                {{ method.isDefault ? 'Default' : 'Active' }}
              </span>
            </div>
          </div>
        </div>
        <div class="view-all">
          <button class="btn btn-outline" (click)="navigateToPaymentMethods()">
            View All Payment Methods
          </button>
        </div>
      </div>

      <!-- No Subscriptions State -->
      <div class="no-subscriptions" *ngIf="!loading && subscriptions.length === 0">
        <div class="empty-state">
          <i class="fas fa-subscription"></i>
          <h3>No Active Subscriptions</h3>
          <p>Get started with our telehealth services by choosing a subscription plan that fits your needs.</p>
          <button class="btn btn-primary" (click)="navigateToSubscriptionPlans()">
            Browse Subscription Plans
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-section" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Loading your dashboard...</p>
      </div>
    </div>
  `,
  styleUrls: ['./user-dashboard.component.scss']
})
export class UserDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  user: any = null;
  subscriptions: Subscription[] = [];
  billingRecords: BillingRecord[] = [];
  paymentMethods: PaymentMethod[] = [];
  loading = false;

  constructor(
    private userSubscriptionService: UserSubscriptionService,
    private userBillingService: UserBillingService,
    private userPaymentService: UserPaymentService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData(): void {
    this.loading = true;

    // Load user data
    this.user = this.authService.getCurrentUser();

    // Load subscriptions
    this.userSubscriptionService.getUserSubscriptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.subscriptions = response.data;
          }
        },
        error: (error) => {
          console.error('Failed to load subscriptions:', error);
        }
      });

    // Load billing records
    this.userBillingService.getUserBillingRecords()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.billingRecords = response.data;
          }
        },
        error: (error) => {
          console.error('Failed to load billing records:', error);
        }
      });

    // Load payment methods
    this.userPaymentService.getUserPaymentMethods()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.paymentMethods = response.data;
          }
        },
        error: (error) => {
          console.error('Failed to load payment methods:', error);
        },
        complete: () => {
          this.loading = false;
        }
      });
  }

  // Navigation methods
  navigateToSubscriptions(): void {
    this.router.navigate(['/web/subscriptions']);
  }

  navigateToBilling(): void {
    this.router.navigate(['/web/billing']);
  }

  navigateToPaymentMethods(): void {
    this.router.navigate(['/web/payment-methods']);
  }

  navigateToSubscriptionPlans(): void {
    this.router.navigate(['/web/subscription-plans']);
  }

  // Subscription summary methods
  getActiveSubscriptionsCount(): number {
    return this.subscriptions.filter(s => s.status === 'Active').length;
  }

  getTrialSubscriptionsCount(): number {
    return this.subscriptions.filter(s => s.status === 'TrialActive').length;
  }

  getTotalMonthlyCost(): number {
    return this.subscriptions
      .filter(s => s.status === 'Active')
      .reduce((total, s) => total + (s.currentPrice || 0), 0);
  }

  getNextPaymentDate(): Date | null {
    const activeSubscriptions = this.subscriptions.filter(s => s.status === 'Active');
    if (activeSubscriptions.length === 0) return null;
    
    return activeSubscriptions.reduce((earliest: Date | null, current: Subscription) => {
      if (!earliest) return current.nextBillingDate || null;
      if (!current.nextBillingDate) return earliest;
      return current.nextBillingDate < earliest ? current.nextBillingDate : earliest;
    }, null as Date | null);
  }

  // Billing summary methods
  getPendingAmount(): number {
    return this.billingRecords
      .filter(record => record.status === 'Pending')
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  getOverdueAmount(): number {
    return this.billingRecords
      .filter(record => record.status === 'Overdue')
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  getPaidAmount(): number {
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();
    
    return this.billingRecords
      .filter(record => {
        const recordDate = new Date(record.billingDate);
        return record.status === 'Paid' && 
               recordDate.getMonth() === currentMonth && 
               recordDate.getFullYear() === currentYear;
      })
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  // Recent activity methods
  getRecentActivity(): Array<{
    type: string;
    title: string;
    description: string;
    time: Date;
    data: any;
  }> {
    const activities: Array<{
      type: string;
      title: string;
      description: string;
      time: Date;
      data: any;
    }> = [];

    // Add subscription activities
    this.subscriptions.slice(0, 3).forEach(sub => {
      activities.push({
        type: 'subscription',
        title: `${sub.subscriptionPlan?.name || 'Subscription'} ${sub.status}`,
        description: `Your subscription is now ${sub.status.toLowerCase()}`,
        time: sub.updatedAt,
        data: sub
      });
    });

    // Add billing activities
    this.billingRecords.slice(0, 3).forEach(record => {
      activities.push({
        type: 'billing',
        title: `Invoice ${record.invoiceNumber}`,
        description: `${record.status} - ${record.description || 'Billing record'}`,
        time: record.updatedAt,
        data: record
      });
    });

    // Sort by time and return top 5
    return activities
      .sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())
      .slice(0, 5);
  }

  getActivityIconClass(type: string): string {
    const classes: { [key: string]: string } = {
      'subscription': 'subscription-activity',
      'billing': 'billing-activity',
      'payment': 'payment-activity'
    };
    return classes[type] || 'default-activity';
  }

  getActivityIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'subscription': 'fa-subscription',
      'billing': 'fa-receipt',
      'payment': 'fa-credit-card'
    };
    return icons[type] || 'fa-info-circle';
  }

  getActivityActionText(type: string): string {
    const actions: { [key: string]: string } = {
      'subscription': 'View',
      'billing': 'Pay',
      'payment': 'Details'
    };
    return actions[type] || 'View';
  }

  handleActivityAction(activity: any): void {
    switch (activity.type) {
      case 'subscription':
        this.router.navigate(['/web/subscriptions', activity.data.id]);
        break;
      case 'billing':
        this.router.navigate(['/web/billing', activity.data.id]);
        break;
      case 'payment':
        this.router.navigate(['/web/payment-methods']);
        break;
    }
  }

  // Payment method methods
  getPaymentTypeIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'visa': 'fa-cc-visa',
      'mastercard': 'fa-cc-mastercard',
      'amex': 'fa-cc-amex',
      'discover': 'fa-cc-discover',
      'jcb': 'fa-cc-jcb',
      'diners': 'fa-cc-diners-club'
    };
    return icons[type.toLowerCase()] || 'fa-credit-card';
  }

  maskCardNumber(last4: string): string {
    return `**** **** **** ${last4}`;
  }
}
