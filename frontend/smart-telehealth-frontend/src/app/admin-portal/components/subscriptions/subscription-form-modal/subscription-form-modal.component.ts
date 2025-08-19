import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { CommonService } from '../../../../core/services/common.service';

import { Subscription, SubscriptionPlan, User, SubscriptionStatus, BillingCycle } from '../../../../core/models/index';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-subscription-form-modal',
  templateUrl: './subscription-form-modal.component.html',
  styleUrls: ['./subscription-form-modal.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class SubscriptionFormModalComponent implements OnInit, OnDestroy {
  @Input() isCreateMode = true;
  @Input() subscription: Subscription | null = null;
  @Output() success = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  // Form
  subscriptionForm: FormGroup;
  isLoading = false;

  // Data
  subscriptionPlans: SubscriptionPlan[] = [];
  users: User[] = [];
  filteredUsers: User[] = [];

  // User search
  isSearchingUsers = false;
  showUserResults = false;
  selectedUser: User | null = null;

  // Plan selection
  selectedPlan: SubscriptionPlan | null = null;

  // Status and billing options
  statusOptions: { value: SubscriptionStatus; label: string; color: string }[] = [
    { value: 'Active', label: 'Active', color: '#10b981' },
    { value: 'Inactive', label: 'Inactive', color: '#6b7280' },
    { value: 'Suspended', label: 'Suspended', color: '#f59e0b' },
    { value: 'Cancelled', label: 'Cancelled', color: '#ef4444' },
    { value: 'Expired', label: 'Expired', color: '#8b5cf6' },
    { value: 'Pending', label: 'Pending', color: '#3b82f6' }
  ];

  billingCycleOptions: { value: BillingCycle; label: string }[] = [
    { value: 'Monthly', label: 'Monthly' },
    { value: 'Quarterly', label: 'Quarterly' },
    { value: 'SemiAnnually', label: 'Semi-Annually' },
    { value: 'Annually', label: 'Annually' }
  ];

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private toastService: ToastService
  ) {
    this.subscriptionForm = this.fb.group({
      userId: ['', Validators.required],
      subscriptionPlanId: ['', Validators.required],
      status: ['Active', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      billingCycle: ['Monthly', Validators.required],
      amount: ['', [Validators.required, Validators.min(0)]],
      discountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      notes: [''],
      autoRenew: [true],
      sendNotifications: [true]
    }, { validators: this.dateRangeValidator });
  }

  ngOnInit(): void {
    this.loadSubscriptionPlans();
    this.setupFormListeners();
    
    if (!this.isCreateMode && this.subscription) {
      this.populateForm();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setupFormListeners(): void {
    // Listen to plan changes to auto-calculate dates and amount
    this.subscriptionForm.get('subscriptionPlanId')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(planId => {
        if (planId) {
          this.onPlanSelection(planId);
        }
      });

    // Listen to billing cycle changes to auto-calculate end date
    this.subscriptionForm.get('billingCycle')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(billingCycle => {
        if (billingCycle && this.subscriptionForm.get('startDate')?.value) {
          this.calculateEndDate();
        }
      });

    // Listen to start date changes to auto-calculate end date
    this.subscriptionForm.get('startDate')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(startDate => {
        if (startDate && this.subscriptionForm.get('billingCycle')?.value) {
          this.calculateEndDate();
        }
      });

    // Listen to discount changes to update amount
    this.subscriptionForm.get('discountPercentage')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(discount => {
        if (discount && this.selectedPlan) {
          this.calculateDiscountedAmount();
        }
      });
  }

  loadSubscriptionPlans(): void {
    this.commonService.get<SubscriptionPlan[]>('/subscription-plans')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.subscriptionPlans = response.data;
          }
        },
        error: (error) => {
          console.error('Error loading subscription plans:', error);
          this.toastService.showError('Failed to load subscription plans');
        }
      });
  }

  onPlanSelection(planId: number): void {
    const plan = this.subscriptionPlans.find(p => p.id === planId);
    if (plan) {
      this.selectedPlan = plan;
      this.subscriptionForm.patchValue({
        amount: plan.price,
        discountPercentage: 0
      });
      this.calculateDiscountedAmount();
    }
  }

  calculateEndDate(): void {
    const startDate = this.subscriptionForm.get('startDate')?.value;
    const billingCycle = this.subscriptionForm.get('billingCycle')?.value;
    
    if (startDate && billingCycle) {
      const start = new Date(startDate);
      let end = new Date(start);
      
      switch (billingCycle) {
        case 'Monthly':
          end.setMonth(end.getMonth() + 1);
          break;
        case 'Quarterly':
          end.setMonth(end.getMonth() + 3);
          break;
        case 'SemiAnnually':
          end.setMonth(end.getMonth() + 6);
          break;
        case 'Annually':
          end.setFullYear(end.getFullYear() + 1);
          break;
      }
      
      // Subtract one day to make it inclusive
      end.setDate(end.getDate() - 1);
      
      this.subscriptionForm.patchValue({
        endDate: end.toISOString().split('T')[0]
      });
    }
  }

  calculateDiscountedAmount(): void {
    if (this.selectedPlan) {
      const discount = this.subscriptionForm.get('discountPercentage')?.value || 0;
      const originalAmount = this.selectedPlan.price;
      const discountedAmount = originalAmount - (originalAmount * discount / 100);
      
      this.subscriptionForm.patchValue({
        amount: Math.round(discountedAmount * 100) / 100
      });
    }
  }

  // User search functionality
  onUserSearch(event: any): void {
    const searchTerm = event.target.value;
    
    if (searchTerm.length < 2) {
      this.filteredUsers = [];
      this.showUserResults = false;
      return;
    }

    this.isSearchingUsers = true;
    this.showUserResults = true;

    // Simulate API call with debounce
    setTimeout(() => {
      this.searchUsers(searchTerm);
    }, 300);
  }

  searchUsers(searchTerm: string): void {
    const params = {
      search: searchTerm,
      userType: 'User,Provider',
      limit: 10
    };

    this.commonService.get<User[]>('/users/search', params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.filteredUsers = response.data;
          }
        },
        error: (error) => {
          console.error('Error searching users:', error);
          this.filteredUsers = [];
        },
        complete: () => {
          this.isSearchingUsers = false;
        }
      });
  }

  selectUser(user: User): void {
    this.selectedUser = user;
    this.subscriptionForm.patchValue({ userId: user.id });
    this.showUserResults = false;
    this.filteredUsers = [];
  }

  removeSelectedUser(): void {
    this.selectedUser = null;
    this.subscriptionForm.patchValue({ userId: '' });
  }

  onUserSearchBlur(): void {
    // Delay hiding results to allow clicking on them
    setTimeout(() => {
      this.showUserResults = false;
    }, 200);
  }

  // Form validation
  dateRangeValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const startDate = control.get('startDate');
    const endDate = control.get('endDate');
    
    if (startDate && endDate && startDate.value && endDate.value) {
      const start = new Date(startDate.value);
      const end = new Date(endDate.value);
      
      if (start >= end) {
        return { invalidDateRange: true };
      }
    }
    
    return null;
  }

  populateForm(): void {
    if (this.subscription) {
      this.subscriptionForm.patchValue({
        userId: this.subscription.userId,
        subscriptionPlanId: this.subscription.subscriptionPlanId,
        status: this.subscription.status,
        startDate: this.subscription.startDate,
        endDate: this.subscription.endDate,
        billingCycle: this.subscription.billingCycle,
        amount: this.subscription.amount,
        discountPercentage: this.subscription.discountPercentage || 0,
        notes: this.subscription.notes || '',
        autoRenew: this.subscription.autoRenew,
        sendNotifications: this.subscription.sendNotifications
      });

      // Set selected user and plan
      if (this.subscription.user) {
        this.selectedUser = this.subscription.user;
      }
      if (this.subscription.subscriptionPlan) {
        this.selectedPlan = this.subscription.subscriptionPlan;
      }
    }
  }

  onSubmit(): void {
    if (this.subscriptionForm.valid) {
      this.isLoading = true;
      
      const formData = this.subscriptionForm.value;
      const endpoint = this.isCreateMode ? '/admin/subscriptions' : `/admin/subscriptions/${this.subscription?.id}`;
      const method = this.isCreateMode ? 'post' : 'put';

      this.commonService[method]<any>(endpoint, formData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              const message = this.isCreateMode ? 'Subscription created successfully' : 'Subscription updated successfully';
              this.toastService.showSuccess(message);
              this.success.emit();
            } else {
              this.toastService.showError(response.message || 'Operation failed');
            }
          },
          error: (error) => {
            this.toastService.showError('An error occurred. Please try again.');
            console.error('Subscription operation error:', error);
          },
          complete: () => {
            this.isLoading = false;
          }
        });
    } else {
      this.markFormGroupTouched();
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }

  markFormGroupTouched(): void {
    Object.keys(this.subscriptionForm.controls).forEach(key => {
      const control = this.subscriptionForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.subscriptionForm.get(fieldName);
    if (field?.errors && field?.touched) {
      if (field.errors['required']) return `${this.getFieldDisplayName(fieldName)} is required`;
      if (field.errors['min']) return `${this.getFieldDisplayName(fieldName)} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${this.getFieldDisplayName(fieldName)} must not exceed ${field.errors['max'].max}`;
      if (field.errors['invalidDateRange']) return 'End date must be after start date';
    }
    return '';
  }

  getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      userId: 'User',
      subscriptionPlanId: 'Subscription Plan',
      status: 'Status',
      startDate: 'Start Date',
      endDate: 'End Date',
      billingCycle: 'Billing Cycle',
      amount: 'Amount',
      discountPercentage: 'Discount Percentage',
      notes: 'Notes'
    };
    return displayNames[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.subscriptionForm.get(fieldName);
    return !!(field?.invalid && field?.touched);
  }

  getFormattedAmount(): string {
    const amount = this.subscriptionForm.get('amount')?.value;
    if (amount) {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(amount);
    }
    return '$0.00';
  }

  getDiscountAmount(): string {
    const amount = this.subscriptionForm.get('amount')?.value;
    const discount = this.subscriptionForm.get('discountPercentage')?.value || 0;
    if (amount && discount > 0) {
      const discountAmount = (amount * discount) / 100;
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(discountAmount);
    }
    return '$0.00';
  }

  getFinalAmount(): string {
    const amount = this.subscriptionForm.get('amount')?.value;
    if (amount) {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(amount);
    }
    return '$0.00';
  }
}
