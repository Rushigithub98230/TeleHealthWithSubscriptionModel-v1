import { Component, OnInit, OnDestroy } from '@angular/core';
import { AdminApiService, Subscription } from '../../../../core/admin-api.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { RealtimeService } from '../../../../core/realtime.service';
import { Subscription as RxSubscription } from 'rxjs';
import { TableColumn, TableAction } from '../../../../shared/global-table/global-table.component';

@Component({
  selector: 'app-subscription-management',
  standalone: true,
  imports: [],
  templateUrl: './subscription-management.component.html',
  styleUrl: './subscription-management.component.scss'
})
export class SubscriptionManagementComponent implements OnInit, OnDestroy {
  subscriptions: Subscription[] = [];
  loading = false;
  error: string | null = null;
  selectedSubscription: Subscription | null = null;
  billingHistory: any[] = [];
  showDetailsModal = false;
  searchTerm: string = '';
  toastMessage: string = '';
  showToast = false;
  toastType: 'success' | 'error' | 'info' = 'success';
  selectedIds = new Set<string>();
  filterForm: FormGroup;
  private realtimeSub: RxSubscription | null = null;

  subscriptionColumns: TableColumn[] = [
    { key: 'id', label: 'ID' },
    { key: 'userId', label: 'User ID' },
    { key: 'planId', label: 'Plan ID' },
    { key: 'status', label: 'Status' },
    { key: 'startDate', label: 'Start Date' },
    { key: 'nextBillingDate', label: 'Next Billing' }
  ];
  subscriptionActions: TableAction[] = [
    { label: 'Details', action: 'details' },
    { label: 'Cancel', action: 'cancel', color: 'warn' },
    { label: 'Pause', action: 'pause' },
    { label: 'Resume', action: 'resume' },
    { label: 'Upgrade', action: 'upgrade' },
    { label: 'Reactivate', action: 'reactivate' }
  ];

  constructor(
    private adminApi: AdminApiService,
    private fb: FormBuilder,
    private realtime: RealtimeService
  ) {
    this.filterForm = this.fb.group({
      status: [''],
      planId: [''],
      userId: ['']
    });
  }

  get filteredSubscriptions(): Subscription[] {
    let subs = this.subscriptions;
    const { status, planId, userId } = this.filterForm.value;
    if (status) subs = subs.filter(s => s.status === status);
    if (planId) subs = subs.filter(s => s.planId === planId);
    if (userId) subs = subs.filter(s => s.userId === userId);
    const term = this.searchTerm.trim().toLowerCase();
    if (term) {
      subs = subs.filter(sub =>
        sub.userId.toLowerCase().includes(term) ||
        sub.planId.toLowerCase().includes(term) ||
        sub.status.toLowerCase().includes(term)
      );
    }
    return subs;
  }

  updateSearchTerm(term: string) {
    this.searchTerm = term;
  }

  updateFilter() {
    // Called on filter change
  }

  clearAllFilters() {
    this.filterForm.reset({ status: '', planId: '', userId: '' });
  }

  showToastMessage(message: string, type: 'success' | 'error' | 'info' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3000);
  }

  ngOnInit() {
    this.fetchSubscriptions();
    this.realtime.connect();
    this.realtimeSub = this.realtime.onSubscriptionUpdated().subscribe(evt => {
      this.showToastMessage('Subscription updated in real time.', 'info');
      this.fetchSubscriptions();
    });
  }

  ngOnDestroy() {
    this.realtime.disconnect();
    this.realtimeSub?.unsubscribe();
  }

  fetchSubscriptions() {
    this.loading = true;
    this.adminApi.getSubscriptions().subscribe({
      next: (subs: Subscription[]) => {
        this.subscriptions = subs;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load subscriptions';
        this.loading = false;
      }
    });
  }

  // Action handlers
  cancel(sub: Subscription) {
    const reason = window.prompt('Enter cancellation reason:');
    if (!reason) return;
    if (!window.confirm('Are you sure you want to cancel this subscription?')) return;
    this.loading = true;
    this.adminApi.cancelSubscription(sub.id, reason).subscribe({
      next: (updated: Subscription) => {
        this.updateSubscriptionInList(updated);
        this.loading = false;
        this.showToastMessage('Subscription cancelled successfully.', 'success');
      },
      error: () => {
        this.error = 'Failed to cancel subscription';
        this.loading = false;
        this.showToastMessage('Failed to cancel subscription.', 'error');
      }
    });
  }

  pause(sub: Subscription) {
    this.loading = true;
    this.adminApi.pauseSubscription(sub.id).subscribe({
      next: (updated: Subscription) => {
        this.updateSubscriptionInList(updated);
        this.loading = false;
        this.showToastMessage('Subscription paused.', 'success');
      },
      error: () => {
        this.error = 'Failed to pause subscription';
        this.loading = false;
        this.showToastMessage('Failed to pause subscription.', 'error');
      }
    });
  }

  resume(sub: Subscription) {
    this.loading = true;
    this.adminApi.resumeSubscription(sub.id).subscribe({
      next: (updated: Subscription) => {
        this.updateSubscriptionInList(updated);
        this.loading = false;
        this.showToastMessage('Subscription resumed.', 'success');
      },
      error: () => {
        this.error = 'Failed to resume subscription';
        this.loading = false;
        this.showToastMessage('Failed to resume subscription.', 'error');
      }
    });
  }

  upgrade(sub: Subscription) {
    const newPlanId = window.prompt('Enter new plan ID:');
    if (!newPlanId) return;
    this.loading = true;
    this.adminApi.upgradeSubscription(sub.id, newPlanId).subscribe({
      next: (updated: Subscription) => {
        this.updateSubscriptionInList(updated);
        this.loading = false;
        this.showToastMessage('Subscription upgraded.', 'success');
      },
      error: () => {
        this.error = 'Failed to upgrade subscription';
        this.loading = false;
        this.showToastMessage('Failed to upgrade subscription.', 'error');
      }
    });
  }

  reactivate(sub: Subscription) {
    this.loading = true;
    this.adminApi.reactivateSubscription(sub.id).subscribe({
      next: (updated: Subscription) => {
        this.updateSubscriptionInList(updated);
        this.loading = false;
        this.showToastMessage('Subscription reactivated.', 'success');
      },
      error: () => {
        this.error = 'Failed to reactivate subscription';
        this.loading = false;
        this.showToastMessage('Failed to reactivate subscription.', 'error');
      }
    });
  }

  details(sub: Subscription) {
    this.selectedSubscription = sub;
    this.showDetailsModal = true;
    this.billingHistory = [];
    this.loading = true;
    this.adminApi.getSubscriptionBillingHistory(sub.id).subscribe({
      next: (history) => {
        this.billingHistory = history;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load billing history';
        this.loading = false;
      }
    });
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedSubscription = null;
    this.billingHistory = [];
  }

  private updateSubscriptionInList(updated: Subscription) {
    this.subscriptions = this.subscriptions.map(s => s.id === updated.id ? updated : s);
  }

  toggleSelection(id: string) {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  isSelected(id: string): boolean {
    return this.selectedIds.has(id);
  }

  selectAll() {
    this.filteredSubscriptions.forEach(sub => this.selectedIds.add(sub.id));
  }

  clearSelection() {
    this.selectedIds.clear();
  }

  isAllSelected(): boolean {
    return this.filteredSubscriptions.length > 0 && this.filteredSubscriptions.every(sub => this.selectedIds.has(sub.id));
  }

  async bulkCancel() {
    if (this.selectedIds.size === 0) return;
    if (!window.confirm(`Are you sure you want to cancel ${this.selectedIds.size} subscriptions?`)) return;
    this.loading = true;
    let successCount = 0;
    let failCount = 0;
    for (const id of Array.from(this.selectedIds)) {
      try {
        await this.adminApi.cancelSubscription(id, 'Bulk cancel').toPromise();
        successCount++;
      } catch (e) {
        failCount++;
      }
    }
    this.loading = false;
    this.clearSelection();
    this.fetchSubscriptions();
    this.showToastMessage(`Bulk cancel complete: ${successCount} succeeded, ${failCount} failed.`, failCount === 0 ? 'success' : 'error');
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: Subscription }) {
    switch (event.action) {
      case 'details': this.details(event.row); break;
      case 'cancel': this.cancel(event.row); break;
      case 'pause': this.pause(event.row); break;
      case 'resume': this.resume(event.row); break;
      case 'upgrade': this.upgrade(event.row); break;
      case 'reactivate': this.reactivate(event.row); break;
    }
  }
}
