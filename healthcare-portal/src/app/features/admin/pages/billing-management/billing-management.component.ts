import { Component, OnInit } from '@angular/core';
import { AdminApiService, BillingRecord, RevenueSummary } from '../../../../core/admin-api.service';
import { TableColumn, TableAction } from '../../../../shared/global-table/global-table.component';

@Component({
  selector: 'app-billing-management',
  standalone: true,
  imports: [],
  templateUrl: './billing-management.component.html',
  styleUrl: './billing-management.component.scss'
})
export class BillingManagementComponent implements OnInit {
  pending: BillingRecord[] = [];
  overdue: BillingRecord[] = [];
  loading = false;
  error: string | null = null;
  searchTerm: string = '';
  selectedRecord: BillingRecord | null = null;
  showDetailsModal = false;
  refundAmount: string = '';
  revenueSummary: RevenueSummary | null = null;
  toastMessage: string = '';
  showToast = false;
  toastType: 'success' | 'error' = 'success';

  billingColumns: TableColumn[] = [
    { key: 'id', label: 'ID' },
    { key: 'userId', label: 'User ID' },
    { key: 'subscriptionId', label: 'Subscription ID' },
    { key: 'amount', label: 'Amount' },
    { key: 'status', label: 'Status' },
    { key: 'date', label: 'Date' }
  ];
  billingActions: TableAction[] = [
    { label: 'Details', action: 'details' },
    { label: 'Refund', action: 'refund' }
  ];
  selectedIds = new Set<string>();

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.fetchBillingRecords();
    this.fetchRevenueSummary();
  }

  fetchBillingRecords() {
    this.loading = true;
    this.adminApi.getPendingBillingRecords().subscribe({
      next: (pending) => {
        this.pending = pending;
        this.adminApi.getOverdueBillingRecords().subscribe({
          next: (overdue) => {
            this.overdue = overdue;
            this.loading = false;
          },
          error: () => {
            this.error = 'Failed to load overdue billing records';
            this.loading = false;
          }
        });
      },
      error: () => {
        this.error = 'Failed to load pending billing records';
        this.loading = false;
      }
    });
  }

  fetchRevenueSummary() {
    this.adminApi.getRevenueSummary().subscribe({
      next: (summary) => { this.revenueSummary = summary; },
      error: () => { this.revenueSummary = null; }
    });
  }

  get filteredPending(): BillingRecord[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.pending;
    return this.pending.filter(r =>
      r.userId.toLowerCase().includes(term) ||
      (r.subscriptionId || '').toLowerCase().includes(term) ||
      r.status.toLowerCase().includes(term) ||
      (r.description || '').toLowerCase().includes(term)
    );
  }
  get filteredOverdue(): BillingRecord[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.overdue;
    return this.overdue.filter(r =>
      r.userId.toLowerCase().includes(term) ||
      (r.subscriptionId || '').toLowerCase().includes(term) ||
      r.status.toLowerCase().includes(term) ||
      (r.description || '').toLowerCase().includes(term)
    );
  }
  updateSearchTerm(term: string) {
    this.searchTerm = term;
  }

  showDetails(record: BillingRecord) {
    this.selectedRecord = record;
    this.showDetailsModal = true;
    this.refundAmount = '';
  }
  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedRecord = null;
    this.refundAmount = '';
  }
  showToastMessage(message: string, type: 'success' | 'error' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3000);
  }

  processRefund() {
    if (!this.selectedRecord || !this.refundAmount) return;
    const amount = parseFloat(this.refundAmount);
    if (isNaN(amount) || amount <= 0) {
      this.error = 'Invalid refund amount';
      this.showToastMessage('Invalid refund amount.', 'error');
      return;
    }
    if (!window.confirm('Are you sure you want to process this refund?')) return;
    this.loading = true;
    this.adminApi.processRefund(this.selectedRecord.id, amount).subscribe({
      next: (updated) => {
        this.closeDetailsModal();
        this.fetchBillingRecords();
        this.loading = false;
        this.showToastMessage('Refund processed successfully.', 'success');
      },
      error: () => {
        this.error = 'Failed to process refund';
        this.loading = false;
        this.showToastMessage('Failed to process refund.', 'error');
      }
    });
  }
  exportRevenue(format: string = 'csv') {
    this.adminApi.exportRevenue(format).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `revenue-export.${format}`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: BillingRecord }) {
    switch (event.action) {
      case 'details': this.showDetails(event.row); break;
      case 'refund': this.showDetails(event.row); break; // Refund handled in modal
    }
  }
}
