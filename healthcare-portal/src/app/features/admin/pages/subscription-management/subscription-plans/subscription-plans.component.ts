import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { AdminSubscriptionManagementService, AdminSubscriptionPlan, ApiResponse } from '../../../../../core/services/admin-subscription-management.service';

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  price: number;
  billingCycle: 'monthly' | 'quarterly' | 'yearly';
  features: string[];
  isActive: boolean;
  maxUsers?: number;
  trialDays?: number;
  category: string;
  createdAt: Date;
  updatedAt: Date;
}

@Component({
  selector: 'app-subscription-plans',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatCheckboxModule,
    MatSlideToggleModule
  ],
  templateUrl: './subscription-plans.component.html',
  styleUrls: ['./subscription-plans.component.scss']
})
export class SubscriptionPlansComponent implements OnInit {
  plans: AdminSubscriptionPlan[] = [];
  filteredPlans: AdminSubscriptionPlan[] = [];
  loading = false;
  error = '';
  successMessage = '';
  showCreateModal = false;
  showEditModal = false;
  selectedPlan: AdminSubscriptionPlan | null = null;
  searchTerm = '';
  selectedCategory = '';
  selectedStatus = '';

  categories = ['Hair Loss', 'Skin Care', 'Mental Health', 'Weight Loss', 'General Health'];
  statuses = ['active', 'inactive'];
  billingCycles = ['monthly', 'quarterly', 'yearly'];

  // Form data for create/edit
  newPlan = {
    name: '',
    description: '',
    price: 0,
    billingCycleId: '',
    currencyId: 'USD',
    features: [''],
    isActive: true,
    isFeatured: false,
    isTrialAllowed: false,
    trialDurationInDays: 0,
    displayOrder: 0,
    stripeProductId: '',
    stripeMonthlyPriceId: '',
    stripeQuarterlyPriceId: '',
    stripeAnnualPriceId: ''
  };

  constructor(
    private subscriptionService: AdminSubscriptionManagementService
  ) {}

  ngOnInit(): void {
    this.loadPlans();
  }

  loadPlans(): void {
    this.loading = true;
    this.error = '';

    this.subscriptionService.getAllPlans(
      this.searchTerm,
      this.selectedStatus
    ).subscribe({
      next: (response: ApiResponse<AdminSubscriptionPlan[]>) => {
        if (response.success && response.data) {
          this.plans = response.data;
          this.filteredPlans = this.plans;
        } else {
          this.error = response.message || 'Failed to load plans';
        }
        this.loading = false;
      },
      error: (error: any) => {
        this.error = 'Error loading plans. Please try again.';
        this.loading = false;
        console.error('Error loading plans:', error);
      }
    });
  }

  onSearchChange(): void {
    this.loadPlans();
  }

  onFilterChange(): void {
    this.loadPlans();
  }

  openCreateModal(): void {
    this.resetNewPlan();
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.resetNewPlan();
  }

  openEditModal(plan: AdminSubscriptionPlan): void {
    this.selectedPlan = plan;
    this.newPlan = {
      name: plan.name,
      description: plan.description || '',
      price: plan.price,
      billingCycleId: plan.billingCycleId,
      currencyId: plan.currencyId,
      features: plan.features ? [plan.features] : [''],
      isActive: plan.isActive,
      isFeatured: plan.isFeatured,
      isTrialAllowed: plan.isTrialAllowed,
      trialDurationInDays: plan.trialDurationInDays,
      displayOrder: plan.displayOrder,
      stripeProductId: plan.stripeProductId || '',
      stripeMonthlyPriceId: plan.stripeMonthlyPriceId || '',
      stripeQuarterlyPriceId: plan.stripeQuarterlyPriceId || '',
      stripeAnnualPriceId: plan.stripeAnnualPriceId || ''
    };
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedPlan = null;
    this.resetNewPlan();
  }

  resetNewPlan(): void {
    this.newPlan = {
      name: '',
      description: '',
      price: 0,
      billingCycleId: '',
      currencyId: 'USD',
      features: [''],
      isActive: true,
      isFeatured: false,
      isTrialAllowed: false,
      trialDurationInDays: 0,
      displayOrder: 0,
      stripeProductId: '',
      stripeMonthlyPriceId: '',
      stripeQuarterlyPriceId: '',
      stripeAnnualPriceId: ''
    };
  }

  addFeature(): void {
    this.newPlan.features.push('');
  }

  removeFeature(index: number): void {
    this.newPlan.features.splice(index, 1);
  }

  createPlan(): void {
    if (!this.validatePlan()) return;

    this.loading = true;
    const planData = {
      ...this.newPlan,
      features: this.newPlan.features.filter(f => f.trim() !== '').join(',')
    };

    this.subscriptionService.createPlan(planData)
      .subscribe({
        next: (response: ApiResponse<AdminSubscriptionPlan>) => {
          if (response.success) {
            this.successMessage = 'Plan created successfully';
            this.closeCreateModal();
            this.loadPlans();
          } else {
            this.error = response.message || 'Failed to create plan';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'Error creating plan. Please try again.';
          this.loading = false;
          console.error('Error creating plan:', error);
        }
      });
  }

  updatePlan(): void {
    if (!this.selectedPlan || !this.validatePlan()) return;

    this.loading = true;
    const planData = {
      ...this.newPlan,
      features: this.newPlan.features.filter(f => f.trim() !== '').join(',')
    };

    this.subscriptionService.updatePlan(this.selectedPlan.id, planData)
      .subscribe({
        next: (response: ApiResponse<AdminSubscriptionPlan>) => {
          if (response.success) {
            this.successMessage = 'Plan updated successfully';
            this.closeEditModal();
            this.loadPlans();
          } else {
            this.error = response.message || 'Failed to update plan';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'Error updating plan. Please try again.';
          this.loading = false;
          console.error('Error updating plan:', error);
        }
      });
  }

  deletePlan(plan: AdminSubscriptionPlan): void {
    if (confirm(`Are you sure you want to delete the plan "${plan.name}"?`)) {
      this.loading = true;
      
      this.subscriptionService.deletePlan(plan.id)
        .subscribe({
          next: (response: ApiResponse<boolean>) => {
            if (response.success) {
              this.successMessage = 'Plan deleted successfully';
              this.loadPlans();
            } else {
              this.error = response.message || 'Failed to delete plan';
            }
            this.loading = false;
          },
          error: (error: any) => {
            this.error = 'Error deleting plan. Please try again.';
            this.loading = false;
            console.error('Error deleting plan:', error);
          }
        });
    }
  }

  togglePlanStatus(plan: AdminSubscriptionPlan): void {
    const action = plan.isActive ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} the plan "${plan.name}"?`)) {
      this.loading = true;
      
      if (plan.isActive) {
        this.subscriptionService.deactivatePlan(plan.id)
          .subscribe({
            next: (response: ApiResponse<boolean>) => {
              if (response.success) {
                this.successMessage = 'Plan deactivated successfully';
                this.loadPlans();
              } else {
                this.error = response.message || 'Failed to deactivate plan';
              }
              this.loading = false;
            },
            error: (error: any) => {
              this.error = 'Error deactivating plan. Please try again.';
              this.loading = false;
              console.error('Error deactivating plan:', error);
            }
          });
      } else {
        this.subscriptionService.activatePlan(plan.id)
          .subscribe({
            next: (response: ApiResponse<boolean>) => {
              if (response.success) {
                this.successMessage = 'Plan activated successfully';
                this.loadPlans();
              } else {
                this.error = response.message || 'Failed to activate plan';
              }
              this.loading = false;
            },
            error: (error: any) => {
              this.error = 'Error activating plan. Please try again.';
              this.loading = false;
              console.error('Error activating plan:', error);
            }
          });
      }
    }
  }

  validatePlan(): boolean {
    if (!this.newPlan.name.trim()) {
      this.error = 'Plan name is required';
      return false;
    }
    if (!this.newPlan.description.trim()) {
      this.error = 'Plan description is required';
      return false;
    }
    if (this.newPlan.price <= 0) {
      this.error = 'Price must be greater than 0';
      return false;
    }
    if (!this.newPlan.billingCycleId) {
      this.error = 'Billing cycle is required';
      return false;
    }
    if (this.newPlan.features.filter(f => f.trim() !== '').length === 0) {
      this.error = 'At least one feature is required';
      return false;
    }
    return true;
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-secondary';
  }

  getBillingCycleLabel(cycleId: string): string {
    switch (cycleId) {
      case 'monthly': return 'Monthly';
      case 'quarterly': return 'Quarterly';
      case 'yearly': return 'Yearly';
      default: return cycleId;
    }
  }

  clearError(): void {
    this.error = '';
  }

  clearSuccess(): void {
    this.successMessage = '';
  }
} 