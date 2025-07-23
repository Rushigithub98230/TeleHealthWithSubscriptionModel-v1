import { Component, OnInit } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartType } from 'chart.js';
import { AdminApiService, AdminDashboardStats, RevenueTrendDto, UserActivityData, SubscriptionsByPlan } from '../../../../core/admin-api.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, BaseChartDirective],
  templateUrl: './analytics-dashboard.component.html',
  styleUrl: './analytics-dashboard.component.scss'
})
export class AnalyticsDashboardComponent implements OnInit {
  stats: AdminDashboardStats | null = null;
  loading = false;
  error: string | null = null;

  revenueChartData: ChartConfiguration<'line'>['data'] = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    datasets: [
      { data: [12000, 15000, 17000, 14000, 18000, 21000], label: 'Revenue', fill: true, tension: 0.4, borderColor: '#1976d2', backgroundColor: 'rgba(25,118,210,0.1)' }
    ]
  };
  revenueChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: {
      legend: { display: true, position: 'top' },
      title: { display: true, text: 'Revenue Trend' }
    }
  };

  userGrowthChartData: ChartConfiguration<'line'>['data'] = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    datasets: [
      { data: [100, 120, 150, 180, 210, 250], label: 'Users', fill: false, borderColor: '#43a047', backgroundColor: 'rgba(67,160,71,0.1)' }
    ]
  };
  userGrowthChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    plugins: {
      legend: { display: true, position: 'top' },
      title: { display: true, text: 'User Growth' }
    }
  };

  planDistributionChartData: ChartConfiguration<'pie'>['data'] = {
    labels: ['Basic', 'Standard', 'Premium'],
    datasets: [
      { data: [120, 80, 50], label: 'Plans', backgroundColor: ['#1976d2', '#43a047', '#fbc02d'] }
    ]
  };
  planDistributionChartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    plugins: {
      legend: { display: true, position: 'top' },
      title: { display: true, text: 'Plan Distribution' }
    }
  };

  constructor(private adminApi: AdminApiService) {}

  ngOnInit(): void {
    this.fetchStats();
    this.fetchRevenueTrend();
    this.fetchUserGrowth();
    this.fetchPlanDistribution();
  }

  fetchStats(): void {
    this.loading = true;
    this.error = null;
    this.adminApi.getAdminDashboardStats().subscribe({
      next: (stats: AdminDashboardStats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: (err: unknown) => {
        this.error = 'Failed to load analytics';
        this.loading = false;
      }
    });
  }

  fetchRevenueTrend(): void {
    this.adminApi.getBillingAnalytics().subscribe({
      next: (data: { revenueTrend: RevenueTrendDto[] }) => {
        const trend: RevenueTrendDto[] = data.revenueTrend || [];
        this.revenueChartData = {
          labels: trend.map(t => t.date),
          datasets: [
            { data: trend.map(t => t.revenue), label: 'Revenue', fill: true, tension: 0.4, borderColor: '#1976d2', backgroundColor: 'rgba(25,118,210,0.1)' }
          ]
        };
      }
    });
  }

  fetchUserGrowth(): void {
    this.adminApi.getUserAnalytics().subscribe({
      next: (data: { userActivityBreakdown: UserActivityData[] }) => {
        const breakdown: UserActivityData[] = data.userActivityBreakdown || [];
        this.userGrowthChartData = {
          labels: breakdown.map(u => u.date),
          datasets: [
            { data: breakdown.map(u => u.activeUsers), label: 'Users', fill: false, borderColor: '#43a047', backgroundColor: 'rgba(67,160,71,0.1)' }
          ]
        };
      }
    });
  }

  fetchPlanDistribution(): void {
    this.adminApi.getSubscriptionAnalytics().subscribe({
      next: (data: { subscriptionsByPlan: SubscriptionsByPlan }) => {
        const byPlan: SubscriptionsByPlan = data.subscriptionsByPlan || {};
        this.planDistributionChartData = {
          labels: Object.keys(byPlan),
          datasets: [
            { data: Object.values(byPlan), label: 'Plans', backgroundColor: ['#1976d2', '#43a047', '#fbc02d', '#e53935', '#8e24aa', '#00838f'] }
          ]
        };
      }
    });
  }

  exportReport(format: 'csv' | 'pdf'): void {
    // For demo: download subscription report for last 6 months
    const end = new Date();
    const start = new Date();
    start.setMonth(end.getMonth() - 5);
    this.adminApi.exportSubscriptionReport(start, end, format).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `subscription-report.${format}`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
} 