import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/auth.guard';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
// Import other provider components as needed

const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
    data: { roles: ['Provider'] }
  },
  // Add other provider routes here, all protected by authGuard and role-based access
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProviderRoutingModule { }
