import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AnalyticsDashboardComponent } from './analytics-dashboard.component';
import { AdminApiService } from '../../../core/admin-api.service';
import { of } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { NgChartsModule } from 'ng2-charts';

describe('AnalyticsDashboardComponent', () => {
  let component: AnalyticsDashboardComponent;
  let fixture: ComponentFixture<AnalyticsDashboardComponent>;
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('AdminApiService', [
      'getAdminDashboardStats',
      'getBillingAnalytics',
      'getUserAnalytics',
      'getSubscriptionAnalytics',
      'exportSubscriptionReport'
    ]);
    await TestBed.configureTestingModule({
      imports: [AnalyticsDashboardComponent, MatCardModule, MatButtonModule, NgChartsModule],
      providers: [{ provide: AdminApiService, useValue: apiSpy }]
    }).compileComponents();
    fixture = TestBed.createComponent(AnalyticsDashboardComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(AdminApiService) as jasmine.SpyObj<AdminApiService>;
  });

  it('should fetch stats on init', () => {
    api.getAdminDashboardStats.and.returnValue(of({ totalSubscriptions: 10, activeSubscriptions: 8, totalRevenue: 10000, monthlyRecurringRevenue: 2000, totalUsers: 50, totalProviders: 5 }));
    api.getBillingAnalytics.and.returnValue(of({ revenueTrend: [] }));
    api.getUserAnalytics.and.returnValue(of({ userActivityBreakdown: [] }));
    api.getSubscriptionAnalytics.and.returnValue(of({ subscriptionsByPlan: { Basic: 10, Premium: 5 } }));
    fixture.detectChanges();
    expect(api.getAdminDashboardStats).toHaveBeenCalled();
    expect(component.stats?.totalSubscriptions).toBe(10);
  });

  it('should update revenue chart with backend data', () => {
    api.getAdminDashboardStats.and.returnValue(of({}));
    api.getBillingAnalytics.and.returnValue(of({ revenueTrend: [ { date: '2024-01', revenue: 1000, growth: 0 } ] }));
    api.getUserAnalytics.and.returnValue(of({ userActivityBreakdown: [] }));
    api.getSubscriptionAnalytics.and.returnValue(of({ subscriptionsByPlan: { Basic: 10, Premium: 5 } }));
    fixture.detectChanges();
    expect(component.revenueChartData.labels).toContain('2024-01');
    expect(component.revenueChartData.datasets[0].data[0]).toBe(1000);
  });

  it('should update user growth chart with backend data', () => {
    api.getAdminDashboardStats.and.returnValue(of({}));
    api.getBillingAnalytics.and.returnValue(of({ revenueTrend: [] }));
    api.getUserAnalytics.and.returnValue(of({ userActivityBreakdown: [ { date: '2024-01', activeUsers: 10, consultations: 0, messages: 0 } ] }));
    api.getSubscriptionAnalytics.and.returnValue(of({ subscriptionsByPlan: { Basic: 10, Premium: 5 } }));
    fixture.detectChanges();
    expect(component.userGrowthChartData.labels).toContain('2024-01');
    expect(component.userGrowthChartData.datasets[0].data[0]).toBe(10);
  });

  it('should update plan distribution chart with backend data', () => {
    api.getAdminDashboardStats.and.returnValue(of({}));
    api.getBillingAnalytics.and.returnValue(of({ revenueTrend: [] }));
    api.getUserAnalytics.and.returnValue(of({ userActivityBreakdown: [] }));
    api.getSubscriptionAnalytics.and.returnValue(of({ subscriptionsByPlan: { Basic: 10, Premium: 5 } }));
    fixture.detectChanges();
    expect(component.planDistributionChartData.labels).toContain('Basic');
    expect(component.planDistributionChartData.datasets[0].data[0]).toBe(10);
  });

  it('should call exportSubscriptionReport on exportReport', () => {
    api.exportSubscriptionReport.and.returnValue(of(new Blob(["test"], { type: 'text/csv' })));
    spyOn(window.URL, 'createObjectURL').and.returnValue('blob:url');
    spyOn(document, 'createElement').and.callThrough();
    fixture.detectChanges();
    component.exportReport('csv');
    expect(api.exportSubscriptionReport).toHaveBeenCalled();
  });
}); 