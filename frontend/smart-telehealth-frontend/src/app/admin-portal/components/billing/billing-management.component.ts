import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { AdminBillingService } from '../../services/admin-billing.service';
import { AdminSubscriptionService } from '../../services/admin-subscription.service';
import { 
  BillingRecord, 
  BillingListParams, 
  BillingStatus, 
  BillingType,
  BillingAnalytics,
  PaymentProcessingDto,
  RefundRequestDto
} from '../../models/billing.interface';
import { JsonModel } from '../../../core/models/json-model.interface';

@Component({
  selector: 'app-billing-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MatSnackBarModule],
  template: `
    <div class="billing-management-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Billing Management</h1>
          <p>Manage all billing records, payments, and financial operations</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="openCreateBillingModal()"
            [disabled]="loading">
            <i class="fas fa-plus"></i> Create Billing Record
          </button>
          <button 
            class="btn btn-secondary" 
            (click)="exportBillingRecords()"
            [disabled]="loading || billingRecords.length === 0">
            <i class="fas fa-download"></i> Export
          </button>
        </div>
      </div>

      <!-- Analytics Dashboard -->
      <div class="analytics-section" *ngIf="analytics">
        <div class="analytics-grid">
          <div class="analytics-card">
            <div class="card-header">
              <h3>Total Revenue</h3>
              <i class="fas fa-dollar-sign"></i>
            </div>
            <div class="card-value">{{ analytics.totalRevenue | currency:'USD':'symbol':'1.2-2' }}</div>
            <div class="card-trend positive">+12.5% from last month</div>
          </div>
          
          <div class="analytics-card">
            <div class="card-header">
              <h3>Pending Amount</h3>
              <i class="fas fa-clock"></i>
            </div>
            <div class="card-value">{{ analytics.pendingAmount | currency:'USD':'symbol':'1.2-2' }}</div>
            <div class="card-trend neutral">No change</div>
          </div>
          
          <div class="analytics-card">
            <div class="card-header">
              <h3>Overdue Amount</h3>
              <i class="fas fa-exclamation-triangle"></i>
            </div>
            <div class="card-value">{{ analytics.overdueAmount | currency:'USD':'symbol':'1.2-2' }}</div>
            <div class="card-trend negative">+5.2% from last month</div>
          </div>
          
          <div class="analytics-card">
            <div class="card-header">
              <h3>Failed Payments</h3>
              <i class="fas fa-times-circle"></i>
            </div>
            <div class="card-value">{{ analytics.failedAmount | currency:'USD':'symbol':'1.2-2' }}</div>
            <div class="card-trend negative">+2.1% from last month</div>
          </div>
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
                formControlName="searchTerm"
                placeholder="Search by user, invoice, or description..."
                class="form-control">
            </div>
            <div class="filter-group">
              <label for="status">Status</label>
              <select id="status" formControlName="status" class="form-control">
                <option value="">All Statuses</option>
                <option *ngFor="let status of billingStatuses" [value]="status">
                  {{ status }}
                </option>
              </select>
            </div>
            <div class="filter-group">
              <label for="type">Type</label>
              <select id="type" formControlName="type" class="form-control">
                <option value="">All Types</option>
                <option *ngFor="let type of billingTypes" [value]="type">
                  {{ type }}
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
      <div class="bulk-actions" *ngIf="selectedBillingRecords.length > 0">
        <div class="bulk-info">
          <span>{{ selectedBillingRecords.length }} billing record(s) selected</span>
        </div>
        <div class="bulk-buttons">
          <button 
            class="btn btn-success" 
            (click)="bulkProcessPayments()"
            [disabled]="loading">
            Process Payments
          </button>
          <button 
            class="btn btn-warning" 
            (click)="bulkSendReminders()"
            [disabled]="loading">
            Send Reminders
          </button>
          <button 
            class="btn btn-outline" 
            (click)="clearSelection()">
            Clear Selection
          </button>
        </div>
      </div>

      <!-- Billing Records Table -->
      <div class="table-container">
        <div class="table-header">
          <div class="table-info">
            <span>Showing {{ paginationInfo.startIndex + 1 }}-{{ paginationInfo.endIndex }} of {{ paginationInfo.totalRecords }} billing records</span>
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

        <table class="billing-table">
          <thead>
            <tr>
              <th>
                <input 
                  type="checkbox" 
                  [checked]="isAllSelected()"
                  (change)="toggleSelectAll($event)">
              </th>
              <th>Invoice #</th>
              <th>User</th>
              <th>Type</th>
              <th>Status</th>
              <th>Amount</th>
              <th>Due Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let record of billingRecords" 
                [class.selected]="isSelected(record.id)"
                [class.overdue]="record.isOverdue">
              <td>
                <input 
                  type="checkbox" 
                  [checked]="isSelected(record.id)"
                  (change)="toggleSelection(record.id)">
              </td>
              <td>
                <div class="invoice-info">
                  <div class="invoice-number">{{ record.invoiceNumber || 'N/A' }}</div>
                  <div class="record-id">ID: {{ record.id }}</div>
                </div>
              </td>
              <td>
                <div class="user-info">
                  <div class="user-name">{{ record.user?.firstName }} {{ record.user?.lastName }}</div>
                  <div class="user-email">{{ record.user?.email || 'N/A' }}</div>
                </div>
              </td>
              <td>
                <span class="type-badge type-{{ record.type.toLowerCase() }}">
                  {{ record.type }}
                </span>
              </td>
              <td>
                <span class="status-badge status-{{ record.status.toLowerCase() }}">
                  {{ record.status }}
                </span>
              </td>
              <td>
                <div class="amount-info">
                  <span class="currency">USD</span>
                  <span class="amount">{{ record.totalAmount | number:'1.2-2' }}</span>
                </div>
              </td>
              <td>{{ record.dueDate | date:'MMM dd, yyyy' }}</td>
              <td>
                <div class="action-buttons">
                  <button 
                    class="btn btn-sm btn-outline" 
                    (click)="viewBillingRecord(record.id)"
                    title="View Details">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button 
                    class="btn btn-sm btn-outline" 
                    (click)="editBillingRecord(record.id)"
                    title="Edit">
                    <i class="fas fa-edit"></i>
                  </button>
                  <div class="dropdown">
                    <button 
                      class="btn btn-sm btn-outline dropdown-toggle" 
                      (click)="toggleDropdown(record.id)"
                      title="More Actions">
                      <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="dropdown-menu" [class.show]="openDropdowns[record.id]">
                      <button 
                        class="dropdown-item" 
                        (click)="processPayment(record.id)"
                        *ngIf="record.status === 'Pending'">
                        <i class="fas fa-credit-card"></i> Process Payment
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="sendReminder(record.id)"
                        *ngIf="record.status === 'Pending'">
                        <i class="fas fa-bell"></i> Send Reminder
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="processRefund(record.id)"
                        *ngIf="record.status === 'Paid'">
                        <i class="fas fa-undo"></i> Process Refund
                      </button>
                      <button 
                        class="dropdown-item" 
                        (click)="generateInvoice(record.id)"
                        *ngIf="record.status === 'Pending'">
                        <i class="fas fa-file-invoice"></i> Generate Invoice
                      </button>
                      <div class="dropdown-divider"></div>
                      <button 
                        class="dropdown-item text-danger" 
                        (click)="deleteBillingRecord(record.id)">
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
          <p>Loading billing records...</p>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!loading && billingRecords.length === 0">
          <i class="fas fa-file-invoice"></i>
          <h3>No billing records found</h3>
          <p>Try adjusting your filters or create a new billing record to get started.</p>
          <button class="btn btn-primary" (click)="openCreateBillingModal()">
            Create First Billing Record
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
  styleUrls: ['./billing-management.component.scss']
})
export class BillingManagementComponent implements OnInit, OnDestroy {
  // Data properties
  billingRecords: BillingRecord[] = [];
  analytics: BillingAnalytics | null = null;
  
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
  selectedBillingRecords: string[] = [];
  openDropdowns: { [key: string]: boolean } = {};
  
  // State properties
  loading = false;
  error: string | null = null;
  
  // Billing statuses and types for filter
  billingStatuses: BillingStatus[] = ['Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded', 'Overdue'];
  billingTypes: BillingType[] = ['Subscription', 'Consultation', 'Medication', 'LateFee', 'Refund', 'Recurring', 'Upfront', 'Bundle', 'Invoice', 'Cycle'];
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private billingService: AdminBillingService,
    private subscriptionService: AdminSubscriptionService,
    private snackBar: MatSnackBar
  ) {
    this.initializeFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscriptions();
    this.loadBillingRecords();
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Form initialization
  private initializeFilterForm(): void {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      status: [''],
      type: [''],
      dateRange: [''],
      startDate: [''],
      endDate: ['']
    });
  }

  // Setup filter subscriptions
  private setupFilterSubscriptions(): void {
    this.filterForm.get('searchTerm')?.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadBillingRecords();
      });

    this.filterForm.get('status')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadBillingRecords();
      });

    this.filterForm.get('type')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadBillingRecords();
      });

    this.filterForm.get('dateRange')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((value) => {
        this.handleDateRangeChange(value);
      });
  }

  // Load billing records with filters
  loadBillingRecords(): void {
    this.loading = true;
    this.error = null;
    
    const params: BillingListParams = {
      page: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.filterForm.get('searchTerm')?.value || undefined,
      status: this.filterForm.get('status')?.value ? [this.filterForm.get('status')?.value] : undefined,
      type: this.filterForm.get('type')?.value ? [this.filterForm.get('type')?.value] : undefined,
      dateFrom: this.filterForm.get('startDate')?.value || undefined,
      dateTo: this.filterForm.get('endDate')?.value || undefined
    };

    this.billingService.getAllBillingRecords(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<BillingRecord[]>) => {
          if (response.statusCode === 200) {
            this.billingRecords = response.data || [];
            this.totalCount = response.meta?.totalRecords || 0;
            this.currentPage = response.meta?.currentPage || 1;
            this.totalPages = response.meta?.totalPages || 0;
            this.pageSize = response.meta?.pageSize || 10;
          } else {
            this.error = response.message || 'Failed to load billing records';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'An error occurred while loading billing records';
          this.loading = false;
          console.error('Error loading billing records:', error);
        }
      });
  }

  // Load analytics
  loadAnalytics(): void {
    // TODO: Implement analytics loading
    this.analytics = {
      totalRevenue: 125000.00,
      pendingAmount: 15000.00,
      overdueAmount: 5000.00,
      failedAmount: 2500.00,
      refundedAmount: 1000.00,
      subscriptionRevenue: 100000.00,
      consultationRevenue: 20000.00,
      medicationRevenue: 5000.00,
      monthlyTrend: [],
      statusDistribution: [],
      topUsers: []
    };
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
    this.loadBillingRecords();
  }

  // Clear all filters
  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadBillingRecords();
  }

  // Pagination methods
  onPageSizeChange(): void {
    this.currentPage = 1;
    this.loadBillingRecords();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.paginationInfo.totalPages) {
      this.currentPage = page;
      this.loadBillingRecords();
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
    return this.billingRecords.length > 0 && this.selectedBillingRecords.length === this.billingRecords.length;
  }

  toggleSelectAll(event: any): void {
    if (event.target.checked) {
      this.selectedBillingRecords = this.billingRecords.map(r => r.id);
    } else {
      this.selectedBillingRecords = [];
    }
  }

  isSelected(id: string): boolean {
    return this.selectedBillingRecords.includes(id);
  }

  toggleSelection(id: string): void {
    const index = this.selectedBillingRecords.indexOf(id);
    if (index > -1) {
      this.selectedBillingRecords.splice(index, 1);
    } else {
      this.selectedBillingRecords.push(id);
    }
  }

  clearSelection(): void {
    this.selectedBillingRecords = [];
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

  // Action methods
  openCreateBillingModal(): void {
    // TODO: Implement create billing modal
    console.log('Open create billing modal');
  }

  viewBillingRecord(id: string): void {
    // TODO: Implement view billing record
    console.log('View billing record:', id);
  }

  editBillingRecord(id: string): void {
    // TODO: Implement edit billing record
    console.log('Edit billing record:', id);
  }

  processPayment(id: string): void {
    // TODO: Implement process payment
    console.log('Process payment for:', id);
  }

  sendReminder(id: string): void {
    // TODO: Implement send reminder
    console.log('Send reminder for:', id);
  }

  processRefund(id: string): void {
    // TODO: Implement process refund
    console.log('Process refund for:', id);
  }

  generateInvoice(id: string): void {
    // TODO: Implement generate invoice
    console.log('Generate invoice for:', id);
  }

  deleteBillingRecord(id: string): void {
    if (confirm('Are you sure you want to delete this billing record? This action cannot be undone.')) {
      // TODO: Implement delete billing record
      console.log('Delete billing record:', id);
    }
  }

  // Bulk actions
  bulkProcessPayments(): void {
    if (this.selectedBillingRecords.length === 0) return;
    // TODO: Implement bulk process payments
    console.log('Bulk process payments for:', this.selectedBillingRecords);
  }

  bulkSendReminders(): void {
    if (this.selectedBillingRecords.length === 0) return;
    // TODO: Implement bulk send reminders
    console.log('Bulk send reminders for:', this.selectedBillingRecords);
  }

  // Export functionality
  exportBillingRecords(): void {
    const params: BillingListParams = {
      page: 1,
      pageSize: 10000, // Export all
      searchTerm: this.filterForm.get('searchTerm')?.value || undefined,
      status: this.filterForm.get('status')?.value ? [this.filterForm.get('status')?.value] : undefined,
      type: this.filterForm.get('type')?.value ? [this.filterForm.get('type')?.value] : undefined,
      dateFrom: this.filterForm.get('startDate')?.value || undefined,
      dateTo: this.filterForm.get('endDate')?.value || undefined
    };

    // TODO: Implement export functionality
    console.log('Export billing records with params:', params);
  }

  // Utility methods
  private showSuccessMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  private showErrorMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 7000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
}
