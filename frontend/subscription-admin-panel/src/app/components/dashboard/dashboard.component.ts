import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SubscriptionService } from '../../services/subscription.service';
import { AuthService } from '../../services/auth.service';
import { DashboardStats, Subscription, SubscriptionPlan } from '../../models/subscription.model';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatMenuModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboardStats$: Observable<DashboardStats>;
  subscriptions$: Observable<Subscription[]>;
  plans$: Observable<SubscriptionPlan[]>;
  currentUser$: Observable<any>;

  constructor(
    private subscriptionService: SubscriptionService,
    private authService: AuthService
  ) {
    this.dashboardStats$ = this.subscriptionService.getDashboardStats();
    this.subscriptions$ = this.subscriptionService.getAllSubscriptions();
    this.plans$ = this.subscriptionService.getAllPlans();
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Component initialization
  }

  logout(): void {
    this.authService.logout();
  }

  // Placeholder methods for actions
  createNewSubscription(): void {
    console.log('Create new subscription');
  }

  createNewPlan(): void {
    console.log('Create new plan');
  }

  triggerBilling(): void {
    console.log('Trigger billing');
  }

  viewAnalytics(): void {
    console.log('View analytics');
  }

  viewSubscription(id: string): void {
    console.log('View subscription:', id);
  }

  pauseSubscription(id: string): void {
    console.log('Pause subscription:', id);
  }

  resumeSubscription(id: string): void {
    console.log('Resume subscription:', id);
  }

  cancelSubscription(id: string): void {
    console.log('Cancel subscription:', id);
  }

  editPlan(id: string): void {
    console.log('Edit plan:', id);
  }

  deletePlan(id: string): void {
    console.log('Delete plan:', id);
  }
}
