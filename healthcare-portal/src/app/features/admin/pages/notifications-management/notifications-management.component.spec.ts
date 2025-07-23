import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NotificationsManagementComponent } from './notifications-management.component';
import { AdminApiService } from '../../../core/admin-api.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { MatPaginatorModule } from '@angular/material/paginator';

const mockNotifications = [
  { id: '1', title: 'Payment Failed', message: 'Payment failed for user u1', userId: 'u1', createdAt: '2024-07-20T10:00:00Z' },
  { id: '2', title: 'User Registered', message: 'New user registered', userId: 'u2', createdAt: '2024-07-20T11:00:00Z' }
];

describe('NotificationsManagementComponent', () => {
  let component: NotificationsManagementComponent;
  let fixture: ComponentFixture<NotificationsManagementComponent>;
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('AdminApiService', ['getNotifications']);
    await TestBed.configureTestingModule({
      imports: [NotificationsManagementComponent, ReactiveFormsModule, MatPaginatorModule],
      providers: [{ provide: AdminApiService, useValue: apiSpy }]
    }).compileComponents();
    fixture = TestBed.createComponent(NotificationsManagementComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(AdminApiService) as jasmine.SpyObj<AdminApiService>;
  });

  it('should fetch notifications on init', () => {
    api.getNotifications.and.returnValue(of(mockNotifications));
    fixture.detectChanges();
    expect(api.getNotifications).toHaveBeenCalled();
    expect(component.notifications.length).toBe(2);
    expect(component.totalNotifications).toBe(2);
  });

  it('should fetch notifications on filter change', fakeAsync(() => {
    api.getNotifications.and.returnValue(of(mockNotifications));
    fixture.detectChanges();
    component.filterForm.get('userId')!.setValue('u1');
    tick(300);
    expect(api.getNotifications).toHaveBeenCalledTimes(2);
  }));

  it('should update page and paginate notifications', () => {
    api.getNotifications.and.returnValue(of(mockNotifications));
    fixture.detectChanges();
    component.onPageChange({ pageIndex: 1, pageSize: 1, length: 2 } as any);
    expect(component.page).toBe(2);
    expect(component.pageSize).toBe(1);
    expect(component.filteredNotifications.length).toBe(1);
  });

  it('should open details modal on details action', () => {
    component.onTableAction({ action: 'details', row: mockNotifications[0] });
    expect(component.showDetailsModal).toBeTrue();
    expect(component.selectedNotification).toEqual(mockNotifications[0]);
  });

  it('should handle API error', () => {
    api.getNotifications.and.returnValue(throwError(() => new Error('API error')));
    fixture.detectChanges();
    expect(component.error).toContain('Failed to load notifications');
  });
}); 