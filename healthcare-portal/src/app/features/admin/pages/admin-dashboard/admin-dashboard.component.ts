import { Component, OnInit } from '@angular/core';
import { AdminApiService, AdminDashboardStats, Notification, AuditLog } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  stats: AdminDashboardStats | null = null;
  loading = false;
  error: string | null = null;
  notifications: Notification[] = [];
  auditLogs: AuditLog[] = [];
  loadingNotifications = false;
  loadingAuditLogs = false;
  errorNotifications: string | null = null;
  errorAuditLogs: string | null = null;

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.fetchStats();
    this.fetchNotifications();
    this.fetchAuditLogs();
  }

  fetchStats() {
    this.loading = true;
    this.adminApi.getAdminDashboardStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load dashboard stats';
        this.loading = false;
      }
    });
  }
  fetchNotifications() {
    this.loadingNotifications = true;
    this.adminApi.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.loadingNotifications = false;
      },
      error: () => {
        this.errorNotifications = 'Failed to load notifications';
        this.loadingNotifications = false;
      }
    });
  }
  fetchAuditLogs() {
    this.loadingAuditLogs = true;
    this.adminApi.getAuditLogs().subscribe({
      next: (logs) => {
        this.auditLogs = logs;
        this.loadingAuditLogs = false;
      },
      error: () => {
        this.errorAuditLogs = 'Failed to load audit logs';
        this.loadingAuditLogs = false;
      }
    });
  }
}
