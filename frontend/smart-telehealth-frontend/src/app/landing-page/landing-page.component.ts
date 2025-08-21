import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, takeUntil } from 'rxjs';
import { SubscriptionPlan } from '../user-portal/models/subscription.interface';
import { CommonService } from '../core/services/common.service';
import { JsonModel } from '../core/models/json-model.interface';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class LandingPageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  subscriptionPlans: SubscriptionPlan[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private router: Router,
    private commonService: CommonService
  ) {}

  ngOnInit(): void {
    this.loadSubscriptionPlans();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSubscriptionPlans(): void {
    this.loading = true;
    this.error = null;

    // Get active subscription plans for public display
    this.commonService.get<SubscriptionPlan[]>('/api/Subscriptions/plans/public')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: JsonModel<SubscriptionPlan[]>) => {
          if (response.statusCode === 200 && response.data) {
            this.subscriptionPlans = response.data;
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

  navigateToAdminPortal(): void {
    this.router.navigate(['/webadmin/login']);
  }

  navigateToUserPortal(): void {
    this.router.navigate(['/web/login']);
  }

  navigateToSubscriptionPlans(): void {
    this.router.navigate(['/web/subscription-plans']);
  }

  getDiscountPercentage(originalPrice: number, discountedPrice: number): number {
    return Math.round(((originalPrice - discountedPrice) / originalPrice) * 100);
  }

  getPrivilegeUnit(privilegeName: string): string {
    const units: { [key: string]: string } = {
      'Teleconsultation': 'consultations',
      'Medical Records': 'records',
      'Prescriptions': 'prescriptions',
      'Lab Tests': 'tests',
      'Video Calls': 'calls',
      'Chat Support': 'chats'
    };
    return units[privilegeName] || 'units';
  }
}
