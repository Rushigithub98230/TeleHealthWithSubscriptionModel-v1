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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ProviderPayoutService, ProviderPayout, ProcessPayoutDto, PayoutPeriodDto, CreatePayoutPeriodDto } from '../../../../core/services/provider-payout.service';

@Component({
  selector: 'app-payout-management',
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
    MatDatepickerModule,
    MatNativeDateModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './payout-management.component.html',
  styleUrl: './payout-management.component.scss'
})
export class PayoutManagementComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Data
  payouts: ProviderPayout[] = [];
  filteredPayouts: ProviderPayout[] = [];
  dataSource = new MatTableDataSource<ProviderPayout>();
  periods: PayoutPeriodDto[] = [];

  // Loading states
  loading = false;
  loadingStats = false;
  loadingPeriods = false;

  // Statistics
  stats = {
    total: 0,
    pending: 0,
    processed: 0,
    totalAmount: 0,
    averagePayoutAmount: 0,
    totalProviders: 0,
    totalPeriods: 0
  };

  // Filters
  selectedStatus = 'all';
  selectedPeriod = 'all';
  searchTerm = '';
  showOnlyPending = false;

  // Table columns
  displayedColumns = ['provider', 'period', 'totalAmount', 'providerAmount', 'status', 'createdAt', 'actions'];

  // Process form
  processForm: FormGroup;
  selectedPayout: ProviderPayout | null = null;
  showProcessDialog = false;

  // Period form
  periodForm: FormGroup;
  showPeriodDialog = false;

  constructor(
    private payoutService: ProviderPayoutService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private fb: FormBuilder
  ) {
    this.processForm = this.fb.group({
      status: ['', Validators.required],
      paymentReference: ['', Validators.required],
      notes: ['', Validators.maxLength(500)]
    });

    this.periodForm = this.fb.group({
      name: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadPayouts();
    this.loadStatistics();
    this.loadPeriods();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadPayouts() {
    this.loading = true;
    this.payoutService.getAllPayouts().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.payouts = response.data;
          this.filteredPayouts = response.data;
          this.dataSource.data = response.data;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading payouts:', error);
        this.loading = false;
        this.showNotification('Error loading payouts', 'error');
      }
    });
  }

  loadStatistics() {
    this.loadingStats = true;
    this.payoutService.getPayoutStatistics().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.stats = {
            total: response.data.totalPayouts,
            pending: response.data.pendingPayouts,
            processed: response.data.processedPayouts,
            totalAmount: response.data.totalAmount,
            averagePayoutAmount: response.data.averagePayoutAmount,
            totalProviders: response.data.totalProviders,
            totalPeriods: response.data.totalPeriods
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

  loadPeriods() {
    this.loadingPeriods = true;
    this.payoutService.getAllPeriods().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.periods = response.data;
        }
        this.loadingPeriods = false;
      },
      error: (error: any) => {
        console.error('Error loading periods:', error);
        this.loadingPeriods = false;
      }
    });
  }

  filterByStatus(status: string) {
    this.selectedStatus = status;
    this.applyFilters();
  }

  filterByPeriod(period: string) {
    this.selectedPeriod = period;
    this.applyFilters();
  }

  togglePendingOnly() {
    this.showOnlyPending = !this.showOnlyPending;
    this.applyFilters();
  }

  applyFilters() {
    let filtered = this.payouts;

    // Filter by status
    if (this.selectedStatus !== 'all') {
      filtered = filtered.filter(item => item.status.toLowerCase() === this.selectedStatus.toLowerCase());
    }

    // Filter by period
    if (this.selectedPeriod !== 'all') {
      filtered = filtered.filter(item => item.periodId === this.selectedPeriod);
    }

    // Filter by search term
    if (this.searchTerm) {
      const search = this.searchTerm.toLowerCase();
      filtered = filtered.filter(item => 
        item.providerName.toLowerCase().includes(search) ||
        item.periodName.toLowerCase().includes(search) ||
        item.paymentReference?.toLowerCase().includes(search) ||
        item.notes?.toLowerCase().includes(search)
      );
    }

    // Filter by pending only
    if (this.showOnlyPending) {
      filtered = filtered.filter(item => item.status.toLowerCase() === 'pending');
    }

    this.filteredPayouts = filtered;
    this.dataSource.data = filtered;
  }

  openProcessDialog(payout: ProviderPayout) {
    this.selectedPayout = payout;
    this.processForm.patchValue({
      status: 'Processed',
      paymentReference: `PAY-${Date.now()}`,
      notes: ''
    });
    this.showProcessDialog = true;
  }

  closeProcessDialog() {
    this.showProcessDialog = false;
    this.selectedPayout = null;
    this.processForm.reset();
  }

  submitProcess() {
    if (this.processForm.valid && this.selectedPayout) {
      const processDto: ProcessPayoutDto = {
        status: this.processForm.value.status,
        paymentReference: this.processForm.value.paymentReference,
        notes: this.processForm.value.notes
      };

      this.payoutService.processPayout(this.selectedPayout.id, processDto).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.showNotification(`Payout ${processDto.status.toLowerCase()} successfully`, 'success');
            this.closeProcessDialog();
            this.loadPayouts();
            this.loadStatistics();
          }
        },
        error: (error: any) => {
          console.error('Error processing payout:', error);
          this.showNotification('Error processing payout', 'error');
        }
      });
    }
  }

  quickProcess(payout: ProviderPayout) {
    const processDto: ProcessPayoutDto = {
      status: 'Processed',
      paymentReference: `PAY-${Date.now()}`,
      notes: 'Quick processed'
    };

    this.payoutService.processPayout(payout.id, processDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Payout processed successfully', 'success');
          this.loadPayouts();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error processing payout:', error);
        this.showNotification('Error processing payout', 'error');
      }
    });
  }

  holdPayout(payout: ProviderPayout) {
    const processDto: ProcessPayoutDto = {
      status: 'OnHold',
      notes: 'Payout placed on hold'
    };

    this.payoutService.processPayout(payout.id, processDto).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Payout placed on hold', 'success');
          this.loadPayouts();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error holding payout:', error);
        this.showNotification('Error holding payout', 'error');
      }
    });
  }

  processAllPayouts() {
    this.payoutService.processAllPendingPayouts().subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('All pending payouts processed successfully', 'success');
          this.loadPayouts();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error processing all payouts:', error);
        this.showNotification('Error processing all payouts', 'error');
      }
    });
  }

  openPeriodDialog() {
    this.periodForm.reset();
    this.showPeriodDialog = true;
  }

  closePeriodDialog() {
    this.showPeriodDialog = false;
    this.periodForm.reset();
  }

  submitPeriod() {
    if (this.periodForm.valid) {
      const createDto: CreatePayoutPeriodDto = {
        name: this.periodForm.value.name,
        startDate: this.periodForm.value.startDate.toISOString(),
        endDate: this.periodForm.value.endDate.toISOString()
      };

      this.payoutService.createPeriod(createDto).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.showNotification('Payout period created successfully', 'success');
            this.closePeriodDialog();
            this.loadPeriods();
          }
        },
        error: (error: any) => {
          console.error('Error creating period:', error);
          this.showNotification('Error creating payout period', 'error');
        }
      });
    }
  }

  generatePayoutsForPeriod(periodId: string) {
    this.payoutService.generatePayoutsForPeriod(periodId).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.showNotification('Payouts generated successfully', 'success');
          this.loadPayouts();
          this.loadStatistics();
        }
      },
      error: (error: any) => {
        console.error('Error generating payouts:', error);
        this.showNotification('Error generating payouts', 'error');
      }
    });
  }

  viewPayoutDetails(payout: ProviderPayout) {
    // Navigate to detailed view or open dialog
    console.log('View payout details:', payout);
    // this.router.navigate(['/admin/payout-management', payout.id]);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'warn';
      case 'processed':
        return 'primary';
      case 'onhold':
        return 'accent';
      default:
        return 'primary';
    }
  }

  getStatusText(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getPeriods(): PayoutPeriodDto[] {
    return this.periods.filter(period => period.status === 'Active');
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
    const headers = ['Provider', 'Period', 'Total Amount', 'Provider Amount', 'Status', 'Created Date', 'Processed Date'];
    const csvData = this.filteredPayouts.map(item => [
      item.providerName,
      item.periodName,
      `$${item.totalAmount}`,
      `$${item.providerAmount}`,
      item.status,
      new Date(item.createdAt).toLocaleDateString(),
      item.processedAt ? new Date(item.processedAt).toLocaleDateString() : 'N/A'
    ]);

    const csvContent = [headers, ...csvData]
      .map(row => row.map(cell => `"${cell}"`).join(','))
      .join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `provider-payouts-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  getProcessingRate(): number {
    if (this.stats.total > 0) {
      return (this.stats.processed / this.stats.total) * 100;
    }
    return 0;
  }

  getAverageProviderAmount(): number {
    if (this.stats.total > 0) {
      return this.stats.totalAmount / this.stats.total;
    }
    return 0;
  }
} 