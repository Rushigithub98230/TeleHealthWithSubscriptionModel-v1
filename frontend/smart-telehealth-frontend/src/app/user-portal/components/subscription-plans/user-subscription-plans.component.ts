import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';

import { UserSubscriptionService } from '../../services/user-subscription.service';
import { AuthService } from '../../../core/services/auth.service';
import { SubscriptionPlan, BillingCycle, SubscriptionPrivilege } from '../../models/subscription.interface';
import { PaymentMethod } from '../../models/payment.interface';
import { UserPaymentService } from '../../services/user-payment.service';

@Component({
  selector: 'app-user-subscription-plans',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatDialogModule
  ],
  templateUrl: './user-subscription-plans.component.html',
  styleUrls: ['./user-subscription-plans.component.scss']
})
export class UserSubscriptionPlansComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  subscriptionPlans: SubscriptionPlan[] = [];
  filteredPlans: SubscriptionPlan[] = [];
  paymentMethods: PaymentMethod[] = [];
  loading = false;
  processing = false;
  error: string | null = null;

  // Modal state
  showSubscribeModal = false;
  selectedPlan: SubscriptionPlan | null = null;
  selectedPaymentMethod: PaymentMethod | null = null;
  selectedBillingCycle: number = 1;

  // Forms
  filterForm: FormGroup;
  subscribeForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userSubscriptionService: UserSubscriptionService,
    private userPaymentService: UserPaymentService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.fb.group({
      billingCycle: [''],
      category: ['']
    });

    this.subscribeForm = this.fb.group({
      paymentMethodId: ['', Validators.required],
      billingCycleId: [1, Validators.required],
      acceptTerms: [false, Validators.requiredTrue]
    });
  }

  ngOnInit(): void {
    this.loadSubscriptionPlans();
    this.loadPaymentMethods();
    this.setupFilterListener();
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

  loadSubscriptionPlans(): void {
    this.loading = true;
    this.error = null;

    this.userSubscriptionService.getAvailablePlans()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.statusCode === 200 && response.data) {
            this.subscriptionPlans = response.data;
            this.filteredPlans = [...this.subscriptionPlans];
          } else {
            this.error = response.message || 'Failed to load subscription plans';
          }
        },
        error: (error: any) => {
          console.error('Error loading subscription plans:', error);
          this.error = 'Failed to load subscription plans';
        },
        complete: () => {
          this.loading = false;
        }
      });
  }

  loadPaymentMethods(): void {
    this.userPaymentService.getUserPaymentMethods()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.statusCode === 200 && response.data) {
            this.paymentMethods = response.data;
          }
        },
        error: (error: any) => {
          console.error('Error loading payment methods:', error);
        }
      });
  }

  applyFilters(): void {
    const { billingCycle, category } = this.filterForm.value;
    
    this.filteredPlans = this.subscriptionPlans.filter(plan => {
      const billingMatch = !billingCycle || plan.billingCycleId === parseInt(billingCycle);
      const categoryMatch = !category || plan.category?.name === category;
      return billingMatch && categoryMatch;
    });
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.filteredPlans = [...this.subscriptionPlans];
  }

  openSubscribeModal(plan: SubscriptionPlan): void {
    this.selectedPlan = plan;
    this.showSubscribeModal = true;
    this.subscribeForm.reset({
      billingCycleId: 1,
      acceptTerms: false
    });
  }

  closeSubscribeModal(): void {
    this.showSubscribeModal = false;
    this.selectedPlan = null;
    this.selectedPaymentMethod = null;
    this.subscribeForm.reset();
  }

  selectPaymentMethod(method: PaymentMethod): void {
    this.selectedPaymentMethod = method;
    this.subscribeForm.patchValue({
      paymentMethodId: method.id
    });
  }

  selectBillingCycle(cycleId: number): void {
    this.selectedBillingCycle = cycleId;
    this.subscribeForm.patchValue({
      billingCycleId: cycleId
    });
  }

  onSubmit(): void {
    if (this.subscribeForm.invalid || !this.selectedPlan) {
      return;
    }

    this.processing = true;
    const formData = this.subscribeForm.value;

    const subscriptionData = {
      subscriptionPlanId: this.selectedPlan.id,
      paymentMethodId: formData.paymentMethodId,
      billingCycleId: formData.billingCycleId
    };

    this.userSubscriptionService.createSubscription(subscriptionData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.statusCode === 200) {
            this.snackBar.open('Subscription created successfully!', 'Close', { duration: 3000 });
            this.closeSubscribeModal();
            this.router.navigate(['/web/subscriptions']);
          } else {
            this.error = response.message || 'Failed to create subscription';
          }
        },
        error: (error: any) => {
          console.error('Error creating subscription:', error);
          this.error = 'Failed to create subscription';
        },
        complete: () => {
          this.processing = false;
        }
      });
  }

  viewPlanDetails(plan: SubscriptionPlan): void {
    // Navigate to plan details page or show detailed modal
    console.log('View plan details:', plan);
  }

  navigateToSubscriptions(): void {
    this.router.navigate(['/web/subscriptions']);
  }

  navigateToPaymentMethods(): void {
    this.router.navigate(['/web/payment-methods']);
  }

  viewTerms(): void {
    // Implement terms view logic
    console.log('View terms clicked');
  }

  viewPrivacy(): void {
    // Implement privacy view logic
    console.log('View privacy clicked');
  }

  getBillingCycleText(billingCycle: BillingCycle): string {
    switch (billingCycle.name) {
      case 'Monthly':
        return '1 Month';
      case 'Quarterly':
        return '3 Months';
      case 'Annual':
        return '12 Months';
      default:
        return billingCycle.name;
    }
  }

  getBillingCyclePrice(plan: SubscriptionPlan, billingCycleId: number): number {
    if (plan.billingCycleId === billingCycleId) {
      return plan.price;
    }
    return plan.price; // Simplified for now
  }

  getBillingCycleDiscount(plan: SubscriptionPlan, billingCycleId: number): number {
    if (plan.billingCycleId === billingCycleId && plan.discountedPrice) {
      return Math.round(((plan.price - plan.discountedPrice) / plan.price) * 100);
    }
    return 0;
  }

  getDiscountPercentage(originalPrice: number, discountedPrice: number): number {
    return Math.round(((originalPrice - discountedPrice) / originalPrice) * 100);
  }

  getPrivilegeUnit(privilegeName: string): string {
    const units: { [key: string]: string } = {
      'Teleconsultation': 'sessions',
      'Medical Records': 'MB',
      'Prescriptions': 'per month',
      'Lab Results': 'per month'
    };
    return units[privilegeName] || '';
  }

  getCommonFeatures(): any[] {
    const allFeatures = new Set<string>();
    this.filteredPlans.forEach(plan => {
      plan.privileges?.forEach((privilege: SubscriptionPrivilege) => {
        allFeatures.add(privilege.privilege?.name || '');
      });
    });
    return Array.from(allFeatures).map(name => ({ name }));
  }

  hasFeature(plan: SubscriptionPlan, featureName: string): boolean {
    return plan.privileges?.some((p: SubscriptionPrivilege) => p.privilege?.name === featureName) || false;
  }

  getAvailableBillingCycles(plan: SubscriptionPlan | null): any[] {
    if (!plan) return [];
    
    const cycles = [
      { value: 1, label: 'Monthly' },
      { value: 3, label: 'Quarterly' },
      { value: 12, label: 'Annual' }
    ];
    
    return cycles.filter(cycle => {
      // Filter based on plan availability
      return true; // Simplified for now
    });
  }

  getCyclePrice(plan: SubscriptionPlan | null, cycleId: number): number {
    if (!plan) return 0;
    
    // This would typically calculate the price based on billing cycle
    // For now, return the base price
    return plan.discountedPrice || plan.price || 0;
  }

  getPaymentTypeIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'visa': 'fa-cc-visa',
      'mastercard': 'fa-cc-mastercard',
      'amex': 'fa-cc-amex',
      'discover': 'fa-cc-discover',
      'jcb': 'fa-cc-jcb',
      'diners': 'fa-cc-diners-club'
    };
    return icons[type.toLowerCase()] || 'fa-credit-card';
  }

  maskCardNumber(last4: string): string {
    return `**** **** **** ${last4}`;
  }
}
