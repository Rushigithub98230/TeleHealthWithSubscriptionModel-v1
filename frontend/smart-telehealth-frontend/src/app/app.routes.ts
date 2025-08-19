import { Routes } from '@angular/router';
import { LandingPageComponent } from './landing-page/landing-page.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'admin', loadChildren: () => import('./admin-portal/admin-portal.routes').then(m => m.ADMIN_ROUTES) },
  { path: 'user', loadChildren: () => import('./user-portal/user-portal.routes').then(m => m.USER_ROUTES) },
  { path: '**', redirectTo: '' }
];
