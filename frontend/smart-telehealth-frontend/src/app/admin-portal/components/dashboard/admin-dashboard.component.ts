import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class AdminDashboardComponent implements OnInit {
  currentUser: any;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser || !this.authService.isAdmin()) {
      this.router.navigate(['/admin/login']);
    }
  }

  logout(): void {
    this.authService.logout();
  }

  navigateToSubscriptions(): void {
    this.router.navigate(['/admin/subscriptions']);
  }

  navigateToUsers(): void {
    // TODO: Implement user management route
    this.router.navigate(['/admin/dashboard']);
  }

  navigateToPrivileges(): void {
    this.router.navigate(['/admin/subscriptions/privileges']);
  }
}
