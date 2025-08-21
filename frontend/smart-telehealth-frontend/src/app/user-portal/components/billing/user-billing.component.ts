import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

import { UserBillingService } from '../../services/user-billing.service';
import { UserSubscriptionService } from '../../services/user-subscription.service';
import { AuthService } from '../../../core/services/auth.service';
import { BillingRecord, BillingStatus, BillingType } from '../../models/billing.interface';
import { Subscription } from '../../models/subscription.interface';

@Component({
  selector: 'app-user-billing',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule
  ],
  template: `
    <div class="user-billing-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Billing & Payments</h1>
          <p>View your billing history and manage payments</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="navigateToPaymentMethods()"
            [disabled]="loading">
            <i class="fas fa-credit-card"></i> Payment Methods
          </button>
          <button 
            class="btn btn-secondary" 
            (click)="downloadInvoice()"
            [disabled]="loading || selectedBillingRecord === null">
            <i class="fas fa-download"></i> Download Invoice
          </button>
        </div>
      </div>

      <!-- Billing Summary -->
      <div class="billing-summary" *ngIf="!loading">
        <div class="summary-cards">
          <div class="summary-card">
            <div class="card-icon">
              <i class="fas fa-dollar-sign"></i>
            </div>
            <div class="card-content">
              <h3>Total Spent</h3>
              <span class="amount">{{ getTotalSpent() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="period">This month</span>
            </div>
          </div>
          
          <div class="summary-card">
            <div class="card-icon">
              <i class="fas fa-clock"></i>
            </div>
            <div class="card-content">
              <h3>Pending</h3>
              <span class="amount">{{ getPendingAmount() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="period">Awaiting payment</span>
            </div>
          </div>
          
          <div class="summary-card">
            <div class="card-icon">
              <i class="fas fa-check-circle"></i>
            </div>
            <div class="card-content">
              <h3>Paid</h3>
              <span class="amount">{{ getPaidAmount() | currency:'USD':'symbol':'1.2-2' }}</span>
              <span class="period">This month</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="filters-section">
        <form [formGroup]="filterForm" class="filter-form">
          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>Search</mat-label>
              <input matInput formControlName="searchTerm" placeholder="Search invoices, descriptions...">
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>Status</mat-label>
              <mat-select formControlName="status">
                <mat-option value="">All Statuses</mat-option>
                <mat-option value="Paid">Paid</mat-option>
                <mat-option value="Pending">Pending</mat-option>
                <mat-option value="Failed">Failed</mat-option>
                <mat-option value="Overdue">Overdue</mat-option>
              </mat-select>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>Type</mat-label>
              <mat-select formControlName="type">
                <mat-option value="">All Types</mat-option>
                <mat-option value="Subscription">Subscription</mat-option>
                <mat-option value="Consultation">Consultation</mat-option>
                <mat-option value="Medication">Medication</mat-option>
                <mat-option value="LateFee">Late Fee</mat-option>
              </mat-select>
            </mat-form-field>
            
            <button 
              type="button" 
              class="btn btn-outline" 
              (click)="clearFilters()">
              Clear Filters
            </button>
          </div>
        </form>
      </div>

      <!-- Loading State -->
      <div class="loading-section" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Loading billing records...</p>
      </div>

      <!-- Billing Records Table -->
      <div class="billing-table-section" *ngIf="!loading">
        <div class="table-container">
          <table mat-table [dataSource]="filteredBillingRecords" matSort class="billing-table">
            <!-- Date Column -->
            <ng-container matColumnDef="billingDate">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Date </th>
              <td mat-cell *matCellDef="let record"> {{ record.billingDate | date:'mediumDate' }} </td>
            </ng-container>

            <!-- Description Column -->
            <ng-container matColumnDef="description">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Description </th>
              <td mat-cell *matCellDef="let record"> {{ record.description }} </td>
            </ng-container>

            <!-- Amount Column -->
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Amount </th>
              <td mat-cell *matCellDef="let record"> {{ record.amount | currency:'USD':'symbol':'1.2-2' }} </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Status </th>
              <td mat-cell *matCellDef="let record">
                <span class="status-badge" [class]="'status-' + record.status.toLowerCase()">
                  {{ record.status }}
                </span>
              </td>
            </ng-container>

            <!-- Type Column -->
            <ng-container matColumnDef="type">
              <th mat-header-cell *matHeaderCellDef mat-sort-header> Type </th>
              <td mat-cell *matCellDef="let record"> {{ record.type }} </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef> Actions </th>
              <td mat-cell *matCellDef="let record">
                <button 
                  mat-icon-button 
                  (click)="viewDetails(record.id)"
                  matTooltip="View Details">
                  <mat-icon>visibility</mat-icon>
                </button>
                <button 
                  mat-icon-button 
                  (click)="downloadInvoice(record.id)"
                  matTooltip="Download Invoice"
                  *ngIf="record.status === 'Paid'">
                  <mat-icon>download</mat-icon>
                </button>
                <button 
                  mat-icon-button 
                  (click)="payNow(record.id)"
                  matTooltip="Pay Now"
                  *ngIf="record.status === 'Pending' || record.status === 'Overdue'">
                  <mat-icon>payment</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr 
              mat-row 
              *matRowDef="let row; columns: displayedColumns;"
              (click)="selectBillingRecord(row)"
              [class.selected]="selectedBillingRecord?.id === row.id"
              class="billing-row">
            </tr>
          </table>

          <mat-paginator 
            [pageSizeOptions]="[5, 10, 25, 50]"
            showFirstLastButtons>
          </mat-paginator>
        </div>
      </div>

      <!-- No Records State -->
      <div class="no-records" *ngIf="!loading && filteredBillingRecords.length === 0">
        <div class="empty-state">
          <i class="fas fa-receipt"></i>
          <h3>No Billing Records Found</h3>
          <p>You don't have any billing records yet.</p>
        </div>
      </div>

      <!-- Billing Details Sidebar -->
      <div class="billing-details-sidebar" *ngIf="selectedBillingRecord">
        <div class="sidebar-header">
          <h3>Billing Details</h3>
          <button mat-icon-button (click)="closeDetails()">
            <mat-icon>close</mat-icon>
          </button>
        </div>
        
        <div class="sidebar-content">
          <div class="detail-section">
            <h4>Invoice Information</h4>
            <div class="detail-row">
              <span class="label">Invoice Number:</span>
              <span class="value">{{ selectedBillingRecord.invoiceNumber }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Date:</span>
              <span class="value">{{ selectedBillingRecord.billingDate | date:'mediumDate' }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Due Date:</span>
              <span class="value">{{ selectedBillingRecord.dueDate | date:'mediumDate' }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Status:</span>
              <span class="value status-badge" [class]="'status-' + selectedBillingRecord.status.toLowerCase()">
                {{ selectedBillingRecord.status }}
              </span>
            </div>
          </div>

          <div class="detail-section">
            <h4>Payment Details</h4>
            <div class="detail-row">
              <span class="label">Amount:</span>
              <span class="value amount">{{ selectedBillingRecord.amount | currency:'USD':'symbol':'1.2-2' }}</span>
            </div>
            <div class="detail-row" *ngIf="selectedBillingRecord.taxAmount">
              <span class="label">Tax:</span>
              <span class="value">{{ selectedBillingRecord.taxAmount | currency:'USD':'symbol':'1.2-2' }}</span>
            </div>
            <div class="detail-row" *ngIf="selectedBillingRecord.shippingAmount">
              <span class="label">Shipping:</span>
              <span class="value">{{ selectedBillingRecord.shippingAmount | currency:'USD':'symbol':'1.2-2' }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Total:</span>
              <span class="value total-amount">{{ selectedBillingRecord.totalAmount | currency:'USD':'symbol':'1.2-2' }}</span>
            </div>
          </div>

          <div class="detail-section" *ngIf="selectedBillingRecord.description">
            <h4>Description</h4>
            <p>{{ selectedBillingRecord.description }}</p>
          </div>

          <div class="detail-section" *ngIf="selectedBillingRecord.subscription">
            <h4>Related Subscription</h4>
            <div class="subscription-info">
              <span class="plan-name">{{ selectedBillingRecord.subscription.subscriptionPlan?.name }}</span>
              <span class="plan-price">{{ selectedBillingRecord.subscription.currentPrice | currency:'USD':'symbol':'1.2-2' }}</span>
            </div>
          </div>

          <div class="actions-section">
            <button 
              class="btn btn-primary full-width" 
              (click)="payNow(selectedBillingRecord.id)"
              *ngIf="selectedBillingRecord.status === 'Pending' || selectedBillingRecord.status === 'Overdue'">
              <i class="fas fa-credit-card"></i> Pay Now
            </button>
            <button 
              class="btn btn-outline full-width" 
              (click)="downloadInvoice(selectedBillingRecord.id)"
              *ngIf="selectedBillingRecord.status === 'Paid'">
              <i class="fas fa-download"></i> Download Invoice
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./user-billing.component.scss']
})
export class UserBillingComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  billingRecords: BillingRecord[] = [];
  filteredBillingRecords: BillingRecord[] = [];
  selectedBillingRecord: BillingRecord | null = null;
  loading = false;
  error: string | null = null;

  // Table
  displayedColumns: string[] = ['billingDate', 'description', 'amount', 'status', 'type', 'actions'];

  // Forms
  filterForm: FormGroup;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;

  constructor(
    private fb: FormBuilder,
    private userBillingService: UserBillingService,
    private userSubscriptionService: UserSubscriptionService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.fb.group({
      searchTerm: [''],
      status: [''],
      type: [''],
      dateFrom: [''],
      dateTo: ['']
    });
  }

  ngOnInit(): void {
    this.loadBillingRecords();
    this.setupFilterListener();
    
    // Check if we have a subscription ID in the route
    this.route.params.subscribe(params => {
      if (params['subscriptionId']) {
        this.loadBillingRecordsForSubscription(params['subscriptionId']);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setupFilterListener(): void {
    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.applyFilters();
      });
  }

  loadBillingRecords(): void {
    this.loading = true;
    this.error = null;

    this.userBillingService.getUserBillingRecords()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.billingRecords = response.data;
            this.filteredBillingRecords = [...this.billingRecords];
            this.totalItems = this.billingRecords.length;
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = error.message || 'Failed to load billing records';
          this.loading = false;
          this.showNotification('Error loading billing records', 'error');
        }
      });
  }

  loadBillingRecordsForSubscription(subscriptionId: string): void {
    this.loading = true;
    this.error = null;

    this.userBillingService.getBillingRecordsForSubscription(subscriptionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.billingRecords = response.data;
            this.filteredBillingRecords = [...this.billingRecords];
            this.totalItems = this.billingRecords.length;
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = error.message || 'Failed to load billing records';
          this.loading = false;
          this.showNotification('Error loading billing records', 'error');
        }
      });
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    
    this.filteredBillingRecords = this.billingRecords.filter(record => {
      let matches = true;
      
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase();
        matches = matches && (
          record.description?.toLowerCase().includes(searchLower) ||
          record.invoiceNumber?.toLowerCase().includes(searchLower)
        );
      }
      
      if (filters.status) {
        matches = matches && record.status === filters.status;
      }
      
      if (filters.type) {
        matches = matches && record.type === filters.type;
      }
      
      return matches;
    });
    
    this.totalItems = this.filteredBillingRecords.length;
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.filteredBillingRecords = [...this.billingRecords];
    this.totalItems = this.billingRecords.length;
  }

  selectBillingRecord(record: BillingRecord): void {
    this.selectedBillingRecord = record;
  }

  closeDetails(): void {
    this.selectedBillingRecord = null;
  }

  viewDetails(recordId: string): void {
    const record = this.billingRecords.find(r => r.id === recordId);
    if (record) {
      this.selectBillingRecord(record);
    }
  }

  payNow(recordId: string): void {
    this.router.navigate(['/web/billing', recordId]);
  }

  downloadInvoice(recordId?: string): void {
    const id = recordId || this.selectedBillingRecord?.id;
    if (id) {
      this.userBillingService.downloadInvoice(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (blob) => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `invoice-${id}.pdf`;
            a.click();
            window.URL.revokeObjectURL(url);
          },
          error: (error) => {
            this.showNotification('Failed to download invoice', 'error');
          }
        });
    }
  }

  navigateToPaymentMethods(): void {
    this.router.navigate(['/web/payment-methods']);
  }

  getTotalSpent(): number {
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();
    
    return this.billingRecords
      .filter(record => {
        const recordDate = new Date(record.billingDate);
        return record.status === 'Paid' && 
               recordDate.getMonth() === currentMonth && 
               recordDate.getFullYear() === currentYear;
      })
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  getPendingAmount(): number {
    return this.billingRecords
      .filter(record => record.status === 'Pending' || record.status === 'Overdue')
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  getPaidAmount(): number {
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();
    
    return this.billingRecords
      .filter(record => {
        const recordDate = new Date(record.billingDate);
        return record.status === 'Paid' && 
               recordDate.getMonth() === currentMonth && 
               recordDate.getFullYear() === currentYear;
      })
      .reduce((total, record) => total + (record.totalAmount || 0), 0);
  }

  showNotification(message: string, type: 'success' | 'error' = 'success'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'success-snackbar' : 'error-snackbar'
    });
  }
}
