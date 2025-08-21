import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import { AdminSubscriptionService, SubscriptionListResponse } from '../../../services/admin-subscription.service';
import { AdminBillingService } from '../../../services/admin-billing.service';
import { AdminNotificationService } from '../../../services/admin-notification.service';
import { JsonModel } from '../../../../core/models/json-model.interface';
import { 
  Subscription, 
  SubscriptionStatus, 
  SubscriptionListParams, 
  BulkActionRequest,
  CreateSubscriptionDto,
  UpdateSubscriptionDto,
  SubscriptionPlan,
  User
} from '../../../models/subscription.interface';



@Component({
  selector: 'app-subscription-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  template: `
    <div class="subscription-list-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Subscription Management</h1>
          <p>Manage all user subscriptions, plans, and lifecycle operations</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="openCreateModal()"
            [disabled]="loading">
            <i class="fas fa-plus"></i> Create Subscription
          </button>
          <button 
            class="btn btn-secondary" 
            (click)="exportSubscriptions()"
            [disabled]="loading || subscriptions.length === 0">
            <i class="fas fa-download"></i> Export
          </button>
        </div>
      </div>

      <!-- Filters and Search -->
      <div class="filters-section">
        <form [formGroup]="filterForm" class="filter-form">
          <div class="filter-row">
            <div class="filter-group">
              <label for="search">Search</label>
              <input 
                type="text" 
                id="search" 
                formControlName="search"
                placeholder="Search by user, plan, or ID..."
                class="form-control">
            </div>
            <div class="filter-group">
              <label for="status">Status</label>
              <select id="status" formControlName="status" class="form-control">
                <option value="">All Statuses</option>
                <option *ngFor="let status of subscriptionStatuses" [value]="status">
                  {{ status }}
                </option>
              </select>
            </div>
            <div class="filter-group">
              <label for="plan">Plan</label>
              <select id="plan" formControlName="planId" class="form-control">
                <option value="">All Plans</option>
                <option *ngFor="let plan of subscriptionPlans" [value]="plan.id">
                  {{ plan.name }}
                </option>
              </select>
            </div>
            <div class="filter-group">
              <label for="dateRange">Date Range</label>
              <select id="dateRange" formControlName="dateRange" class="form-control">
                <option value="">All Time</option>
                <option value="today">Today</option>
                <option value="week">This Week</option>
                <option value="month">This Month</option>
                <option value="quarter">This Quarter</option>
                <option value="year">This Year</option>
              </select>
            </div>
            <div class="filter-group">
              <button type="button" class="btn btn-outline" (click)="clearFilters()">
                Clear Filters
              </button>
            </div>
          </div>
        </form>
      </div>

      <!-- Bulk Actions -->
      <div class="bulk-actions" *ngIf="selectedSubscriptions.length > 0">
        <div class="bulk-info">
          <span>{{ selectedSubscriptions.length }} subscription(s) selected</span>
        </div>
        <div class="bulk-buttons">
          <button 
            class="btn btn-success" 
            (click)="bulkActivate()"
            [disabled]="loading">
            Activate
          </button>
          <button 
            class="btn btn-warning" 
            (click)="bulkPause()"
            [disabled]="loading">
            Pause
          </button>
          <button 
            class="btn btn-danger" 
            (click)="bulkCancel()"
            [disabled]="loading">
            Cancel
          </button>
          <button 
            class="btn btn-outline" 
            (click)="clearSelection()">
            Clear Selection
          </button>
        </div>
      </div>

      <!-- Subscriptions Table -->
      <div class="table-container">
        <div class="table-header">
          <div class="table-info">
            <span>Showing {{ paginationInfo.startIndex + 1 }}-{{ paginationInfo.endIndex }} of {{ paginationInfo.totalRecords }} subscriptions</span>
          </div>
          <div class="table-actions">
            <select 
              [(ngModel)]="pageSize" 
              (change)="onPageSizeChange()"
              class="form-control page-size-select">
              <option value="10">10 per page</option>
              <option value="25">25 per page</option>
              <option value="50">50 per page</option>
              <option value="100">100 per page</option>
            </select>
          </div>
        </div>

        <table class="subscriptions-table">
          <thead>
            <tr>
              <th>
                <input 
                  type="checkbox" 
                  [checked]="isAllSelected()"
                  (change)="toggleSelectAll($event)">
              </th>
              <th>User</th>
              <th>Plan</th>
              <th>Status</th>
              <th>Start Date</th>
              <th>Next Billing</th>
              <th>Price</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let subscription of subscriptions" 
                [class.selected]="isSelected(subscription.id)"
                [class.expired]="subscription.status === 'Expired'">
              <td>
                <input 
                  type="checkbox" 
                  [checked]="isSelected(subscription.id)"
                  (change)="toggleSelection(subscription.id)">
              </td>
              <td>
                <div class="user-info">
                  <div class="user-name">{{ subscription.user?.firstName }} {{ subscription.user?.lastName }}</div>
                  <div class="user-email">{{ subscription.user?.email || 'N/A' }}</div>
                </div>
              </td>
              <td>
                <div class="plan-info">
                  <div class="plan-name">{{ subscription.subscriptionPlan?.name || 'N/A' }}</div>
                  <div class="plan-type">{{ subscription.subscriptionPlan?.description || 'N/A' }}</div>
                </div>
              </td>
              <td>
                <span class="status-badge" [class]="'status-' + subscription.status.toLowerCase()">
                  {{ subscription.status }}
                </span>
              </td>
              <td>{{ subscription.startDate | date:'MMM dd, yyyy' }}</td>
              <td>{{ subscription.nextBillingDate | date:'MMM dd, yyyy' }}</td>
              <td>
                <div class="price-info">
                  <span class="currency">USD</span>
                  <span class="amount">{{ subscription.currentPrice | number:'1.2-2' }}</span>
                </div>
              </td>
              <td>
                <div class="action-buttons">
                  <button 
                    class="btn btn-sm btn-outline" 
                    (click)="viewSubscription(subscription.id)"
                    title="View Details">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button 
                    class="btn btn-sm btn-outline" 
                    (click)="editSubscription(subscription.id)"
                    title="Edit">
                    <i class="fas fa-edit"></i>
                  </button>
                  <div class="dropdown">
                    <button 
                      class="btn btn-sm btn-outline dropdown-toggle" 
                      (click)="toggleDropdown(subscription.id)"
                      title="More Actions">
                      <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="dropdown-menu" [class.show]="openDropdowns[subscription.id]">
                      <button 
                        class="dropdown-item" 
                        (click)="activateSubscription(subscription.id)"
                        *ngIf="subscription.status !== 'Active'">
                        <i class="fas fa-play"></i> Activate
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="pauseSubscription(subscription.id)"
                        *ngIf="subscription.status === 'Active'">
                        <i class="fas fa-pause"></i> Pause
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="cancelSubscription(subscription.id)"
                        *ngIf="subscription.status !== 'Cancelled'">
                        <i class="fas fa-stop"></i> Cancel
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="renewSubscription(subscription.id)"
                        *ngIf="subscription.status === 'Expired'">
                        <i class="fas fa-redo"></i> Renew
                      </button>
                      <div class="dropdown-divider"></div>
                      <button 
                        class="dropdown-item text-danger" 
                        (click)="deleteSubscription(subscription.id)">
                        <i class="fas fa-trash"></i> Delete
                      </button>
                    </div>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Loading State -->
        <div class="loading-state" *ngIf="loading">
          <div class="spinner"></div>
          <p>Loading subscriptions...</p>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!loading && subscriptions.length === 0">
          <i class="fas fa-inbox"></i>
          <h3>No subscriptions found</h3>
          <p>Try adjusting your filters or create a new subscription to get started.</p>
          <button class="btn btn-primary" (click)="openCreateModal()">
            Create First Subscription
          </button>
        </div>
      </div>

      <!-- Pagination -->
      <div class="pagination" *ngIf="paginationInfo.totalPages > 1">
        <button 
          class="btn btn-outline" 
          [disabled]="currentPage === 1"
          (click)="goToPage(currentPage - 1)">
          <i class="fas fa-chevron-left"></i> Previous
        </button>
        
        <div class="page-numbers">
          <button 
            *ngFor="let page of getPageNumbers()" 
            class="btn" 
            [class.active]="page === currentPage"
            [class.disabled]="page === '...'"
            (click)="page !== '...' ? goToPage(+page) : null">
            {{ page }}
          </button>
        </div>
        
        <button 
          class="btn btn-outline" 
          [disabled]="currentPage === paginationInfo.totalPages"
          (click)="goToPage(currentPage + 1)">
          Next <i class="fas fa-chevron-right"></i>
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./subscription-list.component.scss']
})
export class SubscriptionListComponent implements OnInit, OnDestroy {
  // Data properties
  subscriptions: Subscription[] = [];
  subscriptionPlans: SubscriptionPlan[] = [];
  users: User[] = [];
  
  // Form and filter properties
  filterForm!: FormGroup;
  
  // Pagination properties
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  paginationInfo = {
    totalRecords: 0,
    totalPages: 0,
    startIndex: 0,
    endIndex: 0
  };
  
  // Selection properties
  selectedSubscriptions: string[] = [];
  openDropdowns: { [key: string]: boolean } = {};
  
  // State properties
  loading = false;
  error: string | null = null;
  
  // Subscription statuses for filter
  subscriptionStatuses: SubscriptionStatus[] = [
    'Pending', 'Active', 'Paused', 'Cancelled', 'Expired', 
    'PaymentFailed', 'TrialActive', 'TrialExpired', 'Suspended'
  ];
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private subscriptionService: AdminSubscriptionService,
    private billingService: AdminBillingService,
    private notificationService: AdminNotificationService,
    private router: Router
  ) {
    this.initializeFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscriptions();
    this.loadSubscriptions();
    this.loadSubscriptionPlans();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Form initialization
  private initializeFilterForm(): void {
    this.filterForm = this.fb.group({
      search: [''],
      status: [''],
      planId: [''],
      dateRange: [''],
      startDate: [''],
      endDate: ['']
    });
  }

  // Setup filter subscriptions
  private setupFilterSubscriptions(): void {
    this.filterForm.get('search')?.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadSubscriptions();
      });

    this.filterForm.get('status')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadSubscriptions();
      });

    this.filterForm.get('planId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadSubscriptions();
      });

    this.filterForm.get('dateRange')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        this.handleDateRangeChange(value);
      });
  }

  // Load subscriptions with filters
  loadSubscriptions(): void {
    this.loading = true;
    this.error = null;
    
    const params: SubscriptionListParams = {
      page: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.filterForm.get('search')?.value || undefined,
      status: this.filterForm.get('status')?.value ? [this.filterForm.get('status')?.value] : undefined,
      planId: this.filterForm.get('planId')?.value ? [this.filterForm.get('planId')?.value] : undefined,
      dateFrom: this.filterForm.get('startDate')?.value || undefined,
      dateTo: this.filterForm.get('endDate')?.value || undefined
    };

    this.subscriptionService.getAllSubscriptions(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<SubscriptionListResponse>) => {
          if (response.statusCode === 200) {
            this.subscriptions = response.data.data || [];
            this.totalCount = response.data.meta.totalRecords || 0;
            this.currentPage = response.data.meta.currentPage || 1;
            this.totalPages = response.data.meta.totalPages || 0;
            this.pageSize = response.data.meta.pageSize || 10;
          } else {
            this.error = response.message || 'Failed to load subscriptions';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'An error occurred while loading subscriptions';
          this.loading = false;
          console.error('Error loading subscriptions:', error);
        }
      });
  }

  // Load subscription plans for filter
  loadSubscriptionPlans(): void {
    this.subscriptionService.getSubscriptionPlans()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<SubscriptionPlan[]>) => {
          if (response.statusCode === 200) {
            this.subscriptionPlans = response.data || [];
          }
        },
        error: (error: any) => {
          console.error('Error loading subscription plans:', error);
        }
      });
  }

  // Load users for reference
  loadUsers(): void {
    // This would typically come from a user service
    // For now, we'll extract from subscriptions
  }

  // Handle date range filter changes
  private handleDateRangeChange(dateRange: string): void {
    const today = new Date();
    let startDate: Date | null = null;
    let endDate: Date | null = null;

    switch (dateRange) {
      case 'today':
        startDate = today;
        endDate = today;
        break;
      case 'week':
        const weekStart = new Date(today);
        weekStart.setDate(today.getDate() - today.getDay());
        startDate = weekStart;
        endDate = today;
        break;
      case 'month':
        const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
        startDate = monthStart;
        endDate = today;
        break;
      case 'quarter':
        const quarter = Math.floor(today.getMonth() / 3);
        const quarterStart = new Date(today.getFullYear(), quarter * 3, 1);
        startDate = quarterStart;
        endDate = today;
        break;
      case 'year':
        const yearStart = new Date(today.getFullYear(), 0, 1);
        startDate = yearStart;
        endDate = today;
        break;
    }

    this.filterForm.patchValue({
      startDate: startDate,
      endDate: endDate
    });

    this.currentPage = 1;
    this.loadSubscriptions();
  }

  // Clear all filters
  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  // Pagination methods
  onPageSizeChange(): void {
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.paginationInfo.totalPages) {
      this.currentPage = page;
      this.loadSubscriptions();
    }
  }

  getPageNumbers(): (number | string)[] {
    const pages: (number | string)[] = [];
    const totalPages = this.paginationInfo.totalPages;
    const current = this.currentPage;

    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 4) {
        for (let i = 1; i <= 5; i++) {
          pages.push(i);
        }
        pages.push('...');
        pages.push(totalPages);
      } else if (current >= totalPages - 3) {
        pages.push(1);
        pages.push('...');
        for (let i = totalPages - 4; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push('...');
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push('...');
        pages.push(totalPages);
      }
    }

    return pages;
  }

  // Selection methods
  isAllSelected(): boolean {
    return this.subscriptions.length > 0 && this.selectedSubscriptions.length === this.subscriptions.length;
  }

  toggleSelectAll(event: any): void {
    if (event.target.checked) {
      this.selectedSubscriptions = this.subscriptions.map(s => s.id);
    } else {
      this.selectedSubscriptions = [];
    }
  }

  isSelected(id: string): boolean {
    return this.selectedSubscriptions.includes(id);
  }

  toggleSelection(id: string): void {
    const index = this.selectedSubscriptions.indexOf(id);
    if (index > -1) {
      this.selectedSubscriptions.splice(index, 1);
    } else {
      this.selectedSubscriptions.push(id);
    }
  }

  clearSelection(): void {
      this.selectedSubscriptions = [];
  }

  // Dropdown methods
  toggleDropdown(id: string): void {
    this.openDropdowns[id] = !this.openDropdowns[id];
    // Close other dropdowns
    Object.keys(this.openDropdowns).forEach(key => {
      if (key !== id) {
        this.openDropdowns[key] = false;
      }
    });
  }

  // Navigation methods
  openCreateModal(): void {
    // TODO: Implement create subscription modal
    console.log('Open create subscription modal');
  }

  viewSubscription(id: string): void {
    this.router.navigate(['/admin-portal/subscriptions', id]);
  }

  editSubscription(id: string): void {
    this.router.navigate(['/admin-portal/subscriptions', id, 'edit']);
  }

  // Subscription lifecycle methods
  activateSubscription(id: string): void {
    this.subscriptionService.activateSubscription(id, 'Activated by admin')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<boolean>) => {
          if (response.statusCode === 200) {
            this.loadSubscriptions();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to activate subscription:', response.message || response.Message);
          }
        },
        error: (error) => {
          console.error('Error activating subscription:', error);
          // TODO: Show error notification
        }
      });
  }

  pauseSubscription(id: string): void {
    this.subscriptionService.pauseSubscription(id, 'Paused by admin')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<boolean>) => {
          if (response.statusCode === 200) {
            this.loadSubscriptions();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to pause subscription:', response.message || response.Message);
          }
        },
        error: (error) => {
          console.error('Error pausing subscription:', error);
          // TODO: Show error notification
        }
      });
  }

  cancelSubscription(id: string): void {
    this.subscriptionService.cancelSubscription(id, 'Cancelled by admin')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<boolean>) => {
          if (response.statusCode === 200) {
    this.loadSubscriptions();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to cancel subscription:', response.message || response.Message);
          }
        },
        error: (error) => {
          console.error('Error cancelling subscription:', error);
          // TODO: Show error notification
        }
      });
  }

  renewSubscription(id: string): void {
    // TODO: Implement renewal logic
    console.log('Renew subscription:', id);
  }

  deleteSubscription(id: string): void {
    if (confirm('Are you sure you want to delete this subscription? This action cannot be undone.')) {
      this.subscriptionService.deleteSubscription(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.StatusCode === 200) {
    this.loadSubscriptions();
              // TODO: Show success notification
            } else {
              // TODO: Show error notification
              console.error('Failed to delete subscription:', response.Message);
            }
          },
          error: (error) => {
            console.error('Error deleting subscription:', error);
            // TODO: Show error notification
          }
        });
    }
  }

  // Bulk actions
  bulkActivate(): void {
    if (this.selectedSubscriptions.length === 0) return;

    const request: BulkActionRequest = {
      subscriptionIds: this.selectedSubscriptions,
      action: 'activate',
      reason: 'Bulk activation by admin'
    };

    this.subscriptionService.bulkAction(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.StatusCode === 200) {
    this.loadSubscriptions();
            this.clearSelection();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to bulk activate subscriptions:', response.Message);
          }
        },
        error: (error) => {
          console.error('Error bulk activating subscriptions:', error);
          // TODO: Show error notification
        }
      });
  }

  bulkPause(): void {
    if (this.selectedSubscriptions.length === 0) return;

    const request: BulkActionRequest = {
      subscriptionIds: this.selectedSubscriptions,
      action: 'pause',
      reason: 'Bulk pause by admin'
    };

    this.subscriptionService.bulkAction(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.loadSubscriptions();
            this.clearSelection();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to bulk pause subscriptions:', response.message);
          }
        },
        error: (error) => {
          console.error('Error bulk pausing subscriptions:', error);
          // TODO: Show error notification
        }
      });
  }

  bulkCancel(): void {
    if (this.selectedSubscriptions.length === 0) return;

    if (confirm(`Are you sure you want to cancel ${this.selectedSubscriptions.length} subscription(s)? This action cannot be undone.`)) {
          const request: BulkActionRequest = {
      subscriptionIds: this.selectedSubscriptions,
      action: 'cancel',
      reason: 'Bulk cancellation by admin'
    };

      this.subscriptionService.bulkAction(request)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
                  next: (response) => {
          if (response.statusCode === 200) {
            this.loadSubscriptions();
            this.clearSelection();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to bulk cancel subscriptions:', response.message);
          }
        },
          error: (error) => {
            console.error('Error bulk cancelling subscriptions:', error);
            // TODO: Show error notification
          }
        });
    }
  }

  // Export functionality
  exportSubscriptions(): void {
    const params: SubscriptionListParams = {
      page: 1,
      pageSize: 10000, // Export all
      searchTerm: this.filterForm.get('search')?.value || undefined,
      status: this.filterForm.get('status')?.value ? [this.filterForm.get('status')?.value] : undefined,
      planId: this.filterForm.get('planId')?.value ? [this.filterForm.get('planId')?.value] : undefined,
      dateFrom: this.filterForm.get('startDate')?.value || undefined,
      dateTo: this.filterForm.get('endDate')?.value || undefined
    };

    this.subscriptionService.exportSubscriptions(params, 'csv')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `subscriptions_${new Date().toISOString().split('T')[0]}.csv`;
          document.body.appendChild(a);
          a.click();
          window.URL.revokeObjectURL(url);
          document.body.removeChild(a);
        },
        error: (error) => {
          console.error('Error exporting subscriptions:', error);
          // TODO: Show error notification
        }
      });
  }
}
