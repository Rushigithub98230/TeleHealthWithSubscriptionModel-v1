import { Component, OnInit } from '@angular/core';
import { AdminApiService, Plan } from '../../../../core/admin-api.service';

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
      },
      error: (err: any) => {
        this.error = 'Failed to add plan';
        this.loading = false;
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
      },
      error: (err: any) => {
        this.error = 'Failed to update plan';
        this.loading = false;
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
      },
      error: (err: any) => {
        this.error = 'Failed to delete plan';
        this.loading = false;
      }
    });
  }
}
