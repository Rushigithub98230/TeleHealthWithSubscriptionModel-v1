import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AdminSubscriptionManagementService, AdminSubscriptionAnalytics, ApiResponse } from '../../../../../core/services/admin-subscription-management.service';

export interface AnalyticsData {
  totalSubscriptions: number;
  activeSubscriptions: number;
  expiredSubscriptions: number;
  monthlyRevenue: number;
  totalRevenue: number;
  churnRate: number;
  growthRate: number;
  topCategories: Array<{name: string, count: number, revenue: number}>;
  subscriptionTrends: Array<{month: string, count: number, revenue: number}>;
}

@Component({
  selector: 'app-subscription-analytics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './subscription-analytics.component.html',
  styleUrls: ['./subscription-analytics.component.scss']
})
export class SubscriptionAnalyticsComponent implements OnInit {
  analyticsData: AdminSubscriptionAnalytics | null = null;
  loading = false;
  error = '';
  selectedPeriod = 'month';
  startDate?: Date;
  endDate?: Date;

  periods = [
    { value: '7days', label: 'Last 7 Days' },
    { value: '30days', label: 'Last 30 Days' },
    { value: '90days', label: 'Last 90 Days' },
    { value: '1year', label: 'Last Year' }
  ];

  constructor(
    private analyticsService: AdminSubscriptionManagementService
  ) {}

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.loading = true;
    this.error = '';

    this.analyticsService.getAnalytics(
      this.selectedPeriod,
      this.startDate,
      this.endDate
    ).subscribe({
      next: (response: ApiResponse<AdminSubscriptionAnalytics>) => {
        if (response.success && response.data) {
          this.analyticsData = response.data;
        } else {
          this.error = response.message || 'Failed to load analytics';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Error loading analytics. Please try again.';
        this.loading = false;
        console.error('Error loading analytics:', error);
      }
    });
  }

  onPeriodChange(): void {
    this.loadAnalytics();
  }

  onDateRangeChange(): void {
    this.loadAnalytics();
  }

  getGrowthColor(growth: number): string {
    return growth >= 0 ? '#28a745' : '#dc3545';
  }

  getChurnColor(churn: number): string {
    return churn <= 5 ? '#28a745' : churn <= 10 ? '#ffc107' : '#dc3545';
  }

  clearError(): void {
    this.error = '';
  }

  // Helper methods for chart data
  getTopCategoriesChartData(): any[] {
    if (!this.analyticsData?.topCategories) return [];
    
    return this.analyticsData.topCategories.map(category => ({
      name: category.categoryName,
      value: category.subscriptionCount,
      revenue: category.revenue
    }));
  }

  getRevenueTrendData(): any[] {
    if (!this.analyticsData) return [];
    
    // This would come from the backend, but for now we'll create sample data
    return [
      { month: 'Jan', revenue: this.analyticsData.monthlyRevenue * 0.8 },
      { month: 'Feb', revenue: this.analyticsData.monthlyRevenue * 0.9 },
      { month: 'Mar', revenue: this.analyticsData.monthlyRevenue },
      { month: 'Apr', revenue: this.analyticsData.monthlyRevenue * 1.1 },
      { month: 'May', revenue: this.analyticsData.monthlyRevenue * 1.2 },
      { month: 'Jun', revenue: this.analyticsData.monthlyRevenue * 1.3 }
    ];
  }

  getSubscriptionStatusData(): any[] {
    if (!this.analyticsData) return [];
    
    return [
      { name: 'Active', value: this.analyticsData.activeSubscriptions, color: '#28a745' },
      { name: 'Paused', value: this.analyticsData.pausedSubscriptions, color: '#ffc107' },
      { name: 'Cancelled', value: this.analyticsData.cancelledSubscriptions, color: '#dc3545' }
    ];
  }
} 