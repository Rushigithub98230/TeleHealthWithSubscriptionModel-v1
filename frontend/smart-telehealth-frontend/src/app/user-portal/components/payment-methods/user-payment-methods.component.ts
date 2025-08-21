import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { UserPaymentService } from '../../services/user-payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentMethod, CreatePaymentMethodDto } from '../../models/payment.interface';

@Component({
  selector: 'app-user-payment-methods',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule
  ],
  template: `
    <div class="user-payment-methods-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="title-section">
          <h1>Payment Methods</h1>
          <p>Manage your payment methods for subscriptions and billing</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-primary" 
            (click)="openAddPaymentMethodModal()"
            [disabled]="loading">
            <i class="fas fa-plus"></i> Add Payment Method
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-section" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Loading payment methods...</p>
      </div>

      <!-- No Payment Methods State -->
      <div class="no-payment-methods" *ngIf="!loading && paymentMethods.length === 0">
        <div class="empty-state">
          <i class="fas fa-credit-card"></i>
          <h3>No Payment Methods</h3>
          <p>You haven't added any payment methods yet. Add one to get started with subscriptions!</p>
          <button class="btn btn-primary" (click)="openAddPaymentMethodModal()">
            Add Payment Method
          </button>
        </div>
      </div>

      <!-- Payment Methods List -->
      <div class="payment-methods-section" *ngIf="!loading && paymentMethods.length > 0">
        <div class="payment-methods-grid">
          <div 
            class="payment-method-card" 
            *ngFor="let method of paymentMethods"
            [class.default]="method.isDefault">
            
            <div class="card-header">
              <div class="payment-type">
                <i class="fas" [class]="getPaymentTypeIcon(method.type)"></i>
                <span class="type-label">{{ getPaymentTypeLabel(method.type) }}</span>
              </div>
              <div class="card-actions">
                <button 
                  class="btn btn-icon" 
                  (click)="setAsDefault(method.id)"
                  [disabled]="method.isDefault"
                  matTooltip="Set as Default">
                  <i class="fas fa-star" [class]="method.isDefault ? 'fas fa-star' : 'far fa-star'"></i>
                </button>
                <button 
                  class="btn btn-icon" 
                  (click)="editPaymentMethod(method)"
                  matTooltip="Edit">
                  <i class="fas fa-edit"></i>
                </button>
                <button 
                  class="btn btn-icon btn-danger" 
                  (click)="deletePaymentMethod(method.id)"
                  matTooltip="Delete">
                  <i class="fas fa-trash"></i>
                </button>
              </div>
            </div>

            <div class="card-body">
              <div class="payment-details">
                <div class="card-number">
                  <span class="label">Card Number:</span>
                  <span class="value">{{ maskCardNumber(method.last4) }}</span>
                </div>
                <div class="card-info">
                  <div class="expiry">
                    <span class="label">Expires:</span>
                    <span class="value">{{ method.expiryMonth }}/{{ method.expiryYear }}</span>
                  </div>
                  <div class="cardholder">
                    <span class="label">Cardholder:</span>
                    <span class="value">{{ method.cardholderName }}</span>
                  </div>
                </div>
                <div class="billing-address" *ngIf="method.billingAddress">
                  <span class="label">Billing Address:</span>
                  <span class="value">{{ formatBillingAddress(method.billingAddress) }}</span>
                </div>
              </div>

              <div class="payment-status">
                <span class="status-badge" [class]="method.isDefault ? 'default' : 'active'">
                  {{ method.isDefault ? 'Default' : 'Active' }}
                </span>
                <span class="last-used" *ngIf="method.lastUsedAt">
                  Last used: {{ method.lastUsedAt | date:'shortDate' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Payment Security Notice -->
      <div class="security-notice">
        <div class="notice-content">
          <i class="fas fa-shield-alt"></i>
          <div class="notice-text">
            <h4>Secure Payment Processing</h4>
            <p>All payment information is encrypted and securely processed. We never store your full card details.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Payment Method Modal -->
    <div class="modal-overlay" *ngIf="showPaymentMethodModal" (click)="closePaymentMethodModal()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h3>{{ editingPaymentMethod ? 'Edit' : 'Add' }} Payment Method</h3>
          <button class="btn btn-icon" (click)="closePaymentMethodModal()">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <form [formGroup]="paymentMethodForm" (ngSubmit)="savePaymentMethod()" class="modal-body">
          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Card Number</mat-label>
              <input matInput formControlName="cardNumber" placeholder="1234 5678 9012 3456" maxlength="19">
              <mat-error *ngIf="paymentMethodForm.get('cardNumber')?.hasError('required')">
                Card number is required
              </mat-error>
              <mat-error *ngIf="paymentMethodForm.get('cardNumber')?.hasError('pattern')">
                Please enter a valid card number
              </mat-error>
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Cardholder Name</mat-label>
              <input matInput formControlName="cardholderName" placeholder="John Doe">
              <mat-error *ngIf="paymentMethodForm.get('cardholderName')?.hasError('required')">
                Cardholder name is required
              </mat-error>
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Expiry Month</mat-label>
              <mat-select formControlName="expiryMonth">
                <mat-option *ngFor="let month of months" [value]="month.value">
                  {{ month.label }}
                </mat-option>
              </mat-select>
              <mat-error *ngIf="paymentMethodForm.get('expiryMonth')?.hasError('required')">
                Expiry month is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Expiry Year</mat-label>
              <mat-select formControlName="expiryYear">
                <mat-option *ngFor="let year of years" [value]="year">
                  {{ year }}
                </mat-option>
              </mat-select>
              <mat-error *ngIf="paymentMethodForm.get('expiryYear')?.hasError('required')">
                Expiry year is required
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>CVV</mat-label>
              <input matInput formControlName="cvv" placeholder="123" maxlength="4" type="password">
              <mat-error *ngIf="paymentMethodForm.get('cvv')?.hasError('required')">
                CVV is required
              </mat-error>
              <mat-error *ngIf="paymentMethodForm.get('cvv')?.hasError('pattern')">
                Please enter a valid CVV
              </mat-error>
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Billing Address Line 1</mat-label>
              <input matInput formControlName="billingAddressLine1" placeholder="123 Main St">
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Billing Address Line 2</mat-label>
              <input matInput formControlName="billingAddressLine2" placeholder="Apt 4B (optional)">
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>City</mat-label>
              <input matInput formControlName="billingCity" placeholder="New York">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>State</mat-label>
              <input matInput formControlName="billingState" placeholder="NY">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>ZIP Code</mat-label>
              <input matInput formControlName="billingZipCode" placeholder="10001">
            </mat-form-field>
          </div>

          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>Country</mat-label>
              <mat-select formControlName="billingCountry">
                <mat-option value="US">United States</mat-option>
                <mat-option value="CA">Canada</mat-option>
                <mat-option value="GB">United Kingdom</mat-option>
                <mat-option value="AU">Australia</mat-option>
              </mat-select>
            </mat-form-field>
          </div>

          <div class="form-row checkbox-row">
            <mat-checkbox formControlName="isDefault">
              Set as default payment method
            </mat-checkbox>
          </div>

          <div class="form-actions">
            <button 
              type="button" 
              class="btn btn-outline" 
              (click)="closePaymentMethodModal()">
              Cancel
            </button>
            <button 
              type="submit" 
              class="btn btn-primary" 
              [disabled]="paymentMethodForm.invalid || saving">
              <mat-spinner diameter="20" *ngIf="saving"></mat-spinner>
              {{ saving ? 'Saving...' : (editingPaymentMethod ? 'Update' : 'Add') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styleUrls: ['./user-payment-methods.component.scss']
})
export class UserPaymentMethodsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  paymentMethods: PaymentMethod[] = [];
  loading = false;
  saving = false;
  error: string | null = null;

  // Modal state
  showPaymentMethodModal = false;
  editingPaymentMethod: PaymentMethod | null = null;

  // Forms
  paymentMethodForm: FormGroup;

  // Options
  months = [
    { value: '01', label: '01 - January' },
    { value: '02', label: '02 - February' },
    { value: '03', label: '03 - March' },
    { value: '04', label: '04 - April' },
    { value: '05', label: '05 - May' },
    { value: '06', label: '06 - June' },
    { value: '07', label: '07 - July' },
    { value: '08', label: '08 - August' },
    { value: '09', label: '09 - September' },
    { value: '10', label: '10 - October' },
    { value: '11', label: '11 - November' },
    { value: '12', label: '12 - December' }
  ];

  years: number[] = [];

  constructor(
    private fb: FormBuilder,
    private userPaymentService: UserPaymentService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.paymentMethodForm = this.fb.group({
      cardNumber: ['', [Validators.required, Validators.pattern(/^\d{4}\s\d{4}\s\d{4}\s\d{4}$/)]],
      cardholderName: ['', Validators.required],
      expiryMonth: ['', Validators.required],
      expiryYear: ['', Validators.required],
      cvv: ['', [Validators.required, Validators.pattern(/^\d{3,4}$/)]],
      billingAddressLine1: [''],
      billingAddressLine2: [''],
      billingCity: [''],
      billingState: [''],
      billingZipCode: [''],
      billingCountry: ['US'],
      isDefault: [false]
    });

    // Generate years (current year + 10 years)
    const currentYear = new Date().getFullYear();
    for (let i = 0; i < 10; i++) {
      this.years.push(currentYear + i);
    }
  }

  ngOnInit(): void {
    this.loadPaymentMethods();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPaymentMethods(): void {
    this.loading = true;
    this.error = null;

    this.userPaymentService.getUserPaymentMethods()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          if (response.statusCode === 200 && response.data) {
            this.paymentMethods = response.data;
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = error.message || 'Failed to load payment methods';
          this.loading = false;
          this.showNotification('Error loading payment methods', 'error');
        }
      });
  }

  openAddPaymentMethodModal(): void {
    this.editingPaymentMethod = null;
    this.paymentMethodForm.reset({
      billingCountry: 'US',
      isDefault: false
    });
    this.showPaymentMethodModal = true;
  }

  editPaymentMethod(method: PaymentMethod): void {
    this.editingPaymentMethod = method;
    this.paymentMethodForm.patchValue({
      cardNumber: this.maskCardNumber(method.last4),
      cardholderName: method.cardholderName,
      expiryMonth: method.expiryMonth,
      expiryYear: method.expiryYear,
      billingAddressLine1: method.billingAddress?.line1 || '',
      billingAddressLine2: method.billingAddress?.line2 || '',
      billingCity: method.billingAddress?.city || '',
      billingState: method.billingAddress?.state || '',
      billingZipCode: method.billingAddress?.zipCode || '',
      billingCountry: method.billingAddress?.country || 'US',
      isDefault: method.isDefault
    });
    this.showPaymentMethodModal = true;
  }

  closePaymentMethodModal(): void {
    this.showPaymentMethodModal = false;
    this.editingPaymentMethod = null;
    this.paymentMethodForm.reset();
  }

  savePaymentMethod(): void {
    if (this.paymentMethodForm.invalid) {
      return;
    }

    this.saving = true;
    const formValue = this.paymentMethodForm.value;

    const paymentMethodData: CreatePaymentMethodDto = {
      cardNumber: formValue.cardNumber.replace(/\s/g, ''),
      cardholderName: formValue.cardholderName,
      expiryMonth: formValue.expiryMonth,
      expiryYear: formValue.expiryYear,
      cvv: formValue.cvv,
      billingAddress: {
        line1: formValue.billingAddressLine1,
        line2: formValue.billingAddressLine2,
        city: formValue.billingCity,
        state: formValue.billingState,
        zipCode: formValue.billingZipCode,
        country: formValue.billingCountry
      },
      isDefault: formValue.isDefault
    };

    if (this.editingPaymentMethod) {
      // For editing, we need to delete the old one and add a new one
      // since the backend doesn't support updating payment methods
      this.showNotification('Payment method editing is not supported. Please delete and add a new one.', 'error');
      this.saving = false;
      this.closePaymentMethodModal();
    } else {
      // For adding, we need to pass the payment method ID (this would come from Stripe Elements in a real app)
      const paymentMethodId = 'pm_' + Math.random().toString(36).substr(2, 9); // Mock payment method ID
      
      this.userPaymentService.addPaymentMethod(paymentMethodId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response: any) => {
            if (response.statusCode === 200) {
              this.showNotification('Payment method added successfully', 'success');
              this.closePaymentMethodModal();
              this.loadPaymentMethods();
            }
            this.saving = false;
          },
          error: (error: any) => {
            this.showNotification('Failed to add payment method', 'error');
            this.saving = false;
          }
        });
    }
  }

  setAsDefault(paymentMethodId: string): void {
    this.userPaymentService.setDefaultPaymentMethod(paymentMethodId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.showNotification('Default payment method updated', 'success');
            this.loadPaymentMethods();
          }
        },
        error: (error) => {
          this.showNotification('Failed to update default payment method', 'error');
        }
      });
  }

  deletePaymentMethod(paymentMethodId: string): void {
    if (confirm('Are you sure you want to delete this payment method? This action cannot be undone.')) {
      this.userPaymentService.deletePaymentMethod(paymentMethodId)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.showNotification('Payment method deleted successfully', 'success');
              this.loadPaymentMethods();
            }
          },
          error: (error) => {
            this.showNotification('Failed to delete payment method', 'error');
          }
        });
    }
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

  getPaymentTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'visa': 'Visa',
      'mastercard': 'Mastercard',
      'amex': 'American Express',
      'discover': 'Discover',
      'jcb': 'JCB',
      'diners': 'Diners Club'
    };
    return labels[type.toLowerCase()] || 'Credit Card';
  }

  maskCardNumber(last4: string): string {
    return `**** **** **** ${last4}`;
  }

  formatBillingAddress(address: any): string {
    const parts = [
      address.line1,
      address.line2,
      address.city,
      address.state,
      address.zipCode,
      address.country
    ].filter(part => part);
    
    return parts.join(', ');
  }

  showNotification(message: string, type: 'success' | 'error' = 'success'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'success-snackbar' : 'error-snackbar'
    });
  }
}
