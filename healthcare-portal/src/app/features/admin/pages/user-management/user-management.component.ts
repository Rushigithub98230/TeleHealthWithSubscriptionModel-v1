import { Component, OnInit } from '@angular/core';
import { AdminApiService, User } from '../../../../core/admin-api.service';
import { Observable } from 'rxjs';
import { TableColumn, TableAction } from '../../../../shared/global-table/global-table.component';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  loading = false;
  error: string | null = null;
  filterForm!: FormGroup;
  selectedUser: User | null = null;
  showDetailsModal = false;
  toastMessage: string = '';
  showToast = false;
  toastType: 'success' | 'error' = 'success';
  bulkAction: string = '';

  userColumns: TableColumn[] = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name' },
    { key: 'email', label: 'Email' },
    { key: 'role', label: 'Role' },
    { key: 'status', label: 'Status' }
  ];
  userActions: TableAction[] = [
    { label: 'Details', action: 'details' },
    { label: 'Edit', action: 'edit' },
    { label: 'Assign Role', action: 'assignRole' },
    { label: 'Delete', action: 'delete', color: 'warn' },
    { label: 'Impersonate', action: 'impersonate' }
  ];
  selectedIds = new Set<string>();

  get filteredUsers(): User[] {
    const { search, role, status } = this.filterForm?.value || {};
    let users = this.users;
    if (role) users = users.filter(u => u.role === role);
    if (status) users = users.filter(u => u.status === status);
    if (search && search.trim()) {
      const term = search.trim().toLowerCase();
      users = users.filter(user =>
        user.name.toLowerCase().includes(term) ||
        user.email.toLowerCase().includes(term) ||
        user.role.toLowerCase().includes(term) ||
        user.status.toLowerCase().includes(term)
      );
    }
    return users;
  }

  showDetails(user: User) {
    this.selectedUser = user;
    this.showDetailsModal = true;
  }
  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedUser = null;
  }

  showToastMessage(message: string, type: 'success' | 'error' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3000);
  }

  impersonateUser(user: User) {
    if (!window.confirm(`Impersonate user ${user.name}?`)) return;
    // TODO: Call backend to get impersonation token
    // Example: this.adminApi.impersonateUser(user.id).subscribe(token => { ... })
    // For now, simulate:
    localStorage.setItem('impersonationToken', 'FAKE_TOKEN_FOR_' + user.id);
    localStorage.setItem('impersonatedUser', JSON.stringify(user));
    window.location.reload();
  }

  isImpersonating(): boolean {
    return !!localStorage.getItem('impersonationToken');
  }

  returnToAdmin() {
    localStorage.removeItem('impersonationToken');
    localStorage.removeItem('impersonatedUser');
    window.location.reload();
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: User }) {
    switch (event.action) {
      case 'details': this.showDetails(event.row); break;
      case 'edit': this.editUser(event.row); break;
      case 'assignRole': this.assignRole(event.row); break;
      case 'delete': this.deleteUser(event.row); break;
      case 'impersonate': this.impersonateUser(event.row); break;
    }
  }

  handleBulkAction() {
    if (!this.bulkAction || this.selectedIds.size === 0) return;
    const selectedUsers = this.users.filter(u => this.selectedIds.has(u.id));
    switch (this.bulkAction) {
      case 'delete':
        if (!window.confirm(`Delete ${selectedUsers.length} users?`)) return;
        selectedUsers.forEach(user => this.deleteUser(user));
        break;
      case 'activate':
        selectedUsers.forEach(user => this.editUser({ ...user, status: 'Active' }));
        break;
      case 'deactivate':
        selectedUsers.forEach(user => this.editUser({ ...user, status: 'Inactive' }));
        break;
      case 'assignRole':
        const role = window.prompt('Assign new role (Admin/Provider/Patient):');
        if (!role) return;
        selectedUsers.forEach(user => this.assignRole({ ...user, role }));
        break;
    }
    this.bulkAction = '';
    this.selectedIds.clear();
  }

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.filterForm = new FormGroup({
      search: new FormControl(''),
      role: new FormControl(''),
      status: new FormControl('')
    });
    this.fetchUsers();
  }

  fetchUsers() {
    this.loading = true;
    this.adminApi.getUsers().subscribe({
      next: (users: User[]) => {
        this.users = users;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load users';
        this.loading = false;
      }
    });
  }

  addUser() {
    const name = window.prompt('Enter user name:');
    if (!name) return;
    const email = window.prompt('Enter user email:');
    if (!email) return;
    const role = window.prompt('Enter user role (Admin/Provider/Patient):', 'Admin');
    if (!role) return;
    const status = window.prompt('Enter user status (Active/Inactive):', 'Active');
    if (!status) return;
    this.loading = true;
    this.adminApi.addUser({ name, email, role, status }).subscribe({
      next: (user: User) => {
        this.users.push(user);
        this.loading = false;
        this.showToastMessage('User added successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to add user';
        this.loading = false;
        this.showToastMessage('Failed to add user.', 'error');
      }
    });
  }

  editUser(user: User) {
    const name = window.prompt('Edit user name:', user.name);
    if (!name) return;
    const email = window.prompt('Edit user email:', user.email);
    if (!email) return;
    const role = window.prompt('Edit user role (Admin/Provider/Patient):', user.role);
    if (!role) return;
    const status = window.prompt('Edit user status (Active/Inactive):', user.status);
    if (!status) return;
    this.loading = true;
    this.adminApi.updateUser(user.id, { name, email, role, status }).subscribe({
      next: (updated: User) => {
        this.users = this.users.map(u => u.id === user.id ? updated : u);
        this.loading = false;
        this.showToastMessage('User updated successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to update user';
        this.loading = false;
        this.showToastMessage('Failed to update user.', 'error');
      }
    });
  }

  assignRole(user: User) {
    const role = window.prompt('Assign new role (Admin/Provider/Patient):', user.role);
    if (!role) return;
    this.loading = true;
    this.adminApi.assignRole(user.id, role).subscribe({
      next: (updated: User) => {
        this.users = this.users.map(u => u.id === user.id ? updated : u);
        this.loading = false;
        this.showToastMessage('Role assigned successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to assign role';
        this.loading = false;
        this.showToastMessage('Failed to assign role.', 'error');
      }
    });
  }

  deleteUser(user: User) {
    if (!window.confirm(`Are you sure you want to delete user ${user.name}?`)) {
      return;
    }
    this.loading = true;
    this.adminApi.deleteUser(user.id).subscribe({
      next: () => {
        this.users = this.users.filter(u => u.id !== user.id);
        this.loading = false;
        this.showToastMessage('User deleted successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to delete user';
        this.loading = false;
        this.showToastMessage('Failed to delete user.', 'error');
      }
    });
  }
}
