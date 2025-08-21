import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';

import { ADMIN_PORTAL_ROUTES } from './admin-portal.routes';
import { AdminLayoutComponent } from './components/layout/admin-layout.component';
import { AdminDashboardComponent } from './components/dashboard/admin-dashboard.component';
import { AdminLoginComponent } from './components/auth/admin-login.component';
import { AdminSignupComponent } from './components/auth/admin-signup.component';
import { SubscriptionListComponent } from './components/subscriptions/subscription-list/subscription-list.component';
import { SubscriptionDetailsComponent } from './components/subscriptions/subscription-details/subscription-details.component';
import { AdminAuthService } from './services/admin-auth.service';
import { AdminSubscriptionService } from './services/admin-subscription.service';
import { AdminBillingService } from './services/admin-billing.service';
import { AdminAnalyticsService } from './services/admin-analytics.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forChild(ADMIN_PORTAL_ROUTES),
    HttpClientModule,
    ReactiveFormsModule,
    AdminLayoutComponent,
    AdminDashboardComponent,
    AdminLoginComponent,
    AdminSignupComponent,
    SubscriptionListComponent,
    SubscriptionDetailsComponent
  ],
  providers: [
    AdminAuthService,
    AdminSubscriptionService,
    AdminBillingService,
    AdminAnalyticsService
  ]
})
export class AdminPortalModule { }
