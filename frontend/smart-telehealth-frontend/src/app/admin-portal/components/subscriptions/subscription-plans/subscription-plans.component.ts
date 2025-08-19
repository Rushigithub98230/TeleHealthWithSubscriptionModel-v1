import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { SubscriptionService, SubscriptionPlan, BillingCycle } from '../../../services/subscription.service';
import { PrivilegeService, Privilege } from '../../../services/privilege.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SubscriptionPlanFormModalComponent } from '../subscription-plan-form-modal/subscription-plan-form-modal.component';

@Component({
  selector: 'app-subscription-plans',
  templateUrl: './subscription-plans.component.html',
  styleUrls: ['./subscription-plans.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    ConfirmDialogComponent,
    SubscriptionPlanFormModalComponent
  ]
})
export class SubscriptionPlansComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Math reference for template
  Math = Math;

  // Data
  subscriptionPlans: SubscriptionPlan[] = [];
  filteredPlans: SubscriptionPlan[] = [];
  privileges: Privilege[] = [];
  
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
  selectedBillingCycles: BillingCycle[] = [];
  selectedStatus = '';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedPlan: SubscriptionPlan | null = null;

  // Bulk operations
  selectedPlans: number[] = [];
  isBulkActionLoading = false;

  constructor(
    private fb: FormBuilder,
    private subscriptionService: SubscriptionService,
    private privilegeService: PrivilegeService,
    private snackBar: MatSnackBar
  ) {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      billingCycle: [[]],
      isActive: ['']
    });
  }

  ngOnInit(): void {
    this.loadSubscriptionPlans();
    this.loadPrivileges();
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

  loadSubscriptionPlans(): void {
    this.isLoading = true;
    
    const params = {
      page: this.currentPage,
      pageSize: this.pageSize,
      ...this.getFilterParams()
    };

    this.subscriptionService.getSubscriptionPlans(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.subscriptionPlans = response.data;
            this.totalItems = response.meta?.totalRecords || 0;
            this.totalPages = response.meta?.totalPages || 0;
            this.applyFilters();
                      } else {
              this.showErrorMessage(response.message || 'Failed to load subscription plans');
            }
          },
          error: (error) => {
            this.showErrorMessage('Failed to load subscription plans');
          console.error('Error loading subscription plans:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  loadPrivileges(): void {
    this.privilegeService.getPrivileges()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.privileges = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading privileges:', error);
        }
      });
  }

  getFilterParams(): any {
    const formValue = this.searchForm.value;
    const params: any = {};

    if (formValue.searchTerm) {
      params.search = formValue.searchTerm;
    }

    if (formValue.billingCycle && formValue.billingCycle.length > 0) {
      params.billingCycle = formValue.billingCycle.join(',');
    }

    if (formValue.isActive !== '') {
      params.isActive = formValue.isActive;
    }

    return params;
  }

  applyFilters(): void {
    let filtered = [...this.subscriptionPlans];

    // Apply search term filter
    const searchTerm = this.searchForm.get('searchTerm')?.value?.toLowerCase();
    if (searchTerm) {
      filtered = filtered.filter(plan => 
        plan.name.toLowerCase().includes(searchTerm) ||
        plan.description.toLowerCase().includes(searchTerm)
      );
    }

    // Apply billing cycle filter
    const selectedBillingCycles = this.searchForm.get('billingCycle')?.value;
    if (selectedBillingCycles && selectedBillingCycles.length > 0) {
      filtered = filtered.filter(plan => selectedBillingCycles.includes(plan.billingCycle));
    }

    // Apply status filter
    const selectedStatus = this.searchForm.get('isActive')?.value;
    if (selectedStatus !== '') {
      const isActive = selectedStatus === 'true';
      filtered = filtered.filter(plan => plan.isActive === isActive);
    }

    this.filteredPlans = filtered;
  }

  onPageChange(page: number | string): void {
    if (typeof page === 'number') {
      this.currentPage = page;
      this.loadSubscriptionPlans();
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target && target.value) {
      this.pageSize = +target.value;
      this.currentPage = 1;
      this.loadSubscriptionPlans();
    }
  }

  refreshData(): void {
    this.isRefreshing = true;
    this.loadSubscriptionPlans();
    setTimeout(() => {
      this.isRefreshing = false;
    }, 1000);
  }

  clearFilters(): void {
    this.searchForm.reset();
    this.currentPage = 1;
    this.loadSubscriptionPlans();
  }

  // Selection methods
  togglePlanSelection(planId: number): void {
    const index = this.selectedPlans.indexOf(planId);
    if (index > -1) {
      this.selectedPlans.splice(index, 1);
    } else {
      this.selectedPlans.push(planId);
    }
  }

  toggleAllPlans(): void {
    if (this.selectedPlans.length === this.filteredPlans.length) {
      this.selectedPlans = [];
    } else {
      this.selectedPlans = this.filteredPlans.map(plan => plan.id);
    }
  }

  isAllSelected(): boolean {
    return this.selectedPlans.length === this.filteredPlans.length && this.filteredPlans.length > 0;
  }

  isPartiallySelected(): boolean {
    return this.selectedPlans.length > 0 && this.selectedPlans.length < this.filteredPlans.length;
  }

  // Modal methods
  openCreateModal(): void {
    this.showCreateModal = true;
  }

  openEditModal(plan: SubscriptionPlan): void {
    this.selectedPlan = plan;
    this.showEditModal = true;
  }

  openDeleteModal(plan: SubscriptionPlan): void {
    this.selectedPlan = plan;
    this.showDeleteModal = true;
  }

  closeModals(): void {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.selectedPlan = null;
  }

  // CRUD operations
  onCreateSuccess(): void {
    this.closeModals();
    this.loadSubscriptionPlans();
    this.showSuccessMessage('Subscription plan created successfully');
  }

  onEditSuccess(): void {
    this.closeModals();
    this.loadSubscriptionPlans();
    this.showSuccessMessage('Subscription plan updated successfully');
  }

  onDeleteSuccess(): void {
    this.closeModals();
    this.loadSubscriptionPlans();
    this.showSuccessMessage('Subscription plan deleted successfully');
  }

  // Utility methods
  getBillingCycleLabel(billingCycle: BillingCycle): string {
    return this.subscriptionService.getBillingCycleLabel(billingCycle);
  }

  getBillingCycleDescription(billingCycle: BillingCycle): string {
    const cycleOption = this.subscriptionService.getBillingCycleOptions().find(option => option.value === billingCycle);
    return cycleOption?.description || '';
  }

  formatCurrency(amount: number): string {
    return this.subscriptionService.formatCurrency(amount);
  }

  formatDate(date: string | Date): string {
    return this.subscriptionService.formatDate(date);
  }

  getPrivilegeNames(privileges: any[]): string[] {
    return privileges?.map(p => p.privilege?.name || 'Unknown') || [];
  }

  getStatusColor(isActive: boolean): string {
    return isActive ? '#10b981' : '#6b7280';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  // Export functionality
  exportSubscriptionPlans(): void {
    const params = this.getFilterParams();
    
    this.subscriptionService.exportSubscriptionPlans(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: Blob) => {
          const blob = new Blob([response], { type: 'text/csv' });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `subscription-plans-${new Date().toISOString().split('T')[0]}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.showSuccessMessage('Subscription plans exported successfully');
        },
        error: (error) => {
          this.showErrorMessage('Failed to export subscription plans');
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

  // MatSnackBar notification methods
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

  private showWarningMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 6000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['warning-snackbar']
    });
  }

  private showInfoMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }
}
