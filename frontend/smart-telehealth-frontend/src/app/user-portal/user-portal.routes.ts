import { Routes } from '@angular/router';
import { UserLoginComponent } from './components/user-login/user-login.component';
import { UserSignupComponent } from './components/user-signup/user-signup.component';
import { UserDashboardComponent } from './components/dashboard/user-dashboard.component';

export const USER_ROUTES: Routes = [
  { path: 'login', component: UserLoginComponent },
  { path: 'signup', component: UserSignupComponent },
  { 
    path: 'dashboard', 
    component: UserDashboardComponent
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
