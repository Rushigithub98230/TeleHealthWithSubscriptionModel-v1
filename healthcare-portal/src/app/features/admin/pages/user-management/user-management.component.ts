import { Component, OnInit } from '@angular/core';
import { AdminApiService, User } from '../../../../core/admin-api.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  loading = false;
  error: string | null = null;

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
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
      },
      error: (err: any) => {
        this.error = 'Failed to add user';
        this.loading = false;
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
      },
      error: (err: any) => {
        this.error = 'Failed to update user';
        this.loading = false;
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
      },
      error: (err: any) => {
        this.error = 'Failed to assign role';
        this.loading = false;
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
      },
      error: (err: any) => {
        this.error = 'Failed to delete user';
        this.loading = false;
      }
    });
  }
}
