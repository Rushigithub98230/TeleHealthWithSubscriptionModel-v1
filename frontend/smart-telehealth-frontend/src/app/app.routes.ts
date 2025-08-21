import { Routes } from '@angular/router';
import { LandingPageComponent } from './landing-page/landing-page.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  
  // Admin Portal Routes (/webadmin/*)
  { 
    path: 'webadmin', 
    loadChildren: () => import('./admin-portal/admin-portal.routes').then(m => m.ADMIN_PORTAL_ROUTES) 
  },
  
  // User & Provider Portal Routes (/web/*)
  { 
    path: 'web', 
    loadChildren: () => import('./user-portal/user-portal.routes').then(m => m.USER_PORTAL_ROUTES) 
  },
  
  { path: '**', redirectTo: '' }
];
