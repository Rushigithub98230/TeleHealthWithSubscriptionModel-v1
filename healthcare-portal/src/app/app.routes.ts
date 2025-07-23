import { Routes } from '@angular/router';
import { ForbiddenComponent } from './shared/forbidden.component';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/marketing/pages/marketing-home/marketing-home.component').then(m => m.MarketingHomeComponent)
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule)
  },
  {
    path: 'patient',
    loadChildren: () => import('./features/patient/patient.module').then(m => m.PatientModule)
  },
  {
    path: 'provider',
    loadChildren: () => import('./features/provider/provider.module').then(m => m.ProviderModule)
  },
  {
    path: 'forbidden',
    component: ForbiddenComponent
  }
];
