import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { TableColumn, TableAction } from '../../../shared/global-table/global-table.component';
import { AdminApiService, Notification } from '../../../core/admin-api.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-notifications-management',
  standalone: true,
  imports: [ReactiveFormsModule, MatPaginatorModule],
  templateUrl: './notifications-management.component.html',
  styleUrl: './notifications-management.component.scss'
})
export class NotificationsManagementComponent implements OnInit {
  notifications: Notification[] = [];
  loading = false;
  error: string | null = null;
  showDetailsModal = false;
  selectedNotification: Notification | null = null;
  filterForm!: FormGroup;
  notificationColumns: TableColumn[] = [
    { key: 'createdAt', label: 'Date' },
    { key: 'title', label: 'Title' },
    { key: 'message', label: 'Message' },
    { key: 'userId', label: 'User' }
  ];
  notificationActions: TableAction[] = [
    { label: 'Details', action: 'details' }
  ];
  selectedIds = new Set<string>();
  page = 1;
  pageSize = 25;
  totalNotifications = 0;

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.filterForm = new FormGroup({
      userId: new FormControl(''),
      title: new FormControl(''),
      date: new FormControl(''),
      search: new FormControl('')
    });
    this.fetchNotifications();
    this.filterForm.valueChanges.subscribe(() => this.fetchNotifications());
  }

  fetchNotifications() {
    this.loading = true;
    this.error = null;
    this.adminApi.getNotifications().subscribe({
      next: (notifications: Notification[]) => {
        this.notifications = notifications;
        this.totalNotifications = notifications.length;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load notifications';
        this.loading = false;
      }
    });
  }

  get filteredNotifications(): Notification[] {
    // Simulate client-side pagination for now
    const start = (this.page - 1) * this.pageSize;
    return this.notifications.slice(start, start + this.pageSize);
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: Notification }) {
    if (event.action === 'details') {
      this.selectedNotification = event.row;
      this.showDetailsModal = true;
    }
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedNotification = null;
  }
} 