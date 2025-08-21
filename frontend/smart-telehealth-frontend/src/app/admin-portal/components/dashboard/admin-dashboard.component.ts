import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, combineLatest } from 'rxjs';
import { AdminSubscriptionService } from '../../services/admin-subscription.service';
import { AdminBillingService } from '../../services/admin-billing.service';
import { AdminAnalyticsService } from '../../services/admin-analytics.service';

interface DashboardStats {
  totalSubscriptions: number;
  activeSubscriptions: number;
  pendingSubscriptions: number;
  expiringSubscriptions: number;
  totalRevenue: number;
  pendingPayments: number;
  failedPayments: number;
}

interface QuickAction {
  title: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  count?: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="admin-dashboard">
      <!-- Header -->
      <div class="dashboard-header">
        <h1>Admin Dashboard</h1>
        <p class="subtitle">Subscription Management Overview</p>
      </div>

      <!-- Statistics Cards -->
      <div class="stats-grid">
        <div class="stat-card" [class]="'stat-' + getStatusClass('total')">
          <div class="stat-icon">
            <i class="fas fa-users"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.totalSubscriptions | number }}</h3>
            <p>Total Subscriptions</p>
          </div>
        </div>

        <div class="stat-card" [class]="'stat-' + getStatusClass('active')">
          <div class="stat-icon">
            <i class="fas fa-check-circle"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.activeSubscriptions | number }}</h3>
            <p>Active Subscriptions</p>
          </div>
        </div>

        <div class="stat-card" [class]="'stat-' + getStatusClass('pending')">
          <div class="stat-icon">
            <i class="fas fa-clock"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.pendingSubscriptions | number }}</h3>
            <p>Pending Subscriptions</p>
          </div>
        </div>

        <div class="stat-card" [class]="'stat-' + getStatusClass('expiring')">
          <div class="stat-icon">
            <i class="fas fa-exclamation-triangle"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.expiringSubscriptions | number }}</h3>
            <p>Expiring Soon</p>
          </div>
        </div>

        <div class="stat-card" [class]="'stat-' + getStatusClass('revenue')">
          <div class="stat-icon">
            <i class="fas fa-dollar-sign"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.totalRevenue | currency:'USD':'symbol':'1.0-0' }}</h3>
            <p>Total Revenue</p>
          </div>
        </div>

        <div class="stat-card" [class]="'stat-' + getStatusClass('payments')">
          <div class="stat-icon">
            <i class="fas fa-credit-card"></i>
          </div>
          <div class="stat-content">
            <h3>{{ stats.pendingPayments | number }}</h3>
            <p>Pending Payments</p>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions-section">
        <h2>Quick Actions</h2>
        <div class="quick-actions-grid">
          <div 
            *ngFor="let action of quickActions" 
            class="quick-action-card"
            [routerLink]="action.route"
            [class]="'action-' + action.color"
          >
            <div class="action-icon">
              <i [class]="action.icon"></i>
            </div>
            <div class="action-content">
              <h3>{{ action.title }}</h3>
              <p>{{ action.description }}</p>
              <span *ngIf="action.count" class="action-count">{{ action.count }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="recent-activity-section">
        <h2>Recent Activity</h2>
        <div class="activity-list">
          <div *ngFor="let activity of recentActivities" class="activity-item">
            <div class="activity-icon" [class]="'activity-' + activity.type">
              <i [class]="activity.icon"></i>
            </div>
            <div class="activity-content">
              <p class="activity-text">{{ activity.description }}</p>
              <span class="activity-time">{{ activity.time | date:'short' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading" class="loading-overlay">
        <div class="spinner"></div>
        <p>Loading dashboard data...</p>
      </div>

      <!-- Error State -->
      <div *ngIf="error" class="error-message">
        <i class="fas fa-exclamation-circle"></i>
        <p>{{ error }}</p>
        <button (click)="refreshDashboard()" class="btn-retry">Retry</button>
      </div>
    </div>
  `,
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  loading = false;
  error: string | null = null;
  stats: DashboardStats = {
    totalSubscriptions: 0,
    activeSubscriptions: 0,
    pendingSubscriptions: 0,
    expiringSubscriptions: 0,
    totalRevenue: 0,
    pendingPayments: 0,
    failedPayments: 0
  };

  quickActions: QuickAction[] = [
    {
      title: 'Manage Subscriptions',
      description: 'View and manage all subscriptions',
      icon: 'fas fa-list',
      route: '/admin-portal/subscriptions',
      color: 'primary'
    },
    {
      title: 'Subscription Plans',
      description: 'Configure subscription plans and privileges',
      icon: 'fas fa-cog',
      route: '/admin-portal/subscription-plans',
      color: 'secondary'
    },
    {
      title: 'Billing & Payments',
      description: 'Monitor billing and payment status',
      icon: 'fas fa-credit-card',
      route: '/admin-portal/billing',
      color: 'success'
    },
    {
      title: 'User Management',
      description: 'Manage users and their subscriptions',
      icon: 'fas fa-users-cog',
      route: '/admin-portal/users',
      color: 'info'
    },
    {
      title: 'Analytics',
      description: 'View detailed reports and insights',
      icon: 'fas fa-chart-bar',
      route: '/admin-portal/analytics',
      color: 'warning'
    },
    {
      title: 'Automation',
      description: 'Configure subscription automation',
      icon: 'fas fa-robot',
      route: '/admin-portal/automation',
      color: 'danger'
    }
  ];

  recentActivities: any[] = [
    {
      type: 'subscription',
      icon: 'fas fa-plus',
      description: 'New subscription created for John Doe',
      time: new Date()
    },
    {
      type: 'payment',
      icon: 'fas fa-check',
      description: 'Payment processed for subscription #12345',
      time: new Date(Date.now() - 1000 * 60 * 30)
    },
    {
      type: 'warning',
      icon: 'fas fa-exclamation-triangle',
      description: 'Subscription #12346 payment failed',
      time: new Date(Date.now() - 1000 * 60 * 60)
    }
  ];

  constructor(
    private subscriptionService: AdminSubscriptionService,
    private billingService: AdminBillingService,
    private analyticsService: AdminAnalyticsService
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
    this.error = null;

    // Load subscription statistics
    const subscriptionStats$ = this.subscriptionService.getAllSubscriptions({ pageSize: 1000 });
    const activeSubscriptions$ = this.subscriptionService.getActiveSubscriptions();
    const expiringSubscriptions$ = this.subscriptionService.getSubscriptionsNearExpiration(7);

    combineLatest([
      subscriptionStats$,
      activeSubscriptions$,
      expiringSubscriptions$
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: ([allSubs, activeSubs, expiringSubs]) => {
        if (allSubs.statusCode === 200 && allSubs.data) {
          this.stats.totalSubscriptions = allSubs.data.meta.totalRecords;
        }
        
        if (activeSubs.statusCode === 200 && activeSubs.data) {
          this.stats.activeSubscriptions = activeSubs.data.length;
        }
        
        if (expiringSubs.statusCode === 200 && expiringSubs.data) {
          this.stats.expiringSubscriptions = expiringSubs.data.length;
        }

        // Calculate pending subscriptions
        this.stats.pendingSubscriptions = this.stats.totalSubscriptions - this.stats.activeSubscriptions;
        
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load dashboard data';
        this.loading = false;
      }
    });

    // TODO: Load billing statistics when billing service is implemented
    // TODO: Load analytics data when analytics service is implemented
  }

  getStatusClass(type: string): string {
    switch (type) {
      case 'total':
        return 'info';
      case 'active':
        return 'success';
      case 'pending':
        return 'warning';
      case 'expiring':
        return 'danger';
      case 'revenue':
        return 'primary';
      case 'payments':
        return 'secondary';
      default:
        return 'info';
    }
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }
}
