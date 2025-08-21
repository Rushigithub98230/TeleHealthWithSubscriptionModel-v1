import { Routes } from '@angular/router';
import { UserDashboardComponent } from './components/dashboard/user-dashboard.component';
import { UserSubscriptionComponent } from './components/subscription/user-subscription.component';
import { UserBillingComponent } from './components/billing/user-billing.component';
import { UserPaymentMethodsComponent } from './components/payment-methods/user-payment-methods.component';
import { UserSubscriptionPlansComponent } from './components/subscription-plans/user-subscription-plans.component';
import { UserLoginComponent } from './components/user-login/user-login.component';
import { UserGuard } from '../core/guards/auth.guard';

export const USER_PORTAL_ROUTES: Routes = [
  { path: 'login', component: UserLoginComponent }, // Public login route
  {
    path: '',
    canActivate: [UserGuard], // Protect all user routes
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: UserDashboardComponent,
        data: { title: 'Dashboard' }
      },
      // Subscription Management Routes
      {
        path: 'subscriptions',
        component: UserSubscriptionComponent,
        data: { title: 'My Subscriptions' }
      },
      {
        path: 'subscription-plans',
        component: UserSubscriptionPlansComponent,
        data: { title: 'Subscription Plans' }
      },
      // Billing Management Routes
      {
        path: 'billing',
        component: UserBillingComponent,
        data: { title: 'Billing & Payments' }
      },
      // Payment Management Routes
      {
        path: 'payment-methods',
        component: UserPaymentMethodsComponent,
        data: { title: 'Payment Methods' }
      },
      // Catch all route - redirect to dashboard
      {
        path: '**',
        redirectTo: 'dashboard'
      }
    ]
  }
];
