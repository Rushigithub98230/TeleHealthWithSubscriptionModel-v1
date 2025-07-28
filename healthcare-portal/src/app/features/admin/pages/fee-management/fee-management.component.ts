import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ProviderFeeService, ProviderFee, ReviewProviderFeeDto } from '../../../../core/services/provider-fee.service';

@Component({
  selector: 'app-fee-management',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTabsModule,
    MatExpansionModule,
    MatBadgeModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatMenuModule,
    MatSlideToggleModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './fee-management.component.html',
  styleUrl: './fee-management.component.scss'
})
export class FeeManagementComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Data
  fees: ProviderFee[] = [];
  filteredFees: ProviderFee[] = [];
  dataSource = new MatTableDataSource<ProviderFee>();

  // Loading states
  loading = false;
  loadingStats = false;

  // Statistics
  stats = {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0,
    averageProposedFee: 0,
    averageApprovedFee: 0,
    totalProviders: 0,
    totalCategories: 0
  };

  // Filters
  selectedStatus = 'all';
  selectedCategory = 'all';
  searchTerm = '';
  showOnlyPending = false;

  // Table columns
  displayedColumns = ['provider', 'category', 'proposedFee', 'approvedFee', 'status', 'proposedAt', 'actions'];

  // Review form
  reviewForm: FormGroup;
  selectedFee: ProviderFee | null = null;
  showReviewDialog = false;

  constructor(
    private feeService: ProviderFeeService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {
    this.reviewForm = this.fb.group({
      status: ['', Validators.required],
      approvedFee: [0, [Validators.required, Validators.min(0)]],
      adminRemarks: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit() {
    this.loadFees();
    this.loadStatistics();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadFees() {
    this.loading = true;
    this.feeService.getAllFees().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.fees = response.data;
          this.filteredFees = response.data;
          this.dataSource.data = response.data;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading fees:', error);
        this.loading = false;
        this.showNotification('Error loading fees', 'error');
      }
    });
  }

  loadStatistics() {
    this.loadingStats = true;
    this.feeService.getFeeStatistics().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.stats = {
            total: response.data.totalFees,
            pending: response.data.pendingFees,
            approved: response.data.approvedFees,
            rejected: response.data.rejectedFees,
            averageProposedFee: response.data.averageProposedFee,
            averageApprovedFee: response.data.averageApprovedFee,
            totalProviders: response.data.totalProviders,
            totalCategories: response.data.totalCategories
          };
        }
        this.loadingStats = false;
      },
      error: (error: any) => {
        console.error('Error loading statistics:', error);
        this.loadingStats = false;
      }
    });
  }

  filterByStatus(status: string) {
    this.selectedStatus = status;
    this.applyFilters();
  }

  filterByCategory(category: string) {
    this.selectedCategory = category;
    this.applyFilters();
  }

  togglePendingOnly() {
    this.showOnlyPending = !this.showOnlyPending;
    this.applyFilters();
  }

  applyFilters() {
    let filtered = this.fees;

    // Filter by status
    if (this.selectedStatus !== 'all') {
      filtered = filtered.filter(item => item.status.toLowerCase() === this.selectedStatus.toLowerCase());
    }

    // Filter by category
    if (this.selectedCategory !== 'all') {
      filtered = filtered.filter(item => item.categoryName.toLowerCase() === this.selectedCategory.toLowerCase());
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(item => 
        item.providerName.toLowerCase().includes(search) ||
        item.categoryName.toLowerCase().includes(search) ||
        item.providerNotes?.toLowerCase().includes(search) ||
        item.adminRemarks?.toLowerCase().includes(search)
      );
    }

    // Filter by pending only
    if (this.showOnlyPending) {
      filtered = filtered.filter(item => item.status.toLowerCase() === 'pending');
    }

    this.filteredFees = filtered;
    this.dataSource.data = filtered;
  }

  openReviewDialog(fee: ProviderFee) {
    this.selectedFee = fee;
    this.reviewForm.patchValue({
      status: 'Approved',
      approvedFee: fee.proposedFee,
      adminRemarks: ''
    });
    this.showReviewDialog = true;
  }

  closeReviewDialog() {
    this.showReviewDialog = false;
    this.selectedFee = null;
    this.reviewForm.reset();
  }

  submitReview() {
    if (this.reviewForm.valid && this.selectedFee) {
      const reviewDto: ReviewProviderFeeDto = {
        status: this.reviewForm.value.status,
        approvedFee: this.reviewForm.value.approvedFee,
        adminRemarks: this.reviewForm.value.adminRemarks
      };

      this.feeService.reviewFee(this.selectedFee.id, reviewDto).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.showNotification(`Fee ${reviewDto.status.toLowerCase()} successfully`, 'success');
            this.closeReviewDialog();
            this.loadFees();
            this.loadStatistics();
          }
        },
        error: (error: any) => {
          console.error('Error reviewing fee:', error);
          this.showNotification('Error reviewing fee', 'error');
        }
      });
    }
  }

  quickApprove(fee: ProviderFee) {
    const reviewDto: ReviewProviderFeeDto = {
      status: 'Approved',
      approvedFee: fee.proposedFee,
      adminRemarks: 'Quick approved'
    };

    this.feeService.reviewFee(fee.id, reviewDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Fee approved successfully', 'success');
          this.loadFees();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error approving fee:', error);
        this.showNotification('Error approving fee', 'error');
      }
    });
  }

  quickReject(fee: ProviderFee) {
    const reviewDto: ReviewProviderFeeDto = {
      status: 'Rejected',
      adminRemarks: 'Quick rejected'
    };

    this.feeService.reviewFee(fee.id, reviewDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Fee rejected successfully', 'success');
          this.loadFees();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error rejecting fee:', error);
        this.showNotification('Error rejecting fee', 'error');
      }
    });
  }

  viewFeeDetails(fee: ProviderFee) {
    // Navigate to detailed view or open dialog
    console.log('View fee details:', fee);
    // this.router.navigate(['/admin/fee-management', fee.id]);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'warn';
      case 'approved':
        return 'primary';
      case 'rejected':
        return 'accent';
      default:
        return 'primary';
    }
  }

  getStatusText(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getCategories(): string[] {
    const categories = [...new Set(this.fees.map(fee => fee.categoryName))];
    return categories.sort();
  }

  getProviders(): string[] {
    const providers = [...new Set(this.fees.map(fee => fee.providerName))];
    return providers.sort();
  }

  showNotification(message: string, type: 'success' | 'error' | 'info' = 'info') {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: type === 'error' ? 'error-snackbar' : type === 'success' ? 'success-snackbar' : 'info-snackbar'
    });
  }

  exportToCsv() {
    const headers = ['Provider', 'Category', 'Proposed Fee', 'Approved Fee', 'Status', 'Proposed Date', 'Reviewed Date'];
    const csvData = this.filteredFees.map(item => [
      item.providerName,
      item.categoryName,
      `$${item.proposedFee}`,
      item.approvedFee ? `$${item.approvedFee}` : 'N/A',
      item.status,
      new Date(item.proposedAt || item.createdAt).toLocaleDateString(),
      item.reviewedAt ? new Date(item.reviewedAt).toLocaleDateString() : 'N/A'
    ]);

    const csvContent = [headers, ...csvData]
      .map(row => row.map(cell => `"${cell}"`).join(','))
      .join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `provider-fees-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  getAverageFeeDifference(): number {
    if (this.stats.averageProposedFee && this.stats.averageApprovedFee) {
      return this.stats.averageProposedFee - this.stats.averageApprovedFee;
    }
    return 0;
  }

  getApprovalRate(): number {
    if (this.stats.total > 0) {
      return (this.stats.approved / this.stats.total) * 100;
    }
    return 0;
  }
} 