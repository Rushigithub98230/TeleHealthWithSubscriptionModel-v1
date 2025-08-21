import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil, switchMap } from 'rxjs';

import { AdminSubscriptionService } from '../../../services/admin-subscription.service';
import { AdminBillingService } from '../../../services/admin-billing.service';
import { JsonModel } from '../../../../core/models/json-model.interface';
import { 
  Subscription, 
  SubscriptionStatus, 
  UpdateSubscriptionDto,
  SubscriptionPlan,
  User,
  SubscriptionStatusHistory,
  SubscriptionPayment
} from '../../../models/subscription.interface';

@Component({
  selector: 'app-subscription-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  template: `
    <div class="subscription-details-container">
      <!-- Header Section -->
      <div class="header-section">
        <div class="breadcrumb">
          <a routerLink="/admin-portal/subscriptions" class="breadcrumb-link">
            <i class="fas fa-arrow-left"></i> Back to Subscriptions
          </a>
        </div>
        <div class="title-section">
          <h1>Subscription Details</h1>
          <p>Manage subscription information and lifecycle</p>
        </div>
        <div class="actions-section">
          <button 
            class="btn btn-outline" 
            (click)="toggleEditMode()"
            [disabled]="loading">
            <i class="fas fa-edit"></i> {{ isEditMode ? 'Cancel Edit' : 'Edit' }}
          </button>
          <button 
            class="btn btn-primary" 
            (click)="saveChanges()"
            *ngIf="isEditMode"
            [disabled]="loading || !editForm.valid">
            <i class="fas fa-save"></i> Save Changes
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-state" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading subscription details...</p>
      </div>

      <!-- Error State -->
      <div class="error-state" *ngIf="error && !loading">
        <i class="fas fa-exclamation-triangle"></i>
        <h3>Error Loading Subscription</h3>
        <p>{{ error }}</p>
        <button class="btn btn-primary" (click)="loadSubscription()">
          <i class="fas fa-redo"></i> Retry
        </button>
      </div>

      <!-- Subscription Content -->
      <div class="subscription-content" *ngIf="subscription && !loading">
        <!-- Status Banner -->
        <div class="status-banner" [class]="'status-' + subscription.status.toLowerCase()">
          <div class="status-info">
            <span class="status-badge">{{ subscription.status }}</span>
            <span class="status-description">{{ getStatusDescription(subscription.status) }}</span>
          </div>
          <div class="status-actions" *ngIf="!isEditMode">
            <button 
              class="btn btn-success" 
              (click)="activateSubscription()"
              *ngIf="subscription.status !== 'Active'"
              [disabled]="loading">
              <i class="fas fa-play"></i> Activate
            </button>
            <button 
              class="btn btn-warning" 
              (click)="pauseSubscription()"
              *ngIf="subscription.status === 'Active'"
              [disabled]="loading">
              <i class="fas fa-pause"></i> Pause
            </button>
            <button 
              class="btn btn-danger" 
              (click)="cancelSubscription()"
              *ngIf="subscription.status !== 'Cancelled'"
              [disabled]="loading">
              <i class="fas fa-stop"></i> Cancel
            </button>
            <button 
              class="btn btn-info" 
              (click)="renewSubscription()"
              *ngIf="subscription.status === 'Expired'"
              [disabled]="loading">
              <i class="fas fa-redo"></i> Renew
            </button>
          </div>
        </div>

        <!-- Main Content Grid -->
        <div class="content-grid">
          <!-- Left Column - Subscription Details -->
          <div class="left-column">
            <!-- Basic Information -->
            <div class="info-card">
              <h3>Basic Information</h3>
              <div class="info-content">
                <div class="info-row">
                  <label>Subscription ID:</label>
                  <span>{{ subscription.id }}</span>
                </div>
                <div class="info-row">
                  <label>Status:</label>
                  <span class="status-badge" [class]="'status-' + subscription.status.toLowerCase()">
                    {{ subscription.status }}
                  </span>
                </div>
                <div class="info-row">
                  <label>Start Date:</label>
                  <span>{{ subscription.startDate | date:'MMM dd, yyyy' }}</span>
                </div>
                <div class="info-row">
                  <label>End Date:</label>
                  <span>{{ subscription.endDate ? (subscription.endDate | date:'MMM dd, yyyy') : 'N/A' }}</span>
                </div>
                <div class="info-row">
                  <label>Next Billing:</label>
                  <span>{{ subscription.nextBillingDate | date:'MMM dd, yyyy' }}</span>
                </div>
                <div class="info-row">
                  <label>Auto Renew:</label>
                  <span [class]="subscription.autoRenew ? 'text-success' : 'text-danger'">
                    {{ subscription.autoRenew ? 'Yes' : 'No' }}
                  </span>
                </div>
              </div>
            </div>

            <!-- User Information -->
            <div class="info-card">
              <h3>User Information</h3>
              <div class="info-content">
                <div class="info-row">
                  <label>User ID:</label>
                  <span>{{ subscription.userId }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.user">
                  <label>Full Name:</label>
                                     <span>{{ (subscription.user?.firstName || '') + ' ' + (subscription.user?.lastName || '') || 'N/A' }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.user">
                  <label>Email:</label>
                  <span>{{ subscription.user.email || 'N/A' }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.user">
                  <label>Phone:</label>
                  <span>{{ subscription.user.phoneNumber || 'N/A' }}</span>
                </div>
              </div>
            </div>

            <!-- Plan Information -->
            <div class="info-card">
              <h3>Plan Information</h3>
              <div class="info-content">
                <div class="info-row">
                  <label>Plan ID:</label>
                  <span>{{ subscription.subscriptionPlanId }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.subscriptionPlan">
                  <label>Plan Name:</label>
                  <span>{{ subscription.subscriptionPlan.name }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.subscriptionPlan">
                  <label>Plan Type:</label>
                  <span>{{ subscription.subscriptionPlan.billingCycle?.name || 'N/A' }}</span>
                </div>
                <div class="info-row" *ngIf="subscription.subscriptionPlan">
                  <label>Billing Cycle:</label>
                  <span>{{ subscription.subscriptionPlan.billingCycle?.name || 'N/A' }}</span>
                </div>
                <div class="info-row">
                  <label>Current Price:</label>
                  <span class="price">{{ subscription.subscriptionPlan?.currency?.code || 'USD' }} {{ subscription.currentPrice | number:'1.2-2' }}</span>
                </div>
              </div>
            </div>

            <!-- Edit Form (Hidden by default) -->
            <div class="info-card edit-form" *ngIf="isEditMode">
              <h3>Edit Subscription</h3>
              <form [formGroup]="editForm" class="form">
                <div class="form-group">
                  <label for="status">Status</label>
                  <select id="status" formControlName="status" class="form-control">
                    <option *ngFor="let status of availableStatuses" [value]="status">
                      {{ status }}
                    </option>
                  </select>
                </div>
                <div class="form-group">
                  <label for="startDate">Start Date</label>
                  <input type="date" id="startDate" formControlName="startDate" class="form-control">
                </div>
                <div class="form-group">
                  <label for="endDate">End Date</label>
                  <input type="date" id="endDate" formControlName="endDate" class="form-control">
                </div>
                <div class="form-group">
                  <label for="nextBillingDate">Next Billing Date</label>
                  <input type="date" id="nextBillingDate" formControlName="nextBillingDate" class="form-control">
                </div>
                <div class="form-group">
                  <label for="currentPrice">Current Price</label>
                  <input type="number" id="currentPrice" formControlName="currentPrice" class="form-control" step="0.01">
                </div>
                <div class="form-group">
                  <label for="autoRenew">Auto Renew</label>
                  <select id="autoRenew" formControlName="autoRenew" class="form-control">
                    <option [value]="true">Yes</option>
                    <option [value]="false">No</option>
                  </select>
                </div>
                <div class="form-group">
                  <label for="notes">Notes</label>
                  <textarea id="notes" formControlName="notes" class="form-control" rows="3"></textarea>
                </div>
              </form>
            </div>
          </div>

          <!-- Right Column - Additional Information -->
          <div class="right-column">
            <!-- Payment Information -->
            <div class="info-card">
              <h3>Payment Information</h3>
              <div class="info-content">
                <div class="info-row">
                  <label>Last Payment:</label>
                  <span>{{ getLastPaymentInfo() }}</span>
                </div>
                <div class="info-row">
                  <label>Payment Method:</label>
                  <span>{{ getPaymentMethodInfo() }}</span>
                </div>
                <div class="info-row">
                  <label>Next Payment:</label>
                  <span>{{ subscription.nextBillingDate | date:'MMM dd, yyyy' }}</span>
                </div>
              </div>
              <div class="card-actions">
                <button class="btn btn-outline" (click)="viewPaymentHistory()">
                  <i class="fas fa-history"></i> Payment History
                </button>
              </div>
            </div>

            <!-- Usage Statistics -->
            <div class="info-card">
              <h3>Usage Statistics</h3>
              <div class="info-content">
                <div class="info-row">
                  <label>Teleconsultations Used:</label>
                  <span>{{ getUsageInfo('teleconsultation') }}</span>
                </div>
                <div class="info-row">
                  <label>Medications Delivered:</label>
                  <span>{{ getUsageInfo('medication') }}</span>
                </div>
                <div class="info-row">
                  <label>Lab Tests:</label>
                  <span>{{ getUsageInfo('labTest') }}</span>
                </div>
              </div>
              <div class="card-actions">
                <button class="btn btn-outline" (click)="viewUsageDetails()">
                  <i class="fas fa-chart-bar"></i> Usage Details
                </button>
              </div>
            </div>

            <!-- Recent Activity -->
            <div class="info-card">
              <h3>Recent Activity</h3>
              <div class="activity-list">
                <div class="activity-item" *ngFor="let activity of recentActivities">
                  <div class="activity-icon">
                    <i [class]="getActivityIcon(activity.type)"></i>
                  </div>
                  <div class="activity-content">
                    <div class="activity-title">{{ activity.title }}</div>
                    <div class="activity-time">{{ activity.timestamp | date:'MMM dd, yyyy HH:mm' }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Bottom Section - History and Actions -->
        <div class="bottom-section">
          <!-- Status History -->
          <div class="history-section">
            <h3>Status History</h3>
            <div class="timeline">
              <div class="timeline-item" *ngFor="let history of statusHistory">
                                 <div class="timeline-marker" [class]="'status-' + history.toStatus.toLowerCase()"></div>
                <div class="timeline-content">
                  <div class="timeline-header">
                                         <span class="status">{{ history.toStatus }}</span>
                                         <span class="date">{{ history.changedAt | date:'MMM dd, yyyy HH:mm' }}</span>
                  </div>
                  <div class="timeline-description">{{ history.reason || 'No reason provided' }}</div>
                                     <div class="timeline-user">Changed by: {{ history.changedByUser?.firstName + ' ' + history.changedByUser?.lastName || 'System' }}</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="quick-actions">
            <h3>Quick Actions</h3>
            <div class="action-buttons">
              <button class="btn btn-outline" (click)="generateInvoice()">
                <i class="fas fa-file-invoice"></i> Generate Invoice
              </button>
              <button class="btn btn-outline" (click)="sendReminder()">
                <i class="fas fa-bell"></i> Send Reminder
              </button>
              <button class="btn btn-outline" (click)="exportData()">
                <i class="fas fa-download"></i> Export Data
              </button>
              <button class="btn btn-outline" (click)="duplicateSubscription()">
                <i class="fas fa-copy"></i> Duplicate
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./subscription-details.component.scss']
})
export class SubscriptionDetailsComponent implements OnInit, OnDestroy {
  // Data properties
  subscription: Subscription | null = null;
  statusHistory: SubscriptionStatusHistory[] = [];
  recentActivities: any[] = [];
  
  // Form properties
  editForm!: FormGroup;
  isEditMode = false;
  
  // State properties
  loading = false;
  error: string | null = null;
  
  // Available statuses for editing
  availableStatuses: SubscriptionStatus[] = [
    'Pending', 'Active', 'Paused', 'Cancelled', 'Expired', 
    'PaymentFailed', 'TrialActive', 'TrialExpired', 'Suspended'
  ];
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private subscriptionService: AdminSubscriptionService,
    private billingService: AdminBillingService
  ) {
    this.initializeEditForm();
  }

  ngOnInit(): void {
    this.loadSubscription();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Initialize edit form
  private initializeEditForm(): void {
    this.editForm = this.fb.group({
      status: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: [''],
      nextBillingDate: ['', Validators.required],
      currentPrice: ['', [Validators.required, Validators.min(0)]],
      autoRenew: [true],
      notes: ['']
    });
  }

  // Load subscription data
  loadSubscription(): void {
    this.loading = true;
    this.error = null;

    this.route.params.pipe(
      switchMap(params => this.subscriptionService.getSubscriptionById(params['id'])),
      takeUntil(this.destroy$)
    ).subscribe({
              next: (response) => {
          if (response.statusCode === 200) {
            this.subscription = response.data;
            this.loadStatusHistory();
            this.loadRecentActivities();
            this.populateEditForm();
          } else {
            this.error = response.message || 'Failed to load subscription';
          }
          this.loading = false;
        },
      error: (error) => {
        this.error = 'An error occurred while loading the subscription';
        this.loading = false;
        console.error('Error loading subscription:', error);
      }
    });
  }

  // Load status history
  loadStatusHistory(): void {
    if (!this.subscription?.id) return;

            this.subscriptionService.getStatusHistory(this.subscription.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<SubscriptionStatusHistory[]>) => {
          if (response.statusCode === 200) {
            this.statusHistory = response.data || [];
          }
        },
        error: (error: any) => {
          console.error('Error loading status history:', error);
        }
      });
  }

  // Load recent activities
  loadRecentActivities(): void {
    // Mock data for now - would come from a real service
    this.recentActivities = [
      {
        type: 'status_change',
        title: 'Status changed to Active',
        timestamp: new Date()
      },
      {
        type: 'payment',
        title: 'Payment received',
        timestamp: new Date(Date.now() - 86400000)
      },
      {
        type: 'usage',
        title: 'Teleconsultation used',
        timestamp: new Date(Date.now() - 172800000)
      }
    ];
  }

  // Populate edit form with current values
  private populateEditForm(): void {
    if (!this.subscription) return;

    this.editForm.patchValue({
      status: this.subscription.status,
      startDate: this.formatDateForInput(this.subscription.startDate),
      endDate: this.subscription.endDate ? this.formatDateForInput(this.subscription.endDate) : '',
      nextBillingDate: this.formatDateForInput(this.subscription.nextBillingDate),
      currentPrice: this.subscription.currentPrice,
      autoRenew: this.subscription.autoRenew,
      notes: this.subscription.notes || ''
    });
  }

  // Format date for input field
  private formatDateForInput(date: Date): string {
    return new Date(date).toISOString().split('T')[0];
  }

  // Toggle edit mode
  toggleEditMode(): void {
    if (this.isEditMode) {
      this.populateEditForm(); // Reset form to current values
    }
    this.isEditMode = !this.isEditMode;
  }

  // Save changes
  saveChanges(): void {
    if (!this.editForm.valid || !this.subscription) return;

    this.loading = true;
    const updateDto: UpdateSubscriptionDto = {
      id: this.subscription.id,
      status: this.editForm.get('status')?.value,
      endDate: this.editForm.get('endDate')?.value ? new Date(this.editForm.get('endDate')?.value) : undefined,
      nextBillingDate: new Date(this.editForm.get('nextBillingDate')?.value),
      currentPrice: this.editForm.get('currentPrice')?.value,
      autoRenew: this.editForm.get('autoRenew')?.value,
      notes: this.editForm.get('notes')?.value
    };

    this.subscriptionService.updateSubscription(this.subscription.id, updateDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.subscription = { ...this.subscription, ...response.data };
            this.isEditMode = false;
            this.loadStatusHistory();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to update subscription:', response.Message);
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error updating subscription:', error);
          this.loading = false;
          // TODO: Show error notification
        }
      });
  }

  // Subscription lifecycle methods
  activateSubscription(): void {
    if (!this.subscription?.id) return;

    this.subscriptionService.activateSubscription(this.subscription.id, 'Activated by admin')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.StatusCode === 200) {
            this.loadSubscription();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to activate subscription:', response.Message);
          }
        },
        error: (error) => {
          console.error('Error activating subscription:', error);
          // TODO: Show error notification
        }
      });
  }

  pauseSubscription(): void {
    if (!this.subscription?.id) return;

    this.subscriptionService.pauseSubscription(this.subscription.id, 'Paused by admin')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.StatusCode === 200) {
            this.loadSubscription();
            // TODO: Show success notification
          } else {
            // TODO: Show error notification
            console.error('Failed to pause subscription:', response.Message);
          }
        },
        error: (error) => {
          console.error('Error pausing subscription:', error);
          // TODO: Show error notification
        }
      });
  }

  cancelSubscription(): void {
    if (!this.subscription?.id) return;

    if (confirm('Are you sure you want to cancel this subscription? This action cannot be undone.')) {
      this.subscriptionService.cancelSubscription(this.subscription.id, 'Cancelled by admin')
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.StatusCode === 200) {
              this.loadSubscription();
              // TODO: Show success notification
            } else {
              // TODO: Show error notification
              console.error('Failed to cancel subscription:', response.Message);
            }
          },
          error: (error) => {
            console.error('Error cancelling subscription:', error);
            // TODO: Show error notification
          }
        });
    }
  }

  renewSubscription(): void {
    // TODO: Implement renewal logic
    console.log('Renew subscription');
  }

  // Utility methods
  getStatusDescription(status: SubscriptionStatus): string {
    const descriptions: { [key: string]: string } = {
      'Pending': 'Subscription is pending activation',
      'Active': 'Subscription is currently active and providing services',
      'Paused': 'Subscription is temporarily paused',
      'Cancelled': 'Subscription has been cancelled',
      'Expired': 'Subscription has expired and needs renewal',
      'PaymentFailed': 'Payment failed, subscription suspended',
      'TrialActive': 'Trial period is active',
      'TrialExpired': 'Trial period has expired',
      'Suspended': 'Subscription is suspended due to issues'
    };
    return descriptions[status] || 'Unknown status';
  }

  getLastPaymentInfo(): string {
    // TODO: Implement real payment info
    return 'N/A';
  }

  getPaymentMethodInfo(): string {
    // TODO: Implement real payment method info
    return 'N/A';
  }

  getUsageInfo(type: string): string {
    // TODO: Implement real usage info
    return '0 / Unlimited';
  }

  getActivityIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'status_change': 'fas fa-exchange-alt',
      'payment': 'fas fa-credit-card',
      'usage': 'fas fa-chart-line'
    };
    return icons[type] || 'fas fa-info-circle';
  }

  // Action methods
  viewPaymentHistory(): void {
    // TODO: Implement payment history view
    console.log('View payment history');
  }

  viewUsageDetails(): void {
    // TODO: Implement usage details view
    console.log('View usage details');
  }

  generateInvoice(): void {
    // TODO: Implement invoice generation
    console.log('Generate invoice');
  }

  sendReminder(): void {
    // TODO: Implement reminder sending
    console.log('Send reminder');
  }

  exportData(): void {
    // TODO: Implement data export
    console.log('Export data');
  }

  duplicateSubscription(): void {
    // TODO: Implement subscription duplication
    console.log('Duplicate subscription');
  }
}
