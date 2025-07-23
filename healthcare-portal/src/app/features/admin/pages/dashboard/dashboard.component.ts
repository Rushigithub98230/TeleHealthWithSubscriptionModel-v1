import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { AdminApiService, AdminDashboardStats } from '../../../../core/admin-api.service';
import { AuthService } from '../../../../core/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ProfileComponent } from '../../../../shared/profile.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';

function decodeJwt(token: string): any {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, MatSnackBarModule, MatDialogModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  stats: AdminDashboardStats | null = null;
  loading = false;
  error: string | null = null;
  userName: string | null = null;
  userRole: string | null = null;
  avatarUrl: string | null = null;

  constructor(
    private adminApi: AdminApiService,
    private auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.fetchStats();
    this.setUserInfo();
  }

  setUserInfo() {
    const token = localStorage.getItem('token');
    if (token) {
      const decoded = decodeJwt(token);
      this.userName = decoded?.name || decoded?.email || 'Unknown';
      this.userRole = decoded?.role || decoded?.roles || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Admin';
      this.avatarUrl = decoded?.avatarUrl || null;
      if (Array.isArray(this.userRole)) {
        this.userRole = this.userRole.join(', ');
      }
    }
  }

  openProfileDialog() {
    this.dialog.open(ProfileComponent, { width: '420px' });
  }

  fetchStats() {
    this.loading = true;
    this.adminApi.getAdminDashboardStats().subscribe({
      next: (stats: AdminDashboardStats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load dashboard stats';
        this.snackBar.open('Failed to load dashboard stats', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  // Example pattern for admin CRUD actions:
  addUser() {
    // ...
    this.snackBar.open('User added successfully!', 'Close', { duration: 3000 });
  }
  editUser() {
    // ...
    this.snackBar.open('User updated successfully!', 'Close', { duration: 3000 });
  }
  deleteUser() {
    // ...
    this.snackBar.open('User deleted successfully!', 'Close', { duration: 3000 });
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/admin/login']);
      },
      error: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/admin/login']);
      }
    });
  }
}
