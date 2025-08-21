 import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { UserSubscriptionService } from '../../services/user-subscription.service';
import { UserBillingService } from '../../services/user-billing.service';
import { AuthService } from '../../../core/services/auth.service';
import { Subscription, SubscriptionPlan, SubscriptionStatus } from '../../models/subscription.interface';

@Component({
  selector: 'app-user-subscription',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './user-subscription.component.html',
  styleUrls: ['./user-subscription.component.scss']
})
export class UserSubscriptionComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  subscriptions: Subscription[] = [];
  loading = false;
  error: string | null = null;

  // Forms
  filterForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userSubscriptionService: UserSubscriptionService,
    private userBillingService: UserBillingService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.filterForm = this.fb.group({
      status: [''],
      planType: ['']
    });
  }

  ngOnInit(): void {
    this.loadSubscriptions();
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

  loadSubscriptions(): void {
    this.loading = true;
    this.error = null;

    this.userSubscriptionService.getUserSubscriptions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200 && response.data) {
            this.subscriptions = response.data;
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = error.message || 'Failed to load subscriptions';
          this.loading = false;
          this.showNotification('Error loading subscriptions', 'error');
        }
      });
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    // Apply filters to subscriptions
    // This would typically involve calling the service with filter parameters
  }

  browsePlans(): void {
    this.router.navigate(['/web/subscription-plans']);
  }

  navigateToBilling(): void {
    this.router.navigate(['/web/billing']);
  }

  viewDetails(subscriptionId: string): void {
    this.router.navigate(['/web/subscriptions', subscriptionId]);
  }

  manageBilling(subscriptionId: string): void {
    this.router.navigate(['/web/billing', subscriptionId]);
  }



  getStatusLabel(status: SubscriptionStatus): string {
    const statusLabels: { [key: string]: string } = {
      'Active': 'Active',
      'Paused': 'Paused',
      'Cancelled': 'Cancelled',
      'Expired': 'Expired',
      'Suspended': 'Suspended',
      'TrialActive': 'Trial',
      'TrialExpired': 'Trial Expired',
      'PaymentFailed': 'Payment Failed'
    };
    return statusLabels[status] || status;
  }

  getBillingCycleLabel(billingCycleId: number): string {
    const cycles: { [key: number]: string } = {
      1: 'month',
      3: 'quarter',
      12: 'year'
    };
    return cycles[billingCycleId] || 'period';
  }

  getActiveSubscriptionsCount(): number {
    return this.subscriptions.filter(s => s.status === 'Active').length;
  }

  getTotalMonthlyCost(): number {
    return this.subscriptions
      .filter(s => s.status === 'Active')
      .reduce((total, s) => total + (s.currentPrice || 0), 0);
  }

  getNextPaymentDate(): Date | null {
    const activeSubscriptions = this.subscriptions.filter(s => s.status === 'Active');
    if (activeSubscriptions.length === 0) return null;
    
    return activeSubscriptions.reduce((earliest: Date | null, current: Subscription) => {
      if (!earliest) return current.nextBillingDate || null;
      if (!current.nextBillingDate) return earliest;
      return current.nextBillingDate < earliest ? current.nextBillingDate : earliest;
    }, null as Date | null);
  }

  // Additional methods needed by the HTML template
  navigateToSubscriptionPlans(): void {
    this.router.navigate(['/web/subscription-plans']);
  }

  getTrialSubscriptionsCount(): number {
    return this.subscriptions.filter(s => s.status === 'TrialActive').length;
  }

  viewSubscriptionDetails(subscription: Subscription): void {
    this.router.navigate(['/web/subscriptions', subscription.id]);
  }

  pauseSubscription(subscription: Subscription): void {
    if (confirm('Are you sure you want to pause this subscription?')) {
      this.userSubscriptionService.pauseSubscription(subscription.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.showNotification('Subscription paused successfully', 'success');
              this.loadSubscriptions();
            }
          },
          error: (error) => {
            this.showNotification('Failed to pause subscription', 'error');
          }
        });
    }
  }

  resumeSubscription(subscription: Subscription): void {
    if (confirm('Are you sure you want to resume this subscription?')) {
      this.userSubscriptionService.resumeSubscription(subscription.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.showNotification('Subscription resumed successfully', 'success');
              this.loadSubscriptions();
            }
          },
          error: (error) => {
            this.showNotification('Failed to resume subscription', 'error');
          }
        });
    }
  }

  cancelSubscription(subscription: Subscription): void {
    const reason = prompt('Please provide a reason for cancellation:');
    if (reason && confirm('Are you sure you want to cancel this subscription? This action cannot be undone.')) {
      this.userSubscriptionService.cancelSubscription(subscription.id, reason)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.showNotification('Subscription cancelled successfully', 'success');
              this.loadSubscriptions();
            }
          },
          error: (error) => {
            this.showNotification('Failed to cancel subscription', 'error');
          }
        });
    }
  }

  getPrivilegeUnit(privilegeName: string): string {
    const units: { [key: string]: string } = {
      'consultations': 'consultations',
      'prescriptions': 'prescriptions',
      'lab_tests': 'tests',
      'video_calls': 'calls',
      'messages': 'messages',
      'file_uploads': 'uploads',
      'priority_support': 'priority'
    };
    return units[privilegeName] || 'units';
  }

  showNotification(message: string, type: 'success' | 'error' = 'success'): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: type === 'success' ? 'success-snackbar' : 'error-snackbar'
    });
  }
}
