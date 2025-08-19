import { Routes } from '@angular/router';
import { SubscriptionPlansComponent } from './subscription-plans/subscription-plans.component';
import { SubscriptionListComponent } from './subscription-list/subscription-list.component';
import { PrivilegeManagementComponent } from './privilege-management/privilege-management.component';

export const SUBSCRIPTION_ROUTES: Routes = [
  { path: 'plans', component: SubscriptionPlansComponent },
  { path: 'list', component: SubscriptionListComponent },
  { path: 'privileges', component: PrivilegeManagementComponent },
  { path: '', redirectTo: 'plans', pathMatch: 'full' }
];
