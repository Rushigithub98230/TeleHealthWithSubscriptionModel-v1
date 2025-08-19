import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { SubscriptionService, SubscriptionPlan, BillingCycle, CreateSubscriptionPlanRequest, UpdateSubscriptionPlanRequest } from '../../../services/subscription.service';
import { PrivilegeService, Privilege } from '../../../services/privilege.service';

@Component({
  selector: 'app-subscription-plan-form-modal',
  templateUrl: './subscription-plan-form-modal.component.html',
  styleUrls: ['./subscription-plan-form-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule
  ]
})
export class SubscriptionPlanFormModalComponent implements OnInit, OnDestroy {
  @Input() isCreateMode: boolean = true;
  @Input() plan: SubscriptionPlan | null = null;
  @Input() privileges: Privilege[] = [];
  
  @Output() success = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  planForm: FormGroup;
  isSubmitting = false;
  billingCycles: Array<{ value: BillingCycle; label: string; description: string }> = [];
  availablePrivileges: Privilege[] = [];

  constructor(
    private fb: FormBuilder,
    private subscriptionService: SubscriptionService,
    private privilegeService: PrivilegeService,
    private snackBar: MatSnackBar
  ) {
    console.log('SubscriptionPlanFormModalComponent constructor called');
    this.planForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      price: [0, [Validators.required, Validators.min(0)]],
      billingCycle: ['', Validators.required],
      duration: [1, [Validators.required, Validators.min(1)]],
      features: this.fb.array([]),
      isActive: [true],
      maxUsers: [null, [Validators.min(1)]],
      privilegeIds: [[], Validators.required]
    });
  }

  ngOnInit(): void {
    console.log('SubscriptionPlanFormModalComponent ngOnInit called');
    this.billingCycles = this.subscriptionService.getBillingCycleOptions();
    this.availablePrivileges = this.privileges.filter(p => p.isActive);
    
    if (!this.isCreateMode && this.plan) {
      this.populateFormForEdit();
    }

    // Add initial feature field
    this.addFeature();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private populateFormForEdit(): void {
    if (!this.plan) return;

    // Clear existing features
    while (this.featuresArray.length !== 0) {
      this.featuresArray.removeAt(0);
    }

    // Add existing features
    if (this.plan.features && this.plan.features.length > 0) {
      this.plan.features.forEach(feature => {
        this.featuresArray.push(this.fb.control(feature, Validators.required));
      });
    } else {
      this.addFeature();
    }

    // Populate form with existing data
    this.planForm.patchValue({
      name: this.plan.name,
      description: this.plan.description,
      price: this.plan.price,
      billingCycle: this.plan.billingCycle,
      duration: this.plan.duration,
      isActive: this.plan.isActive,
      maxUsers: this.plan.maxUsers,
      privilegeIds: this.plan.privileges?.map(p => p.privilegeId) || []
    });
  }

  onSubmit(): void {
    if (this.planForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;
    const formData = this.planForm.value;

    // Prepare the request data
    const requestData = {
      ...formData,
      features: formData.features.filter((feature: string) => feature.trim() !== '')
    };

    if (this.isCreateMode) {
      this.createSubscriptionPlan(requestData);
    } else {
      this.updateSubscriptionPlan(requestData);
    }
  }

  private createSubscriptionPlan(data: CreateSubscriptionPlanRequest): void {
    console.log('Creating subscription plan with data:', data);
    this.subscriptionService.createSubscriptionPlan(data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.showSuccessMessage('Subscription plan created successfully');
            this.success.emit();
          } else {
            this.showErrorMessage(response.message || 'Failed to create subscription plan');
          }
        },
        error: (error) => {
          this.showErrorMessage('Failed to create subscription plan');
          console.error('Error creating subscription plan:', error);
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }

  private updateSubscriptionPlan(data: UpdateSubscriptionPlanRequest): void {
    if (!this.plan) return;

    console.log('Updating subscription plan with data:', data);
    this.subscriptionService.updateSubscriptionPlan(this.plan.id, data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.showSuccessMessage('Subscription plan updated successfully');
            this.success.emit();
          } else {
            this.showErrorMessage(response.message || 'Failed to update subscription plan');
          }
        },
        error: (error) => {
          this.showErrorMessage('Failed to update subscription plan');
          console.error('Error updating subscription plan:', error);
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }

  onCancel(): void {
    this.cancel.emit();
  }

  // Feature management
  get featuresArray(): FormArray {
    return this.planForm.get('features') as FormArray;
  }

  addFeature(): void {
    this.featuresArray.push(this.fb.control('', Validators.required));
  }

  removeFeature(index: number): void {
    if (this.featuresArray.length > 1) {
      this.featuresArray.removeAt(index);
    }
  }

  // Utility methods
  private markFormGroupTouched(): void {
    Object.keys(this.planForm.controls).forEach(key => {
      const control = this.planForm.get(key);
      if (control instanceof FormArray) {
        control.controls.forEach(c => c.markAsTouched());
      } else {
        control?.markAsTouched();
      }
    });
  }

  getModalTitle(): string {
    return this.isCreateMode ? 'Create New Subscription Plan' : 'Edit Subscription Plan';
  }

  getSubmitButtonText(): string {
    return this.isSubmitting 
      ? (this.isCreateMode ? 'Creating...' : 'Updating...') 
      : (this.isCreateMode ? 'Create Plan' : 'Update Plan');
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.planForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  isFeatureInvalid(index: number): boolean {
    const featureControl = this.featuresArray.at(index);
    return !!(featureControl && featureControl.invalid && featureControl.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.planForm.get(fieldName);
    if (field && field.errors && field.touched) {
      if (field.errors['required']) return 'This field is required';
      if (field.errors['minlength']) return `Minimum length is ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['maxlength']) return `Maximum length is ${field.errors['maxlength'].requiredLength} characters`;
      if (field.errors['min']) return `Value must be ${field.errors['min'].min} or greater`;
    }
    return '';
  }

  getFeatureError(index: number): string {
    const featureControl = this.featuresArray.at(index);
    if (featureControl && featureControl.errors && featureControl.touched) {
      if (featureControl.errors['required']) return 'Feature description is required';
    }
    return '';
  }

  getBillingCycleDescription(billingCycle: BillingCycle): string {
    const cycleOption = this.billingCycles.find(option => option.value === billingCycle);
    return cycleOption?.description || '';
  }

  onBillingCycleChange(): void {
    const billingCycle = this.planForm.get('billingCycle')?.value;
    if (billingCycle === 'OneTime') {
      this.planForm.patchValue({ duration: 1 });
    }
  }

  getSelectedPrivilegeNames(): string[] {
    const selectedIds = this.planForm.get('privilegeIds')?.value || [];
    return this.availablePrivileges
      .filter(p => selectedIds.includes(p.id))
      .map(p => p.name);
  }

  onPrivilegeSelectionChange(event: any, privilegeId: number): void {
    const currentValue = this.planForm.get('privilegeIds')?.value || [];
    const isChecked = event.target.checked;
    
    if (isChecked) {
      if (!currentValue.includes(privilegeId)) {
        this.planForm.patchValue({
          privilegeIds: [...currentValue, privilegeId]
        });
      }
    } else {
      this.planForm.patchValue({
        privilegeIds: currentValue.filter((id: number) => id !== privilegeId)
      });
    }
  }

  // MatSnackBar notification methods
  private showSuccessMessage(message: string): void {
    console.log('Showing success message:', message);
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  private showErrorMessage(message: string): void {
    console.log('Showing error message:', message);
    this.snackBar.open(message, 'Close', {
      duration: 7000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }

  private showWarningMessage(message: string): void {
    console.log('Showing warning message:', message);
    this.snackBar.open(message, 'Close', {
      duration: 6000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['warning-snackbar']
    });
  }

  private showInfoMessage(message: string): void {
    console.log('Showing info message:', message);
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }

  // Test method for MatSnackBar functionality
  testSnackBar(): void {
    console.log('Testing MatSnackBar functionality');
    this.showSuccessMessage('This is a test success message!');
    
    setTimeout(() => {
      this.showErrorMessage('This is a test error message!');
    }, 1000);
    
    setTimeout(() => {
      this.showWarningMessage('This is a test warning message!');
    }, 2000);
    
    setTimeout(() => {
      this.showInfoMessage('This is a test info message!');
    }, 3000);
  }
}
