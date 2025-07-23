import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/auth.guard';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
// Import other patient components as needed

const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
    data: { roles: ['Patient'] }
  },
  // Add other patient routes here, all protected by authGuard and role-based access
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PatientRoutingModule { }
