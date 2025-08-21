import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './components/layout/admin-layout.component';
import { AdminDashboardComponent } from './components/dashboard/admin-dashboard.component';
import { AdminLoginComponent } from './components/auth/admin-login.component';
import { AdminSignupComponent } from './components/auth/admin-signup.component';
import { SubscriptionListComponent } from './components/subscriptions/subscription-list/subscription-list.component';
import { SubscriptionDetailsComponent } from './components/subscriptions/subscription-details/subscription-details.component';
import { SubscriptionPlansComponent } from './components/subscriptions/subscription-plans/subscription-plans.component';
import { BillingManagementComponent } from './components/billing/billing-management.component';
import { AnalyticsDashboardComponent } from './components/analytics/analytics-dashboard.component';
import { UserManagementComponent } from './components/users/user-management.component';
import { ProviderManagementComponent } from './components/providers/provider-management.component';
import { AdminGuard } from '../core/guards/auth.guard';

export const ADMIN_PORTAL_ROUTES: Routes = [
  { path: 'login', component: AdminLoginComponent },
  { path: 'signup', component: AdminSignupComponent },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [AdminGuard], // Protect all admin routes
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      
      // Subscription Management
      { path: 'subscriptions', component: SubscriptionListComponent },
      { path: 'subscriptions/:id', component: SubscriptionDetailsComponent },
      
      // Subscription Plans Management
      { path: 'subscription-plans', component: SubscriptionPlansComponent },
      { path: 'subscription-plans/create', component: SubscriptionPlansComponent },
      { path: 'subscription-plans/:id', component: SubscriptionPlansComponent },
      { path: 'subscription-plans/:id/edit', component: SubscriptionPlansComponent },
      
      // User Management
      { path: 'users', component: UserManagementComponent },
      { path: 'users/:id', component: UserManagementComponent },
      
      // Provider Management
      { path: 'providers', component: ProviderManagementComponent },
      { path: 'providers/:id', component: ProviderManagementComponent },
      
      // Billing Management
      { path: 'billing', component: BillingManagementComponent },
      
      // Analytics
      { path: 'analytics', component: AnalyticsDashboardComponent },
      
      // Reports
      { path: 'reports', component: SubscriptionListComponent }, // TODO: Replace with actual component
      
      // System Settings
      { path: 'settings', component: SubscriptionListComponent }, // TODO: Replace with actual component
      
      // Privilege Management
      { path: 'privileges', component: SubscriptionListComponent }, // Using existing component for now
      
      // Audit Logs
      { path: 'audit-logs', component: SubscriptionListComponent }, // TODO: Replace with actual component
      
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];
