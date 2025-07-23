import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserManagementComponent } from './user-management.component';

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should filter users by role', () => {
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Active' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.filterForm.setValue({ search: '', role: 'Admin', status: '' });
    expect(component.filteredUsers.length).toBe(1);
    expect(component.filteredUsers[0].name).toBe('Alice');
  });

  it('should filter users by status', () => {
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Active' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.filterForm.setValue({ search: '', role: '', status: 'Inactive' });
    expect(component.filteredUsers.length).toBe(1);
    expect(component.filteredUsers[0].name).toBe('Bob');
  });

  it('should filter users by search term', () => {
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Active' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.filterForm.setValue({ search: 'bob', role: '', status: '' });
    expect(component.filteredUsers.length).toBe(1);
    expect(component.filteredUsers[0].name).toBe('Bob');
  });

  it('should call deleteUser for each selected user on bulk delete', () => {
    const spy = spyOn(component, 'deleteUser');
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Active' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.selectedIds = new Set(['1', '2']);
    component.bulkAction = 'delete';
    spyOn(window, 'confirm').and.returnValue(true);
    component.handleBulkAction();
    expect(spy).toHaveBeenCalledTimes(2);
  });

  it('should call editUser for each selected user on bulk activate', () => {
    const spy = spyOn(component, 'editUser');
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Inactive' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.selectedIds = new Set(['1', '2']);
    component.bulkAction = 'activate';
    component.handleBulkAction();
    expect(spy).toHaveBeenCalledTimes(2);
    expect(spy.calls.mostRecent().args[0].status).toBe('Active');
  });

  it('should call assignRole for each selected user on bulk assignRole', () => {
    const spy = spyOn(component, 'assignRole');
    component.users = [
      { id: '1', name: 'Alice', email: 'alice@example.com', role: 'Admin', status: 'Active' },
      { id: '2', name: 'Bob', email: 'bob@example.com', role: 'Provider', status: 'Inactive' }
    ];
    component.selectedIds = new Set(['1', '2']);
    component.bulkAction = 'assignRole';
    spyOn(window, 'prompt').and.returnValue('Provider');
    component.handleBulkAction();
    expect(spy).toHaveBeenCalledTimes(2);
    expect(spy.calls.mostRecent().args[0].role).toBe('Provider');
  });
});
