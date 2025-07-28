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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AdminSubscriptionManagementService, AdminUserSubscription, BulkActionRequest, BulkActionResult, ApiResponse } from '../../../../../core/services/admin-subscription-management.service';

export interface BulkAction {
  id: string;
  name: string;
  description: string;
  action: 'cancel' | 'pause' | 'resume' | 'extend' | 'change_plan';
  selectedSubscriptions: string[];
  parameters: any;
  status: 'pending' | 'running' | 'completed' | 'failed';
  createdAt: Date;
  completedAt?: Date;
  results?: any;
}

@Component({
  selector: 'app-subscription-bulk-actions',
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
    MatCheckboxModule,
    MatSlideToggleModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './subscription-bulk-actions.component.html',
  styleUrls: ['./subscription-bulk-actions.component.scss']
})
export class SubscriptionBulkActionsComponent implements OnInit {
  subscriptions: AdminUserSubscription[] = [];
  selectedSubscriptions: string[] = [];
  loading = false;
  error = '';
  successMessage = '';
  showBulkActionModal = false;
  showResultsModal = false;
  currentAction: 'cancel' | 'pause' | 'resume' | 'extend' | 'change_plan' = 'cancel';
  actionReason = '';
  actionParameters: any = {};
  bulkActionResult: BulkActionResult | null = null;
  searchTerm = '';
  selectedStatus = '';
  selectedCategory = '';

  statuses = ['active', 'paused', 'cancelled', 'expired', 'trial'];
  categories = ['Hair Loss', 'Skin Care', 'Mental Health', 'Weight Loss', 'General Health'];
  actions = [
    { value: 'cancel', label: 'Cancel Subscriptions', icon: 'fas fa-times' },
    { value: 'pause', label: 'Pause Subscriptions', icon: 'fas fa-pause' },
    { value: 'resume', label: 'Resume Subscriptions', icon: 'fas fa-play' },
    { value: 'extend', label: 'Extend Subscriptions', icon: 'fas fa-calendar-plus' },
    { value: 'change_plan', label: 'Change Plan', icon: 'fas fa-exchange-alt' }
  ];

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
      undefined, // plan
      undefined, // startDate
      undefined, // endDate
      1, // page
      100 // pageSize - load more for bulk operations
    ).subscribe({
      next: (response: ApiResponse<AdminUserSubscription[]>) => {
        if (response.success && response.data) {
          this.subscriptions = response.data;
        } else {
          this.error = response.message || 'Failed to load subscriptions';
        }
        this.loading = false;
      },
      error: (error: any) => {
        this.error = 'Error loading subscriptions. Please try again.';
        this.loading = false;
        console.error('Error loading subscriptions:', error);
      }
    });
  }

  onSearchChange(): void {
    this.loadSubscriptions();
  }

  onFilterChange(): void {
    this.loadSubscriptions();
  }

  toggleSubscriptionSelection(subscriptionId: string): void {
    const index = this.selectedSubscriptions.indexOf(subscriptionId);
    if (index > -1) {
      this.selectedSubscriptions.splice(index, 1);
    } else {
      this.selectedSubscriptions.push(subscriptionId);
    }
  }

  selectAllSubscriptions(): void {
    this.selectedSubscriptions = this.subscriptions.map(s => s.id);
  }

  deselectAllSubscriptions(): void {
    this.selectedSubscriptions = [];
  }

  openBulkActionModal(action: string): void {
    if (this.selectedSubscriptions.length === 0) {
      this.error = 'Please select at least one subscription';
      return;
    }

    this.currentAction = action as 'cancel' | 'pause' | 'resume' | 'extend' | 'change_plan';
    this.actionReason = '';
    this.actionParameters = {};
    this.showBulkActionModal = true;
  }

  closeBulkActionModal(): void {
    this.showBulkActionModal = false;
    this.actionReason = '';
    this.actionParameters = {};
  }

  performBulkAction(): void {
    if (!this.actionReason.trim()) {
      this.error = 'Please provide a reason for this action';
      return;
    }

    this.loading = true;
    this.error = '';

    const request: BulkActionRequest = {
      subscriptionIds: this.selectedSubscriptions,
      action: this.currentAction as 'cancel' | 'pause' | 'resume',
      reason: this.actionReason
    };

    this.subscriptionService.performBulkAction(request)
      .subscribe({
        next: (response: ApiResponse<BulkActionResult>) => {
          if (response.success && response.data) {
            this.bulkActionResult = response.data;
            this.successMessage = `Bulk action completed. ${response.data.successCount} successful, ${response.data.failureCount} failed.`;
            this.closeBulkActionModal();
            this.showResultsModal = true;
            this.loadSubscriptions(); // Reload to get updated data
          } else {
            this.error = response.message || 'Failed to perform bulk action';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'Error performing bulk action. Please try again.';
          this.loading = false;
          console.error('Error performing bulk action:', error);
        }
      });
  }

  closeResultsModal(): void {
    this.showResultsModal = false;
    this.bulkActionResult = null;
  }

  getSelectedCount(): number {
    return this.selectedSubscriptions.length;
  }

  getTotalCount(): number {
    return this.subscriptions.length;
  }

  isSubscriptionSelected(subscriptionId: string): boolean {
    return this.selectedSubscriptions.includes(subscriptionId);
  }

  getActionIcon(action: string): string {
    const actionConfig = this.actions.find(a => a.value === action);
    return actionConfig?.icon || 'fas fa-cog';
  }

  getActionLabel(action: string): string {
    const actionConfig = this.actions.find(a => a.value === action);
    return actionConfig?.label || action;
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

  getBulkActionStatusClass(status: string): string {
    switch (status) {
      case 'pending': return 'text-warning';
      case 'running': return 'text-info';
      case 'completed': return 'text-success';
      case 'failed': return 'text-danger';
      default: return 'text-secondary';
    }
  }

  clearError(): void {
    this.error = '';
  }

  clearSuccess(): void {
    this.successMessage = '';
  }

  // Helper methods for different action types
  isExtendAction(): boolean {
    return this.currentAction === 'extend';
  }

  isChangePlanAction(): boolean {
    return this.currentAction === 'change_plan';
  }

  getActionDescription(): string {
    switch (this.currentAction) {
      case 'cancel':
        return 'This will cancel the selected subscriptions immediately. Users will lose access to services.';
      case 'pause':
        return 'This will pause the selected subscriptions. Users will retain access but billing will be suspended.';
      case 'resume':
        return 'This will resume the selected paused subscriptions. Billing will resume.';
      case 'extend':
        return 'This will extend the selected subscriptions by the specified number of days.';
      case 'change_plan':
        return 'This will change the plan for selected subscriptions. Users will be moved to the new plan.';
      default:
        return '';
    }
  }
} 