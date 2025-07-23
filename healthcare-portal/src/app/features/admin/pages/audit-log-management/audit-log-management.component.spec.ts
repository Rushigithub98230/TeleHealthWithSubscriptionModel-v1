import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AuditLogManagementComponent } from './audit-log-management.component';
import { AdminApiService } from '../../../core/admin-api.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { MatPaginatorModule } from '@angular/material/paginator';

const mockLogs = [
  { id: '1', userId: 'u1', action: 'Login', entityType: 'User', entityId: 'u1', description: 'User login', createdAt: '2024-07-20T10:00:00Z' },
  { id: '2', userId: 'u2', action: 'Update', entityType: 'Plan', entityId: 'p1', description: 'Plan updated', createdAt: '2024-07-20T11:00:00Z' }
];

describe('AuditLogManagementComponent', () => {
  let component: AuditLogManagementComponent;
  let fixture: ComponentFixture<AuditLogManagementComponent>;
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('AdminApiService', ['getAuditLogs']);
    await TestBed.configureTestingModule({
      imports: [AuditLogManagementComponent, ReactiveFormsModule, MatPaginatorModule],
      providers: [{ provide: AdminApiService, useValue: apiSpy }]
    }).compileComponents();
    fixture = TestBed.createComponent(AuditLogManagementComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(AdminApiService) as jasmine.SpyObj<AdminApiService>;
  });

  it('should fetch audit logs on init', () => {
    api.getAuditLogs.and.returnValue(of({ items: mockLogs, totalCount: 2 }));
    fixture.detectChanges();
    expect(api.getAuditLogs).toHaveBeenCalled();
    expect(component.auditLogs.length).toBe(2);
    expect(component.totalLogs).toBe(2);
  });

  it('should fetch audit logs on filter change', fakeAsync(() => {
    api.getAuditLogs.and.returnValue(of({ items: mockLogs, totalCount: 2 }));
    fixture.detectChanges();
    component.filterForm.get('userId')!.setValue('u1');
    tick(300);
    expect(api.getAuditLogs).toHaveBeenCalledTimes(2);
  }));

  it('should update page and fetch logs on paginator event', () => {
    api.getAuditLogs.and.returnValue(of({ items: mockLogs, totalCount: 2 }));
    fixture.detectChanges();
    component.onPageChange({ pageIndex: 1, pageSize: 25, length: 2 } as any);
    expect(component.page).toBe(2);
    expect(component.pageSize).toBe(25);
    expect(api.getAuditLogs).toHaveBeenCalledTimes(2);
  });

  it('should open details modal on details action', () => {
    component.onTableAction({ action: 'details', row: mockLogs[0] });
    expect(component.showDetailsModal).toBeTrue();
    expect(component.selectedLog).toEqual(mockLogs[0]);
  });

  it('should handle API error', () => {
    api.getAuditLogs.and.returnValue(throwError(() => new Error('API error')));
    fixture.detectChanges();
    expect(component.error).toContain('Failed to load audit logs');
  });
}); 