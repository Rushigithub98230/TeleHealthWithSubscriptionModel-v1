import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-provider-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MatSnackBarModule],
  template: `
    <div class="provider-management-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Provider Management</h1>
          <p>Manage healthcare providers, their specialties, and availability</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="openCreateProviderModal()"
            [disabled]="loading">
            <i class="fas fa-plus"></i> Add Provider
          </button>
          <button 
            class="btn btn-secondary" 
            (click)="exportProviders()"
            [disabled]="loading || providers.length === 0">
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
                formControlName="searchTerm"
                placeholder="Search by name, specialty, or ID..."
                class="form-control">
            </div>
            <div class="filter-group">
              <label for="specialty">Specialty</label>
              <select id="specialty" formControlName="specialty" class="form-control">
                <option value="">All Specialties</option>
                <option value="Cardiology">Cardiology</option>
                <option value="Dermatology">Dermatology</option>
                <option value="Endocrinology">Endocrinology</option>
                <option value="General Practice">General Practice</option>
                <option value="Neurology">Neurology</option>
                <option value="Oncology">Oncology</option>
                <option value="Pediatrics">Pediatrics</option>
                <option value="Psychiatry">Psychiatry</option>
              </select>
            </div>
            <div class="filter-group">
              <label for="status">Status</label>
              <select id="status" formControlName="status" class="form-control">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="On Leave">On Leave</option>
                <option value="Suspended">Suspended</option>
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

      <!-- Providers Table -->
      <div class="table-container">
        <table class="providers-table">
          <thead>
            <tr>
              <th>Provider</th>
              <th>Specialty</th>
              <th>Contact</th>
              <th>Status</th>
              <th>Availability</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let provider of providers">
              <td>
                <div class="provider-info">
                  <div class="provider-avatar">
                    <i class="fas fa-user-md"></i>
                  </div>
                  <div class="provider-details">
                    <div class="provider-name">Dr. {{ provider.firstName }} {{ provider.lastName }}</div>
                    <div class="provider-id">ID: {{ provider.id }}</div>
                    <div class="provider-license">License: {{ provider.licenseNumber }}</div>
                  </div>
                </div>
              </td>
              <td>
                <span class="specialty-badge">{{ provider.specialty }}</span>
              </td>
              <td>
                <div class="contact-info">
                  <div class="contact-email">{{ provider.email }}</div>
                  <div class="contact-phone">{{ provider.phoneNumber || 'N/A' }}</div>
                </div>
              </td>
              <td>
                <span class="status-badge status-{{ provider.isActive ? 'active' : 'inactive' }}">
                  {{ provider.status || (provider.isActive ? 'Active' : 'Inactive') }}
                </span>
              </td>
              <td>
                <div class="availability-info">
                  <span class="availability-status" [class]="provider.isAvailable ? 'available' : 'unavailable'">
                    {{ provider.isAvailable ? 'Available' : 'Unavailable' }}
                  </span>
                  <div class="next-available" *ngIf="provider.nextAvailableSlot">
                    Next: {{ provider.nextAvailableSlot | date:'MMM dd, HH:mm' }}
                  </div>
                </div>
              </td>
              <td>
                <div class="action-buttons">
                  <button class="btn btn-sm btn-outline" (click)="viewProvider(provider.id)" title="View">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="editProvider(provider.id)" title="Edit">
                    <i class="fas fa-edit"></i>
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="toggleProviderStatus(provider.id)" title="Toggle Status">
                    <i class="fas fa-toggle-on" *ngIf="provider.isActive"></i>
                    <i class="fas fa-toggle-off" *ngIf="!provider.isActive"></i>
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="manageSchedule(provider.id)" title="Manage Schedule">
                    <i class="fas fa-calendar"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Loading State -->
        <div class="loading-state" *ngIf="loading">
          <div class="spinner"></div>
          <p>Loading providers...</p>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!loading && providers.length === 0">
          <i class="fas fa-user-md"></i>
          <h3>No providers found</h3>
          <p>Try adjusting your filters or add a new provider to get started.</p>
          <button class="btn btn-primary" (click)="openCreateProviderModal()">
            Add First Provider
          </button>
        </div>
      </div>

      <!-- Pagination -->
      <div class="pagination" *ngIf="totalPages > 1">
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
          [disabled]="currentPage === totalPages"
          (click)="goToPage(currentPage + 1)">
          Next <i class="fas fa-chevron-right"></i>
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./provider-management.component.scss']
})
export class ProviderManagementComponent implements OnInit, OnDestroy {
  // Data properties
  providers: any[] = [];
  
  // Form and filter properties
  filterForm!: FormGroup;
  
  // Pagination properties
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  
  // State properties
  loading = false;
  error: string | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.initializeFilterForm();
  }

  ngOnInit(): void {
    this.setupFilterSubscriptions();
    this.loadProviders();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Form initialization
  private initializeFilterForm(): void {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      specialty: [''],
      status: ['']
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
        this.loadProviders();
      });

    this.filterForm.get('specialty')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadProviders();
      });

    this.filterForm.get('status')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadProviders();
      });
  }

  // Load providers with filters
  loadProviders(): void {
    this.loading = true;
    this.error = null;
    
    // TODO: Implement actual API call
    // For now, using mock data
    setTimeout(() => {
      this.providers = [
        {
          id: 1,
          firstName: 'John',
          lastName: 'Smith',
          email: 'john.smith@healthcare.com',
          phoneNumber: '+1-555-0123',
          specialty: 'Cardiology',
          licenseNumber: 'MD12345',
          isActive: true,
          status: 'Active',
          isAvailable: true,
          nextAvailableSlot: new Date(Date.now() + 2 * 60 * 60 * 1000) // 2 hours from now
        },
        {
          id: 2,
          firstName: 'Sarah',
          lastName: 'Johnson',
          email: 'sarah.johnson@healthcare.com',
          phoneNumber: '+1-555-0124',
          specialty: 'Dermatology',
          licenseNumber: 'MD12346',
          isActive: true,
          status: 'Active',
          isAvailable: false,
          nextAvailableSlot: new Date(Date.now() + 24 * 60 * 60 * 1000) // 24 hours from now
        }
      ];
      this.totalCount = this.providers.length;
      this.totalPages = Math.ceil(this.totalCount / this.pageSize);
      this.loading = false;
    }, 1000);
  }

  // Clear all filters
  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadProviders();
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProviders();
    }
  }

  getPageNumbers(): (number | string)[] {
    const pages: (number | string)[] = [];
    const totalPages = this.totalPages;
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

  // Action methods
  openCreateProviderModal(): void {
    // TODO: Implement create provider modal
    console.log('Open create provider modal');
    this.showSuccessMessage('Create provider modal opened');
  }

  viewProvider(id: number): void {
    // TODO: Implement view provider
    console.log('View provider:', id);
  }

  editProvider(id: number): void {
    // TODO: Implement edit provider
    console.log('Edit provider:', id);
  }

  toggleProviderStatus(id: number): void {
    // TODO: Implement toggle provider status
    console.log('Toggle provider status:', id);
    this.showSuccessMessage('Provider status updated');
  }

  manageSchedule(id: number): void {
    // TODO: Implement manage schedule
    console.log('Manage schedule for provider:', id);
    this.showSuccessMessage('Schedule management opened');
  }

  exportProviders(): void {
    // TODO: Implement export functionality
    console.log('Export providers');
    this.showSuccessMessage('Provider export started');
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
