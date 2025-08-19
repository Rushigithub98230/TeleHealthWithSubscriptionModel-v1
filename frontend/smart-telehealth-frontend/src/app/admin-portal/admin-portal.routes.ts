import { Routes } from '@angular/router';
import { AdminLoginComponent } from './components/auth/admin-login.component';
import { AdminSignupComponent } from './components/auth/admin-signup.component';
import { AdminForgotPasswordComponent } from './components/auth/admin-forgot-password.component';
import { AdminDashboardComponent } from './components/dashboard/admin-dashboard.component';

export const ADMIN_ROUTES: Routes = [
  { path: 'login', component: AdminLoginComponent },
  { path: 'signup', component: AdminSignupComponent },
  { path: 'forgot-password', component: AdminForgotPasswordComponent },
  { 
    path: 'dashboard', 
    component: AdminDashboardComponent
  },
  {
    path: 'subscriptions',
    loadChildren: () => import('./components/subscriptions/subscriptions.routes').then(m => m.SUBSCRIPTION_ROUTES)
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
