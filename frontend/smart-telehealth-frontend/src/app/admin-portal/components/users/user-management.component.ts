import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MatSnackBarModule],
  template: `
    <div class="user-management-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>User Management</h1>
          <p>Manage all users, their roles, and access permissions</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="openCreateUserModal()"
            [disabled]="loading">
            <i class="fas fa-plus"></i> Create User
          </button>
          <button 
            class="btn btn-secondary" 
            (click)="exportUsers()"
            [disabled]="loading || users.length === 0">
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
                placeholder="Search by name, email, or ID..."
                class="form-control">
            </div>
            <div class="filter-group">
              <label for="role">Role</label>
              <select id="role" formControlName="role" class="form-control">
                <option value="">All Roles</option>
                <option value="User">User</option>
                <option value="Provider">Provider</option>
                <option value="Admin">Admin</option>
              </select>
            </div>
            <div class="filter-group">
              <label for="status">Status</label>
              <select id="status" formControlName="status" class="form-control">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
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

      <!-- Users Table -->
      <div class="table-container">
        <table class="users-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Created Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of users">
              <td>
                <div class="user-info">
                  <div class="user-avatar">
                    <i class="fas fa-user"></i>
                  </div>
                  <div class="user-details">
                    <div class="user-name">{{ user.firstName }} {{ user.lastName }}</div>
                    <div class="user-id">ID: {{ user.id }}</div>
                  </div>
                </div>
              </td>
              <td>{{ user.email }}</td>
              <td>
                <span class="role-badge role-{{ user.role?.toLowerCase() }}">
                  {{ user.role || 'N/A' }}
                </span>
              </td>
              <td>
                <span class="status-badge status-{{ user.isActive ? 'active' : 'inactive' }}">
                  {{ user.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>{{ user.createdAt | date:'MMM dd, yyyy' }}</td>
              <td>
                <div class="action-buttons">
                  <button class="btn btn-sm btn-outline" (click)="viewUser(user.id)" title="View">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="editUser(user.id)" title="Edit">
                    <i class="fas fa-edit"></i>
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="toggleUserStatus(user.id)" title="Toggle Status">
                    <i class="fas fa-toggle-on" *ngIf="user.isActive"></i>
                    <i class="fas fa-toggle-off" *ngIf="!user.isActive"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Loading State -->
        <div class="loading-state" *ngIf="loading">
          <div class="spinner"></div>
          <p>Loading users...</p>
        </div>

        <!-- Empty State -->
        <div class="empty-state" *ngIf="!loading && users.length === 0">
          <i class="fas fa-users"></i>
          <h3>No users found</h3>
          <p>Try adjusting your filters or create a new user to get started.</p>
          <button class="btn btn-primary" (click)="openCreateUserModal()">
            Create First User
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
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit, OnDestroy {
  // Data properties
  users: any[] = [];
  
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
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Form initialization
  private initializeFilterForm(): void {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      role: [''],
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
        this.loadUsers();
      });

    this.filterForm.get('role')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });

    this.filterForm.get('status')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });
  }

  // Load users with filters
  loadUsers(): void {
    this.loading = true;
    this.error = null;
    
    // TODO: Implement actual API call
    // For now, using mock data
    setTimeout(() => {
      this.users = [
        {
          id: 1,
          firstName: 'John',
          lastName: 'Doe',
          email: 'john.doe@example.com',
          role: 'User',
          isActive: true,
          createdAt: new Date('2024-01-15')
        },
        {
          id: 2,
          firstName: 'Jane',
          lastName: 'Smith',
          email: 'jane.smith@example.com',
          role: 'Provider',
          isActive: true,
          createdAt: new Date('2024-01-20')
        }
      ];
      this.totalCount = this.users.length;
      this.totalPages = Math.ceil(this.totalCount / this.pageSize);
      this.loading = false;
    }, 1000);
  }

  // Clear all filters
  clearFilters(): void {
    this.filterForm.reset();
    this.currentPage = 1;
    this.loadUsers();
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadUsers();
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
  openCreateUserModal(): void {
    // TODO: Implement create user modal
    console.log('Open create user modal');
    this.showSuccessMessage('Create user modal opened');
  }

  viewUser(id: number): void {
    // TODO: Implement view user
    console.log('View user:', id);
  }

  editUser(id: number): void {
    // TODO: Implement edit user
    console.log('Edit user:', id);
  }

  toggleUserStatus(id: number): void {
    // TODO: Implement toggle user status
    console.log('Toggle user status:', id);
    this.showSuccessMessage('User status updated');
  }

  exportUsers(): void {
    // TODO: Implement export functionality
    console.log('Export users');
    this.showSuccessMessage('User export started');
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
