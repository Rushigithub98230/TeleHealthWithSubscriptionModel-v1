import { Component, OnInit } from '@angular/core';
import { AdminApiService, Plan } from '../../../../core/admin-api.service';
import { TableColumn, TableAction } from '../../../../shared/global-table/global-table.component';

@Component({
  selector: 'app-subscription-plans-management',
  standalone: true,
  imports: [],
  templateUrl: './subscription-plans-management.component.html',
  styleUrl: './subscription-plans-management.component.scss'
})
export class SubscriptionPlansManagementComponent implements OnInit {
  plans: Plan[] = [];
  loading = false;
  error: string | null = null;
  searchTerm: string = '';
  selectedPlan: Plan | null = null;
  showDetailsModal = false;
  toastMessage: string = '';
  showToast = false;
  toastType: 'success' | 'error' = 'success';

  planColumns: TableColumn[] = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name' },
    { key: 'price', label: 'Price' },
    { key: 'duration', label: 'Duration' },
    { key: 'status', label: 'Status' }
  ];
  planActions: TableAction[] = [
    { label: 'Details', action: 'details' },
    { label: 'Edit', action: 'edit' },
    { label: 'Delete', action: 'delete', color: 'warn' }
  ];
  selectedIds = new Set<string>();

  get filteredPlans(): Plan[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.plans;
    return this.plans.filter(plan =>
      plan.name.toLowerCase().includes(term) ||
      plan.status.toLowerCase().includes(term) ||
      plan.duration.toLowerCase().includes(term)
    );
  }

  updateSearchTerm(term: string) {
    this.searchTerm = term;
  }

  showDetails(plan: Plan) {
    this.selectedPlan = plan;
    this.showDetailsModal = true;
    // Placeholder: fetch analytics/privileges if needed
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedPlan = null;
  }

  showToastMessage(message: string, type: 'success' | 'error' = 'success') {
    this.toastMessage = message;
    this.toastType = type;
    this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3000);
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: Plan }) {
    switch (event.action) {
      case 'details': this.showDetails(event.row); break;
      case 'edit': this.editPlan(event.row); break;
      case 'delete': this.deletePlan(event.row); break;
    }
  }

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.fetchPlans();
  }

  fetchPlans() {
    this.loading = true;
    this.adminApi.getPlans().subscribe({
      next: (plans: Plan[]) => {
        this.plans = plans;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load plans';
        this.loading = false;
      }
    });
  }

  addPlan() {
    const name = window.prompt('Enter plan name:');
    if (!name) return;
    const priceStr = window.prompt('Enter plan price:');
    if (!priceStr) return;
    const price = parseFloat(priceStr);
    if (isNaN(price)) {
      this.error = 'Invalid price';
      this.showToastMessage('Invalid price.', 'error');
      return;
    }
    const duration = window.prompt('Enter plan duration (e.g., 1 Month):', '1 Month');
    if (!duration) return;
    const status = window.prompt('Enter plan status (Active/Inactive):', 'Active');
    if (!status) return;
    this.loading = true;
    this.adminApi.addPlan({ name, price, duration, status }).subscribe({
      next: (plan: Plan) => {
        this.plans.push(plan);
        this.loading = false;
        this.showToastMessage('Plan added successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to add plan';
        this.loading = false;
        this.showToastMessage('Failed to add plan.', 'error');
      }
    });
  }

  editPlan(plan: Plan) {
    const name = window.prompt('Edit plan name:', plan.name);
    if (!name) return;
    const priceStr = window.prompt('Edit plan price:', plan.price.toString());
    if (!priceStr) return;
    const price = parseFloat(priceStr);
    if (isNaN(price)) {
      this.error = 'Invalid price';
      this.showToastMessage('Invalid price.', 'error');
      return;
    }
    const duration = window.prompt('Edit plan duration (e.g., 1 Month):', plan.duration);
    if (!duration) return;
    const status = window.prompt('Edit plan status (Active/Inactive):', plan.status);
    if (!status) return;
    this.loading = true;
    this.adminApi.updatePlan(plan.id, { name, price, duration, status }).subscribe({
      next: (updated: Plan) => {
        this.plans = this.plans.map(p => p.id === plan.id ? updated : p);
        this.loading = false;
        this.showToastMessage('Plan updated successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to update plan';
        this.loading = false;
        this.showToastMessage('Failed to update plan.', 'error');
      }
    });
  }

  deletePlan(plan: Plan) {
    if (!window.confirm(`Are you sure you want to delete plan ${plan.name}?`)) {
      return;
    }
    this.loading = true;
    this.adminApi.deletePlan(plan.id).subscribe({
      next: () => {
        this.plans = this.plans.filter(p => p.id !== plan.id);
        this.loading = false;
        this.showToastMessage('Plan deleted successfully.', 'success');
      },
      error: (err: any) => {
        this.error = 'Failed to delete plan';
        this.loading = false;
        this.showToastMessage('Failed to delete plan.', 'error');
      }
    });
  }
}
