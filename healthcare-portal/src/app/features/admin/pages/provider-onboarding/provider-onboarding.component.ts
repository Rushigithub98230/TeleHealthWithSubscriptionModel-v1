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
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ProviderOnboardingService, ProviderOnboarding } from '../../../../core/services/provider-onboarding.service';

@Component({
  selector: 'app-provider-onboarding',
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
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './provider-onboarding.component.html',
  styleUrl: './provider-onboarding.component.scss'
})
export class ProviderOnboardingComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Data
  onboardings: ProviderOnboarding[] = [];
  filteredOnboardings: ProviderOnboarding[] = [];
  dataSource = new MatTableDataSource<ProviderOnboarding>();

  // Loading states
  loading = false;
  loadingStats = false;

  // Statistics
  stats = {
    total: 0,
    pending: 0,
    underReview: 0,
    approved: 0,
    rejected: 0,
    requiresMoreInfo: 0
  };

  // Filters
  selectedStatus = 'all';
  searchTerm = '';

  // Table columns
  displayedColumns = ['name', 'specialty', 'email', 'licenseNumber', 'status', 'createdAt', 'actions'];

  constructor(
    private onboardingService: ProviderOnboardingService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadOnboardings();
    this.loadStatistics();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadOnboardings() {
    this.loading = true;
    this.onboardingService.getAllOnboardings().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.onboardings = response.data;
          this.filteredOnboardings = response.data;
          this.dataSource.data = response.data;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading onboardings:', error);
        this.loading = false;
        this.showNotification('Error loading onboardings', 'error');
      }
    });
  }

  loadStatistics() {
    this.loadingStats = true;
    this.onboardingService.getOnboardingStatistics().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.stats = {
            total: response.data.totalOnboardings,
            pending: response.data.pendingOnboardings,
            underReview: response.data.underReviewOnboardings,
            approved: response.data.approvedOnboardings,
            rejected: response.data.rejectedOnboardings,
            requiresMoreInfo: response.data.requiresMoreInfoOnboardings || 0
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

  applyFilters() {
    let filtered = this.onboardings;

    // Filter by status
    if (this.selectedStatus !== 'all') {
      filtered = filtered.filter(item => item.status.toLowerCase() === this.selectedStatus.toLowerCase());
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(item => 
        item.firstName.toLowerCase().includes(search) ||
        item.lastName.toLowerCase().includes(search) ||
        item.email.toLowerCase().includes(search) ||
        item.specialty.toLowerCase().includes(search) ||
        item.licenseNumber.toLowerCase().includes(search)
      );
    }

    this.filteredOnboardings = filtered;
    this.dataSource.data = filtered;
  }

  approveOnboarding(id: string) {
    const reviewDto = {
      status: 'Approved',
      adminRemarks: 'Application approved after review'
    };
    
    this.onboardingService.reviewOnboarding(id, reviewDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Onboarding application approved', 'success');
          this.loadOnboardings();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error approving onboarding:', error);
        this.showNotification('Error approving application', 'error');
      }
    });
  }

  rejectOnboarding(id: string) {
    const reviewDto = {
      status: 'Rejected',
      adminRemarks: 'Application rejected after review'
    };
    
    this.onboardingService.reviewOnboarding(id, reviewDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Onboarding application rejected', 'success');
          this.loadOnboardings();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error rejecting onboarding:', error);
        this.showNotification('Error rejecting application', 'error');
      }
    });
  }

  requestMoreInfo(id: string) {
    const reviewDto = {
      status: 'RequiresMoreInfo',
      adminRemarks: 'Additional information required'
    };
    
    this.onboardingService.reviewOnboarding(id, reviewDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('More information requested', 'success');
          this.loadOnboardings();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error requesting more info:', error);
        this.showNotification('Error requesting more information', 'error');
      }
    });
  }

  viewOnboardingDetails(id: string) {
    // Navigate to detailed view or open dialog
    console.log('View onboarding details:', id);
    // this.router.navigate(['/admin/provider-onboarding', id]);
  }

  downloadDocument(url: string, filename: string) {
    if (url) {
      const link = document.createElement('a');
      link.href = url;
      link.download = filename;
      link.click();
    }
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'warn';
      case 'underreview':
        return 'accent';
      case 'approved':
        return 'primary';
      case 'rejected':
        return 'accent';
      case 'requiresmoreinfo':
        return 'warn';
      default:
        return 'primary';
    }
  }

  getStatusText(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
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
    const headers = ['Name', 'Email', 'Specialty', 'License Number', 'Status', 'Created Date'];
    const csvData = this.filteredOnboardings.map(item => [
      `${item.firstName} ${item.lastName}`,
      item.email,
      item.specialty,
      item.licenseNumber,
      item.status,
      new Date(item.createdAt).toLocaleDateString()
    ]);

    const csvContent = [headers, ...csvData]
      .map(row => row.map(cell => `"${cell}"`).join(','))
      .join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `provider-onboardings-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }
} 