import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AdminSubscriptionManagementService, AdminUserSubscription, ApiResponse } from '../../../../../core/services/admin-subscription-management.service';

export interface UserSubscription {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  planId: string;
  planName: string;
  category: string;
  status: 'active' | 'expired' | 'cancelled' | 'paused' | 'trial';
  startDate: Date;
  endDate: Date;
  nextBillingDate: Date;
  lastPaymentDate: Date;
  totalPaid: number;
  nextPaymentAmount: number;
  stripeSubscriptionId?: string;
  createdAt: Date;
  updatedAt: Date;
}

@Component({
  selector: 'app-user-subscriptions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './user-subscriptions.component.html',
  styleUrls: ['./user-subscriptions.component.scss']
})
export class UserSubscriptionsComponent implements OnInit {
  subscriptions: AdminUserSubscription[] = [];
  filteredSubscriptions: AdminUserSubscription[] = [];
  loading = false;
  error = '';
  successMessage = '';
  showDetailsModal = false;
  selectedSubscription: AdminUserSubscription | null = null;
  searchTerm = '';
  selectedStatus = '';
  selectedCategory = '';
  selectedPlan = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalItems = 0;
  totalPages = 0;

  statuses = ['active', 'expired', 'cancelled', 'paused', 'trial'];
  categories = ['Hair Loss', 'Skin Care', 'Mental Health', 'Weight Loss', 'General Health'];
  plans = ['Basic Hair Loss', 'Premium Hair Loss', 'Basic Skin Care', 'Premium Skin Care'];

  constructor(
    private subscriptionService: AdminSubscriptionManagementService
  ) {}

  ngOnInit(): void {
    this.loadSubscriptions();
  }

  loadSubscriptions(): void {
    this.loading = true;
    this.error = '';

    this.subscriptionService.getAllUserSubscriptions(
      this.searchTerm,
      this.selectedStatus,
      this.selectedCategory,
      this.selectedPlan,
      undefined, // startDate
      undefined, // endDate
      this.currentPage,
      this.pageSize
    ).subscribe({
      next: (response: ApiResponse<AdminUserSubscription[]>) => {
        if (response.success && response.data) {
          this.subscriptions = response.data;
          this.filteredSubscriptions = this.subscriptions;
          this.totalItems = response.pagination?.totalCount || 0;
          this.totalPages = response.pagination?.totalPages || 0;
        } else {
          this.error = response.message || 'Failed to load subscriptions';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Error loading subscriptions. Please try again.';
        this.loading = false;
        console.error('Error loading subscriptions:', error);
      }
    });
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadSubscriptions();
  }

  openDetailsModal(subscription: AdminUserSubscription): void {
    this.selectedSubscription = subscription;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.selectedSubscription = null;
    this.showDetailsModal = false;
  }

  cancelSubscription(subscription: AdminUserSubscription): void {
    if (confirm(`Are you sure you want to cancel subscription for ${subscription.userName}?`)) {
      this.loading = true;
      const reason = prompt('Please provide a reason for cancellation (optional):');
      
      this.subscriptionService.cancelUserSubscription(subscription.id, reason || undefined)
        .subscribe({
          next: (response: ApiResponse<AdminUserSubscription>) => {
            if (response.success) {
              this.successMessage = 'Subscription cancelled successfully';
              this.loadSubscriptions(); // Reload to get updated data
            } else {
              this.error = response.message || 'Failed to cancel subscription';
            }
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Error cancelling subscription. Please try again.';
            this.loading = false;
            console.error('Error cancelling subscription:', error);
          }
        });
    }
  }

  pauseSubscription(subscription: AdminUserSubscription): void {
    if (confirm(`Are you sure you want to pause subscription for ${subscription.userName}?`)) {
      this.loading = true;
      const reason = prompt('Please provide a reason for pausing (optional):');
      
      this.subscriptionService.pauseUserSubscription(subscription.id, reason || undefined)
        .subscribe({
          next: (response: ApiResponse<AdminUserSubscription>) => {
            if (response.success) {
              this.successMessage = 'Subscription paused successfully';
              this.loadSubscriptions(); // Reload to get updated data
            } else {
              this.error = response.message || 'Failed to pause subscription';
            }
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Error pausing subscription. Please try again.';
            this.loading = false;
            console.error('Error pausing subscription:', error);
          }
        });
    }
  }

  resumeSubscription(subscription: AdminUserSubscription): void {
    if (confirm(`Are you sure you want to resume subscription for ${subscription.userName}?`)) {
      this.loading = true;
      
      this.subscriptionService.resumeUserSubscription(subscription.id)
        .subscribe({
          next: (response: ApiResponse<AdminUserSubscription>) => {
            if (response.success) {
              this.successMessage = 'Subscription resumed successfully';
              this.loadSubscriptions(); // Reload to get updated data
            } else {
              this.error = response.message || 'Failed to resume subscription';
            }
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Error resuming subscription. Please try again.';
            this.loading = false;
            console.error('Error resuming subscription:', error);
          }
        });
    }
  }

  extendSubscription(subscription: AdminUserSubscription): void {
    const newEndDate = prompt('Enter new end date (YYYY-MM-DD):');
    if (newEndDate) {
      this.loading = true;
      const reason = prompt('Please provide a reason for extension (optional):');
      
      const extendRequest = {
        newEndDate: newEndDate,
        reason: reason || undefined
      };
      
      this.subscriptionService.extendUserSubscription(subscription.id, extendRequest)
        .subscribe({
          next: (response: ApiResponse<AdminUserSubscription>) => {
            if (response.success) {
              this.successMessage = 'Subscription extended successfully';
              this.loadSubscriptions(); // Reload to get updated data
            } else {
              this.error = response.message || 'Failed to extend subscription';
            }
            this.loading = false;
          },
          error: (error) => {
            this.error = 'Error extending subscription. Please try again.';
            this.loading = false;
            console.error('Error extending subscription:', error);
          }
        });
    }
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return '#28a745';
      case 'paused': return '#ffc107';
      case 'cancelled': return '#dc3545';
      case 'expired': return '#6c757d';
      case 'trial': return '#17a2b8';
      default: return '#6c757d';
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'badge-success';
      case 'paused': return 'badge-warning';
      case 'cancelled': return 'badge-danger';
      case 'expired': return 'badge-secondary';
      case 'trial': return 'badge-info';
      default: return 'badge-secondary';
    }
  }

  isSubscriptionExpired(subscription: AdminUserSubscription): boolean {
    return subscription.isExpired;
  }

  isSubscriptionExpiringSoon(subscription: AdminUserSubscription): boolean {
    return subscription.isNearExpiration;
  }

  clearError(): void {
    this.error = '';
  }

  clearSuccess(): void {
    this.successMessage = '';
  }
} 