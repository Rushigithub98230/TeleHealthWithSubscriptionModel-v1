import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { PrivilegeService, Privilege } from '../../../services/privilege.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PrivilegeFormModalComponent } from '../../privileges/privilege-form-modal/privilege-form-modal.component';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-privilege-management',
  templateUrl: './privilege-management.component.html',
  styleUrls: ['./privilege-management.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, ConfirmDialogComponent, PrivilegeFormModalComponent]
})
export class PrivilegeManagementComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Math reference for template
  Math = Math;

  // Data
  privileges: Privilege[] = [];
  filteredPrivileges: Privilege[] = [];
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Loading states
  isLoading = false;
  isRefreshing = false;

  // Search and filters
  searchTerm = '';
  selectedCategory = '';
  selectedStatus = '';

  // Modal states
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedPrivilege: Privilege | null = null;

  // Form
  privilegeForm: FormGroup;

  // Categories and status options
  categories: string[] = [];
  statusOptions: Array<{ value: string; label: string; color: string }> = [];

  constructor(
    private fb: FormBuilder,
    private privilegeService: PrivilegeService,
    private toastService: ToastService
  ) {
    this.privilegeForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      category: ['', Validators.required],
      isActive: [true],
      sortOrder: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.categories = this.privilegeService.getPrivilegeCategories();
    this.statusOptions = this.privilegeService.getStatusOptions();
    this.loadPrivileges();
    this.initializeDefaultPrivileges();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  initializeDefaultPrivileges(): void {
    // Initialize with the two required privileges for now
    const defaultPrivileges: Partial<Privilege>[] = [
      {
        name: 'Teleconsultation',
        description: 'Access to video consultations with healthcare providers',
        category: 'Telehealth Services',
        isActive: true,
        sortOrder: 1
      },
      {
        name: 'Medications',
        description: 'Access to medication management and prescriptions',
        category: 'Medical Services',
        isActive: true,
        sortOrder: 2
      }
    ];

    // Check if privileges exist, if not create them
    defaultPrivileges.forEach(privilege => {
      if (!this.privileges.find(p => p.name === privilege.name)) {
        this.createDefaultPrivilege(privilege);
      }
    });
  }

  createDefaultPrivilege(privilegeData: Partial<Privilege>): void {
    this.privilegeService.createPrivilege(privilegeData as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            console.log(`Default privilege ${privilegeData.name} created`);
          }
        },
        error: (error) => {
          console.error(`Error creating default privilege ${privilegeData.name}:`, error);
        }
      });
  }

  loadPrivileges(): void {
    this.isLoading = true;
    
    const params = {
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchTerm,
      category: this.selectedCategory,
      status: this.selectedStatus
    };

    this.privilegeService.getPrivileges(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.privileges = response.data;
            this.totalItems = response.meta?.totalRecords || 0;
            this.totalPages = response.meta?.totalPages || 0;
            this.applyFilters();
          } else {
            this.toastService.showError(response.message || 'Failed to load privileges');
          }
        },
        error: (error) => {
          this.toastService.showError('Failed to load privileges');
          console.error('Error loading privileges:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  applyFilters(): void {
    let filtered = [...this.privileges];

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(privilege =>
        privilege.name.toLowerCase().includes(term) ||
        privilege.description.toLowerCase().includes(term) ||
        privilege.category.toLowerCase().includes(term)
      );
    }

    // Apply category filter
    if (this.selectedCategory) {
      filtered = filtered.filter(privilege => privilege.category === this.selectedCategory);
    }

    // Apply status filter
    if (this.selectedStatus) {
      const isActive = this.selectedStatus === 'active';
      filtered = filtered.filter(privilege => privilege.isActive === isActive);
    }

    this.filteredPrivileges = filtered;
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onCategoryChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onStatusChange(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onPageChange(page: number | string): void {
    if (typeof page === 'number') {
      this.currentPage = page;
      this.loadPrivileges();
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target && target.value) {
      this.pageSize = +target.value;
      this.currentPage = 1;
      this.loadPrivileges();
    }
  }

  refreshData(): void {
    this.isRefreshing = true;
    this.loadPrivileges();
    setTimeout(() => {
      this.isRefreshing = false;
    }, 1000);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.loadPrivileges();
  }

  // Modal methods
  openCreateModal(): void {
    this.privilegeForm.reset({
      isActive: true,
      sortOrder: 0
    });
    this.showCreateModal = true;
  }

  openEditModal(privilege: Privilege): void {
    this.selectedPrivilege = privilege;
    this.privilegeForm.patchValue({
      name: privilege.name,
      description: privilege.description,
      category: privilege.category,
      isActive: privilege.isActive,
      sortOrder: privilege.sortOrder
    });
    this.showEditModal = true;
  }

  openDeleteModal(privilege: Privilege): void {
    this.selectedPrivilege = privilege;
    this.showDeleteModal = true;
  }

  closeModals(): void {
    this.showCreateModal = false;
    this.showEditModal = false;
    this.showDeleteModal = false;
    this.selectedPrivilege = null;
  }

  // CRUD operations
  onCreateSuccess(): void {
    this.closeModals();
    this.loadPrivileges();
    this.toastService.showSuccess('Privilege created successfully');
  }

  onEditSuccess(): void {
    this.closeModals();
    this.loadPrivileges();
    this.toastService.showSuccess('Privilege updated successfully');
  }

  onDeleteSuccess(): void {
    this.closeModals();
    this.loadPrivileges();
    this.toastService.showSuccess('Privilege deleted successfully');
  }

  // Utility methods
  getStatusColor(isActive: boolean): string {
    return this.privilegeService.getStatusColor(isActive);
  }

  getStatusLabel(isActive: boolean): string {
    return this.privilegeService.getStatusLabel(isActive);
  }

  getCategoryColor(category: string): string {
    return this.privilegeService.getCategoryColor(category);
  }

  formatDate(date: string | Date): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  // Export functionality
  exportPrivileges(): void {
    const params = {
      search: this.searchTerm,
      category: this.selectedCategory,
      status: this.selectedStatus
    };

    this.privilegeService.exportPrivileges(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: Blob) => {
          const blob = new Blob([response], { type: 'text/csv' });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `privileges-${new Date().toISOString().split('T')[0]}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.toastService.showSuccess('Privileges exported successfully');
        },
        error: (error) => {
          this.toastService.showError('Failed to export privileges');
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
