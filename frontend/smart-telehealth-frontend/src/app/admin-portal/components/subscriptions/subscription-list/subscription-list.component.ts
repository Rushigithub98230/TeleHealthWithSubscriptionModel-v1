import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SubscriptionFormModalComponent } from '../subscription-form-modal/subscription-form-modal.component';
import { BulkActionModalComponent } from '../../../../shared/components/bulk-action-modal/bulk-action-modal.component';
import { SubscriptionService, Subscription, SubscriptionStatus, SubscriptionPlan } from '../../../services/subscription.service';
import { ToastService } from '../../../../core/services/toast.service';


@Component({
  selector: 'app-subscription-list',
  templateUrl: './subscription-list.component.html',
  styleUrls: ['./subscription-list.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ConfirmDialogComponent, SubscriptionFormModalComponent, BulkActionModalComponent]
})
export class SubscriptionListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Math reference for template
  Math = Math;

  // Data
  subscriptions: Subscription[] = [];
  subscriptionPlans: SubscriptionPlan[] = [];
  filteredSubscriptions: Subscription[] = [];
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Loading states
  isLoading = false;
  isRefreshing = false;

  // Search and filters
  searchForm: FormGroup;
  selectedStatuses: SubscriptionStatus[] = [];
  selectedPlans: number[] = [];
  dateRange: { start: Date | null; end: Date | null } = { start: null, end: null };

  // Bulk operations
  selectedSubscriptions: number[] = [];
  isBulkActionLoading = false;

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  showBulkActionModal = false;
  selectedSubscription: Subscription | null = null;

  // Status options
  statusOptions: { value: SubscriptionStatus; label: string; color: string }[] = [
    { value: 'Active', label: 'Active', color: '#10b981' },
    { value: 'Inactive', label: 'Inactive', color: '#6b7280' },
    { value: 'Suspended', label: 'Suspended', color: '#f59e0b' },
    { value: 'Cancelled', label: 'Cancelled', color: '#ef4444' },
    { value: 'Expired', label: 'Expired', color: '#8b5cf6' },
    { value: 'Pending', label: 'Pending', color: '#3b82f6' }
  ];

  constructor(
    private fb: FormBuilder,
    private subscriptionService: SubscriptionService,
    private toastService: ToastService
  ) {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      status: [[]],
      planId: [[]],
      dateFrom: [null],
      dateTo: [null]
    });
  }

  ngOnInit(): void {
    this.loadSubscriptions();
    this.loadSubscriptionPlans();
    this.setupSearchListener();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setupSearchListener(): void {
    this.searchForm.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.applyFilters();
      });
  }

  loadSubscriptions(): void {
    this.isLoading = true;
    
    const params = {
      page: this.currentPage,
      pageSize: this.pageSize,
      ...this.getFilterParams()
    };

    this.subscriptionService.getSubscriptions(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.subscriptions = response.data;
            this.totalItems = response.meta?.totalRecords || 0;
            this.totalPages = response.meta?.totalPages || 0;
            this.applyFilters();
          } else {
            this.toastService.showError(response.message || 'Failed to load subscriptions');
          }
        },
        error: (error) => {
          this.toastService.showError('Failed to load subscriptions');
          console.error('Error loading subscriptions:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  loadSubscriptionPlans(): void {
    this.subscriptionService.getSubscriptionPlans()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.subscriptionPlans = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading subscription plans:', error);
        }
      });
  }

  getFilterParams(): any {
    const formValue = this.searchForm.value;
    const params: any = {};

    if (formValue.searchTerm) {
      params.searchTerm = formValue.searchTerm;
    }

    if (formValue.status && formValue.status.length > 0) {
      params.status = formValue.status.join(',');
    }

    if (formValue.planId && formValue.planId.length > 0) {
      params.planId = formValue.planId.join(',');
    }

    if (formValue.dateFrom) {
      params.dateFrom = formValue.dateFrom;
    }

    if (formValue.dateTo) {
      params.dateTo = formValue.dateTo;
    }

    return params;
  }

  applyFilters(): void {
    let filtered = [...this.subscriptions];

    // Apply search term filter
    const searchTerm = this.searchForm.get('searchTerm')?.value?.toLowerCase();
    if (searchTerm) {
      filtered = filtered.filter(sub => 
        sub.user?.firstName?.toLowerCase().includes(searchTerm) ||
        sub.user?.lastName?.toLowerCase().includes(searchTerm) ||
        sub.user?.email?.toLowerCase().includes(searchTerm) ||
        sub.subscriptionPlan?.name?.toLowerCase().includes(searchTerm)
      );
    }

    // Apply status filter
    const selectedStatuses = this.searchForm.get('status')?.value;
    if (selectedStatuses && selectedStatuses.length > 0) {
      filtered = filtered.filter(sub => selectedStatuses.includes(sub.status));
    }

    // Apply plan filter
    const selectedPlans = this.searchForm.get('planId')?.value;
    if (selectedPlans && selectedPlans.length > 0) {
      filtered = filtered.filter(sub => selectedPlans.includes(sub.subscriptionPlanId));
    }

    // Apply date range filter
    const dateFrom = this.searchForm.get('dateFrom')?.value;
    const dateTo = this.searchForm.get('dateTo')?.value;
    
    if (dateFrom) {
      filtered = filtered.filter(sub => new Date(sub.startDate) >= new Date(dateFrom));
    }
    
    if (dateTo) {
      filtered = filtered.filter(sub => new Date(sub.endDate) <= new Date(dateTo));
    }

    this.filteredSubscriptions = filtered;
  }

  onPageChange(page: number | string): void {
    if (typeof page === 'number') {
      this.currentPage = page;
      this.loadSubscriptions();
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target && target.value) {
      this.pageSize = +target.value;
      this.currentPage = 1;
      this.loadSubscriptions();
    }
  }

  refreshData(): void {
    this.isRefreshing = true;
    this.loadSubscriptions();
    setTimeout(() => {
      this.isRefreshing = false;
    }, 1000);
  }

  clearFilters(): void {
    this.searchForm.reset();
    this.currentPage = 1;
    this.loadSubscriptions();
  }

  // Selection methods
  toggleSubscriptionSelection(subscriptionId: number): void {
    const index = this.selectedSubscriptions.indexOf(subscriptionId);
    if (index > -1) {
      this.selectedSubscriptions.splice(index, 1);
    } else {
      this.selectedSubscriptions.push(subscriptionId);
    }
  }

  toggleAllSubscriptions(): void {
    if (this.selectedSubscriptions.length === this.filteredSubscriptions.length) {
      this.selectedSubscriptions = [];
    } else {
      this.selectedSubscriptions = this.filteredSubscriptions.map(sub => sub.id);
    }
  }

  isAllSelected(): boolean {
    return this.selectedSubscriptions.length === this.filteredSubscriptions.length && this.filteredSubscriptions.length > 0;
  }

  isPartiallySelected(): boolean {
    return this.selectedSubscriptions.length > 0 && this.selectedSubscriptions.length < this.filteredSubscriptions.length;
  }

  // Modal methods
  openCreateModal(): void {
    this.showCreateModal = true;
  }

  openEditModal(subscription: Subscription): void {
    this.selectedSubscription = subscription;
    this.showEditModal = true;
  }

  openDeleteModal(subscription: Subscription): void {
    this.selectedSubscription = subscription;
    this.showDeleteModal = true;
  }

  openBulkActionModal(): void {
    if (this.selectedSubscriptions.length === 0) {
      this.toastService.showWarning('Please select subscriptions first');
      return;
    }
    this.showBulkActionModal = true;
  }

  closeModals(): void {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.showBulkActionModal = false;
    this.selectedSubscription = null;
  }

  // CRUD operations
  onCreateSuccess(): void {
    this.closeModals();
    this.loadSubscriptions();
    this.toastService.showSuccess('Subscription created successfully');
  }

  onEditSuccess(): void {
    this.closeModals();
    this.loadSubscriptions();
    this.toastService.showSuccess('Subscription updated successfully');
  }

  onDeleteSuccess(): void {
    this.closeModals();
    this.loadSubscriptions();
    this.toastService.showSuccess('Subscription deleted successfully');
  }

  onBulkActionSuccess(): void {
    this.closeModals();
    this.selectedSubscriptions = [];
    this.loadSubscriptions();
    this.toastService.showSuccess('Bulk action completed successfully');
  }

  // Utility methods
  formatCurrency(amount: number | undefined): string {
    if (amount === undefined || amount === null) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getStatusColor(status: SubscriptionStatus): string {
    return this.subscriptionService.getStatusColor(status);
  }

  getStatusLabel(status: SubscriptionStatus): string {
    return this.subscriptionService.getStatusLabel(status);
  }

  getPlanName(planId: number): string {
    const plan = this.subscriptionPlans.find(p => p.id === planId);
    return plan?.name || 'Unknown Plan';
  }

  formatDate(date: string | Date): string {
    return this.subscriptionService.formatDate(date);
  }

  getDaysUntilExpiry(endDate: string | Date): number {
    return this.subscriptionService.getDaysUntilExpiry(endDate);
  }

  isExpiringSoon(endDate: string | Date): boolean {
    return this.subscriptionService.isExpiringSoon(endDate);
  }

  isExpired(endDate: string | Date): boolean {
    return this.subscriptionService.isExpired(endDate);
  }

  // Export functionality
  exportSubscriptions(): void {
    const params = this.getFilterParams();
    this.subscriptionService.exportSubscriptions(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: Blob) => {
          const blob = new Blob([response], { type: 'text/csv' });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `subscriptions-${new Date().toISOString().split('T')[0]}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.toastService.showSuccess('Subscriptions exported successfully');
        },
        error: (error) => {
          this.toastService.showError('Failed to export subscriptions');
          console.error('Export error:', error);
        }
      });
  }

  // Pagination helper
  getPageNumbers(): (number | string)[] {
    const pageNumbers: (number | string)[] = [];
    const maxPagesToShow = 5;
    const startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (startPage > 1) {
      pageNumbers.push(1);
      if (startPage > 2) {
        pageNumbers.push('...');
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pageNumbers.push(i);
    }

    if (endPage < this.totalPages) {
      if (endPage < this.totalPages - 1) {
        pageNumbers.push('...');
      }
      pageNumbers.push(this.totalPages);
    }

    return pageNumbers;
  }
}
