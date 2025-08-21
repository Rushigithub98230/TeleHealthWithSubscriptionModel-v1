import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AdminAnalyticsService, 
  SubscriptionAnalytics, 
  RevenueAnalytics, 
  UserAnalytics,
  JsonModel 
} from '../../services/admin-analytics.service';
import { AdminSubscriptionService } from '../../services/admin-subscription.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MatSnackBarModule],
  template: `
    <div class="analytics-dashboard-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Analytics Dashboard</h1>
          <p>Comprehensive insights into subscription performance, revenue, and user behavior</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-secondary" 
            (click)="exportAnalytics()"
            [disabled]="loading">
            <i class="fas fa-download"></i> Export Report
          </button>
          <button 
            class="btn btn-primary" 
            (click)="refreshAnalytics()"
            [disabled]="loading">
            <i class="fas fa-sync-alt"></i> Refresh
          </button>
        </div>
      </div>

      <!-- Date Range Filter -->
      <div class="filter-section">
        <form [formGroup]="filterForm" class="filter-form">
          <div class="filter-row">
            <div class="filter-group">
              <label for="dateRange">Date Range</label>
              <select id="dateRange" formControlName="dateRange" class="form-control">
                <option value="7">Last 7 Days</option>
                <option value="30">Last 30 Days</option>
                <option value="90">Last 90 Days</option>
                <option value="365">Last Year</option>
                <option value="custom">Custom Range</option>
              </select>
            </div>
            <div class="filter-group" *ngIf="filterForm.get('dateRange')?.value === 'custom'">
              <label for="startDate">Start Date</label>
              <input 
                type="date" 
                id="startDate" 
                formControlName="startDate" 
                class="form-control">
            </div>
            <div class="filter-group" *ngIf="filterForm.get('dateRange')?.value === 'custom'">
              <label for="endDate">End Date</label>
              <input 
                type="date" 
                id="endDate" 
                formControlName="endDate" 
                class="form-control">
            </div>
            <div class="filter-group">
              <button type="button" class="btn btn-outline" (click)="applyFilters()">
                Apply Filters
              </button>
            </div>
          </div>
        </form>
      </div>

      <!-- Key Metrics Overview -->
      <div class="metrics-overview">
        <div class="metrics-grid">
          <!-- Subscription Metrics -->
          <div class="metric-card subscription-metrics">
            <div class="card-header">
              <h3>Subscription Overview</h3>
              <i class="fas fa-users"></i>
            </div>
            <div class="metrics-content" *ngIf="subscriptionAnalytics">
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Total Subscriptions</span>
                  <span class="metric-value">{{ subscriptionAnalytics.totalSubscriptions | number }}</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Active Subscriptions</span>
                  <span class="metric-value">{{ subscriptionAnalytics.activeSubscriptions | number }}</span>
                </div>
              </div>
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">New This Month</span>
                  <span class="metric-value positive">{{ subscriptionAnalytics.newSubscriptionsThisMonth | number }}</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Cancelled This Month</span>
                  <span class="metric-value negative">{{ subscriptionAnalytics.cancelledSubscriptionsThisMonth | number }}</span>
                </div>
              </div>
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Growth Rate</span>
                  <span class="metric-value" [class]="subscriptionAnalytics.subscriptionGrowthRate >= 0 ? 'positive' : 'negative'">
                    {{ subscriptionAnalytics.subscriptionGrowthRate >= 0 ? '+' : '' }}{{ subscriptionAnalytics.subscriptionGrowthRate | number:'1.1-1' }}%
                  </span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Churn Rate</span>
                  <span class="metric-value negative">{{ subscriptionAnalytics.churnRate | number:'1.1-1' }}%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Revenue Metrics -->
          <div class="metric-card revenue-metrics">
            <div class="card-header">
              <h3>Revenue Overview</h3>
              <i class="fas fa-dollar-sign"></i>
            </div>
            <div class="metrics-content" *ngIf="revenueAnalytics">
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Total Revenue</span>
                  <span class="metric-value">{{ revenueAnalytics.totalRevenue | currency:'USD':'symbol':'1.0-0' }}</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Monthly Revenue</span>
                  <span class="metric-value">{{ revenueAnalytics.monthlyRevenue | currency:'USD':'symbol':'1.0-0' }}</span>
                </div>
              </div>
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Revenue Growth</span>
                  <span class="metric-value" [class]="revenueAnalytics.revenueGrowth >= 0 ? 'positive' : 'negative'">
                    {{ revenueAnalytics.revenueGrowth >= 0 ? '+' : '' }}{{ revenueAnalytics.revenueGrowth | number:'1.1-1' }}%
                  </span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Avg Order Value</span>
                  <span class="metric-value">{{ revenueAnalytics.averageOrderValue | currency:'USD':'symbol':'1.0-0' }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- User Metrics -->
          <div class="metric-card user-metrics">
            <div class="card-header">
              <h3>User Overview</h3>
              <i class="fas fa-user"></i>
            </div>
            <div class="metrics-content" *ngIf="userAnalytics">
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Total Users</span>
                  <span class="metric-value">{{ userAnalytics.totalUsers | number }}</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Active Users</span>
                  <span class="metric-value">{{ userAnalytics.activeUsers | number }}</span>
                </div>
              </div>
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">New This Month</span>
                  <span class="metric-value positive">{{ userAnalytics.newUsersThisMonth | number }}</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">User Growth</span>
                  <span class="metric-value" [class]="userAnalytics.userGrowthRate >= 0 ? 'positive' : 'negative'">
                    {{ userAnalytics.userGrowthRate >= 0 ? '+' : '' }}{{ userAnalytics.userGrowthRate | number:'1.1-1' }}%
                  </span>
                </div>
              </div>
              <div class="metric-row">
                <div class="metric-item">
                  <span class="metric-label">Engagement Rate</span>
                  <span class="metric-value">{{ userAnalytics.userEngagementRate | number:'1.1-1' }}%</span>
                </div>
                <div class="metric-item">
                  <span class="metric-label">Retention Rate</span>
                  <span class="metric-value">{{ subscriptionAnalytics?.userRetentionRate | number:'1.1-1' }}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts and Detailed Analytics -->
      <div class="charts-section">
        <div class="charts-grid">
          <!-- Top Subscription Plans -->
          <div class="chart-card">
            <div class="chart-header">
              <h3>Top Subscription Plans</h3>
              <button class="btn btn-sm btn-outline" (click)="viewAllPlans()">View All</button>
            </div>
            <div class="chart-content" *ngIf="subscriptionAnalytics?.topPlans">
              <div class="plan-ranking">
                <div 
                  *ngFor="let plan of subscriptionAnalytics?.topPlans || []; let i = index" 
                  class="plan-item"
                  [class.top-plan]="i === 0">
                  <div class="plan-rank">{{ i + 1 }}</div>
                  <div class="plan-info">
                    <div class="plan-name">{{ plan.planName }}</div>
                    <div class="plan-stats">
                      <span class="subscription-count">{{ plan.subscriptionCount }} subscriptions</span>
                      <span class="revenue">{{ plan.revenue | currency:'USD':'symbol':'1.0-0' }}</span>
                    </div>
                  </div>
                  <div class="plan-growth" [class]="plan.growthRate >= 0 ? 'positive' : 'negative'">
                    {{ plan.growthRate >= 0 ? '+' : '' }}{{ plan.growthRate | number:'1.1-1' }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Revenue Trends -->
          <div class="chart-card">
            <div class="chart-header">
              <h3>Revenue Trends</h3>
              <button class="btn btn-sm btn-outline" (click)="viewRevenueDetails()">View Details</button>
            </div>
            <div class="chart-content" *ngIf="revenueAnalytics?.monthlyRevenueTrend">
              <div class="trend-chart">
                <div 
                  *ngFor="let trend of revenueAnalytics?.monthlyRevenueTrend || []" 
                  class="trend-item">
                  <div class="trend-month">{{ trend.month }}</div>
                  <div class="trend-bar">
                    <div 
                      class="trend-fill" 
                      [style.width.%]="getRevenuePercentage(trend.revenue)">
                    </div>
                  </div>
                  <div class="trend-value">{{ trend.revenue | currency:'USD':'symbol':'1.0-0' }}</div>
                  <div class="trend-growth" [class]="trend.growth >= 0 ? 'positive' : 'negative'">
                    {{ trend.growth >= 0 ? '+' : '' }}{{ trend.growth | number:'1.1-1' }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Status Distribution -->
          <div class="chart-card">
            <div class="chart-header">
              <h3>Subscription Status Distribution</h3>
            </div>
            <div class="chart-content" *ngIf="subscriptionAnalytics?.statusDistribution">
              <div class="status-distribution">
                <div 
                  *ngFor="let status of subscriptionAnalytics?.statusDistribution || []" 
                  class="status-item">
                  <div class="status-info">
                    <span class="status-name">{{ status.status }}</span>
                    <span class="status-count">{{ status.count }}</span>
                  </div>
                  <div class="status-bar">
                    <div 
                      class="status-fill" 
                      [style.width.%]="status.percentage">
                    </div>
                  </div>
                  <span class="status-percentage">{{ status.percentage | number:'1.1-1' }}%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- User Segments -->
          <div class="chart-card">
            <div class="chart-header">
              <h3>User Segments</h3>
            </div>
            <div class="chart-content" *ngIf="userAnalytics?.topUserSegments">
              <div class="user-segments">
                <div 
                  *ngFor="let segment of userAnalytics?.topUserSegments || []" 
                  class="segment-item">
                  <div class="segment-info">
                    <span class="segment-name">{{ segment.segment }}</span>
                    <span class="segment-count">{{ segment.count }} users</span>
                  </div>
                  <div class="segment-stats">
                    <span class="segment-percentage">{{ segment.percentage | number:'1.1-1' }}%</span>
                    <span class="segment-value">{{ segment.averageValue | currency:'USD':'symbol':'1.0-0' }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <div class="actions-grid">
          <div class="action-card">
            <div class="action-icon">
              <i class="fas fa-chart-line"></i>
            </div>
            <div class="action-content">
              <h4>Generate Report</h4>
              <p>Create comprehensive analytics reports</p>
              <button class="btn btn-primary" (click)="generateReport()">Generate</button>
            </div>
          </div>
          
          <div class="action-card">
            <div class="action-icon">
              <i class="fas fa-bell"></i>
            </div>
            <div class="action-content">
              <h4>Set Alerts</h4>
              <p>Configure performance alerts and notifications</p>
              <button class="btn btn-outline" (click)="configureAlerts()">Configure</button>
            </div>
          </div>
          
          <div class="action-card">
            <div class="action-icon">
              <i class="fas fa-download"></i>
            </div>
            <div class="action-content">
              <h4>Export Data</h4>
              <p>Export analytics data in various formats</p>
              <button class="btn btn-outline" (click)="exportData()">Export</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-state" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading analytics data...</p>
      </div>
    </div>
  `,
  styleUrls: ['./analytics-dashboard.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  // Data properties
  subscriptionAnalytics: SubscriptionAnalytics | null = null;
  revenueAnalytics: RevenueAnalytics | null = null;
  userAnalytics: UserAnalytics | null = null;
  
  // Form and filter properties
  filterForm!: FormGroup;
  
  // State properties
  loading = false;
  error: string | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private analyticsService: AdminAnalyticsService,
    private subscriptionService: AdminSubscriptionService,
    private snackBar: MatSnackBar
  ) {
    this.initializeFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscriptions();
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Form initialization
  private initializeFilterForm(): void {
    this.filterForm = this.fb.group({
      dateRange: ['30'],
      startDate: [''],
      endDate: ['']
    });
  }

  // Setup filter subscriptions
  private setupFilterSubscriptions(): void {
    this.filterForm.get('dateRange')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        if (value !== 'custom') {
          this.loadAnalytics();
        }
      });
  }

  // Load analytics data
  loadAnalytics(): void {
    this.loading = true;
    this.error = null;

    const days = parseInt(this.filterForm.get('dateRange')?.value || '30');
    const startDate = this.filterForm.get('startDate')?.value;
    const endDate = this.filterForm.get('endDate')?.value;

    // Load subscription analytics
    this.analyticsService.getSubscriptionAnalytics(
      startDate ? new Date(startDate) : undefined,
      endDate ? new Date(endDate) : undefined
    ).subscribe({
      next: (response: JsonModel<SubscriptionAnalytics>) => {
        if (response.statusCode === 200) {
          this.subscriptionAnalytics = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading subscription analytics:', error);
      }
    });

    // Load revenue analytics
    this.analyticsService.getRevenueAnalytics(
      startDate ? new Date(startDate) : undefined,
      endDate ? new Date(endDate) : undefined
    ).subscribe({
      next: (response: JsonModel<RevenueAnalytics>) => {
        if (response.statusCode === 200) {
          this.revenueAnalytics = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading revenue analytics:', error);
      }
    });

    // Load user analytics
    this.analyticsService.getUserAnalytics(
      startDate ? new Date(startDate) : undefined,
      endDate ? new Date(endDate) : undefined
    ).subscribe({
      next: (response: JsonModel<UserAnalytics>) => {
        if (response.statusCode === 200) {
          this.userAnalytics = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading user analytics:', error);
        this.loading = false;
      }
    });
  }

  // Apply filters
  applyFilters(): void {
    this.loadAnalytics();
  }

  // Refresh analytics
  refreshAnalytics(): void {
    this.loadAnalytics();
  }

  // Export analytics
  exportAnalytics(): void {
    // TODO: Implement export functionality
    console.log('Export analytics');
    this.showSuccessMessage('Analytics export started');
  }

  // Generate report
  generateReport(): void {
    // TODO: Implement report generation
    console.log('Generate report');
    this.showSuccessMessage('Report generation started');
  }

  // Configure alerts
  configureAlerts(): void {
    // TODO: Implement alert configuration
    console.log('Configure alerts');
    this.showSuccessMessage('Alert configuration opened');
  }

  // Export data
  exportData(): void {
    // TODO: Implement data export
    console.log('Export data');
    this.showSuccessMessage('Data export started');
  }

  // View all plans
  viewAllPlans(): void {
    // TODO: Navigate to subscription plans
    console.log('View all plans');
  }

  // View revenue details
  viewRevenueDetails(): void {
    // TODO: Navigate to revenue details
    console.log('View revenue details');
  }

  // Utility methods
  getRevenuePercentage(revenue: number): number {
    if (!this.revenueAnalytics?.monthlyRevenueTrend) return 0;
    const maxRevenue = Math.max(...this.revenueAnalytics.monthlyRevenueTrend.map(t => t.revenue));
    return maxRevenue > 0 ? (revenue / maxRevenue) * 100 : 0;
  }

  private showSuccessMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  private showErrorMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 7000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
}
