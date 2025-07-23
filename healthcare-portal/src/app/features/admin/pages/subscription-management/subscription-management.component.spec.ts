import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { SubscriptionManagementComponent } from './subscription-management.component';
import { AdminApiService, Subscription } from '../../../../core/admin-api.service';
import { of, throwError } from 'rxjs';

const mockSubscriptions: Subscription[] = [
  { id: '1', userId: 'user1', planId: 'plan1', status: 'Active', startDate: '2024-01-01' },
  { id: '2', userId: 'user2', planId: 'plan2', status: 'Paused', startDate: '2024-02-01' }
];

describe('SubscriptionManagementComponent', () => {
  let component: SubscriptionManagementComponent;
  let fixture: ComponentFixture<SubscriptionManagementComponent>;
  let adminApiServiceSpy: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('AdminApiService', ['getSubscriptions']);
    await TestBed.configureTestingModule({
      declarations: [SubscriptionManagementComponent],
      providers: [
        { provide: AdminApiService, useValue: spy }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(SubscriptionManagementComponent);
    component = fixture.componentInstance;
    adminApiServiceSpy = TestBed.inject(AdminApiService) as jasmine.SpyObj<AdminApiService>;
  });

  it('should fetch subscriptions on init', () => {
    adminApiServiceSpy.getSubscriptions.and.returnValue(of(mockSubscriptions));
    component.ngOnInit();
    expect(component.loading === false).toBe(true);
    expect(component.subscriptions.length === 2).toBe(true);
  });

  it('should handle fetch error', () => {
    adminApiServiceSpy.getSubscriptions.and.returnValue(throwError(() => new Error('API error')));
    component.ngOnInit();
    expect(component.loading === false).toBe(true);
    expect(component.error && component.error.indexOf('Failed') !== -1).toBe(true);
  });

  it('should filter subscriptions by searchTerm', () => {
    component.subscriptions = mockSubscriptions;
    component.searchTerm = 'user1';
    expect(component.filteredSubscriptions.length === 1).toBe(true);
    expect(component.filteredSubscriptions[0].userId === 'user1').toBe(true);
  });

  it('should show and hide toast', fakeAsync(() => {
    component.showToastMessage('Test message', 'success');
    expect(component.showToast).toBe(true);
    tick(3000);
    expect(component.showToast).toBe(false);
  }));
});

describe('Bulk actions', () => {
  let component: SubscriptionManagementComponent;
  let adminApiServiceSpy: jasmine.SpyObj<AdminApiService>;

  beforeEach(() => {
    adminApiServiceSpy = jasmine.createSpyObj('AdminApiService', ['getSubscriptions', 'cancelSubscription']);
    component = new SubscriptionManagementComponent(adminApiServiceSpy);
    component.subscriptions = [
      { id: '1', userId: 'u1', planId: 'p1', status: 'Active', startDate: '2024-01-01' },
      { id: '2', userId: 'u2', planId: 'p2', status: 'Active', startDate: '2024-01-02' }
    ];
  });

  it('should toggle selection', () => {
    component.toggleSelection('1');
    expect(component.selectedIds.has('1')).toBeTruthy();
    component.toggleSelection('1');
    expect(component.selectedIds.has('1')).toBeFalsy();
  });

  it('should select all and clear selection', () => {
    component.filteredSubscriptions = component.subscriptions;
    component.selectAll();
    expect(component.selectedIds.size === 2).toBeTruthy();
    component.clearSelection();
    expect(component.selectedIds.size === 0).toBeTruthy();
  });

  it('should return true for isAllSelected when all are selected', () => {
    component.filteredSubscriptions = component.subscriptions;
    component.selectAll();
    expect(component.isAllSelected()).toBeTruthy();
  });

  it('should call cancelSubscription for each selected ID in bulkCancel', async () => {
    spyOn(window, 'confirm').and.returnValue(true);
    adminApiServiceSpy.cancelSubscription.and.returnValue({ toPromise: () => Promise.resolve() } as any);
    component.filteredSubscriptions = component.subscriptions;
    component.selectAll();
    await component.bulkCancel();
    expect(adminApiServiceSpy.cancelSubscription.calls.count() === 2).toBeTruthy();
    expect(component.selectedIds.size === 0).toBeTruthy();
  });
});
