import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProviderApiService, Provider } from '../../../../core/provider-api.service';
import { AuthService } from '../../../../core/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ProfileComponent } from '../../../../shared/profile.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';

function decodeJwt(token: string): any {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatSnackBarModule, MatDialogModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  providers: Provider[] = [];
  loading = false;
  error: string | null = null;
  userName: string | null = null;
  userRole: string | null = null;
  avatarUrl: string | null = null;

  constructor(
    private providerApi: ProviderApiService,
    private auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.fetchProviders();
    this.setUserInfo();
  }

  setUserInfo() {
    const token = localStorage.getItem('token');
    if (token) {
      const decoded = decodeJwt(token);
      this.userName = decoded?.name || decoded?.email || 'Unknown';
      this.userRole = decoded?.role || decoded?.roles || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Provider';
      this.avatarUrl = decoded?.avatarUrl || null;
      if (Array.isArray(this.userRole)) {
        this.userRole = this.userRole.join(', ');
      }
    }
  }

  openProfileDialog() {
    this.dialog.open(ProfileComponent, { width: '420px' });
  }

  fetchProviders() {
    this.loading = true;
    this.providerApi.getAllProviders().subscribe({
      next: (providers: Provider[]) => {
        this.providers = providers;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load providers';
        this.snackBar.open('Failed to load providers', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  addProvider() {
    const name = window.prompt('Enter provider name:');
    if (!name) return;
    const email = window.prompt('Enter provider email:');
    if (!email) return;
    this.loading = true;
    this.providerApi.createProvider({ name, email }).subscribe({
      next: (provider: Provider) => {
        this.providers.push(provider);
        this.snackBar.open('Provider added successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to add provider';
        this.snackBar.open('Failed to add provider', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  editProvider(provider: Provider) {
    const name = window.prompt('Edit provider name:', provider.name);
    if (!name) return;
    const email = window.prompt('Edit provider email:', provider.email);
    if (!email) return;
    this.loading = true;
    this.providerApi.updateProvider(provider.id, { name, email }).subscribe({
      next: (updated: Provider) => {
        this.providers = this.providers.map(p => p.id === provider.id ? updated : p);
        this.snackBar.open('Provider updated successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to update provider';
        this.snackBar.open('Failed to update provider', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  deleteProvider(provider: Provider) {
    if (!window.confirm(`Are you sure you want to delete provider ${provider.name}?`)) {
      return;
    }
    this.loading = true;
    this.providerApi.deleteProvider(provider.id).subscribe({
      next: () => {
        this.providers = this.providers.filter(p => p.id !== provider.id);
        this.snackBar.open('Provider deleted successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to delete provider';
        this.snackBar.open('Failed to delete provider', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/provider/login']);
      },
      error: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/provider/login']);
      }
    });
  }
}
