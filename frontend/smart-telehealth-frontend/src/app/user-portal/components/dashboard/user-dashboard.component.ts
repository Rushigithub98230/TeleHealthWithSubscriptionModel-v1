import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-dashboard',
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class UserDashboardComponent implements OnInit {
  currentUser: any;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser || !this.authService.isUser()) {
      this.router.navigate(['/user/login']);
    }
  }

  logout(): void {
    this.authService.logout();
  }

  navigateToAppointments(): void {
    this.router.navigate(['/user/appointments']);
  }

  navigateToMedicalRecords(): void {
    this.router.navigate(['/user/medical-records']);
  }

  navigateToConsultations(): void {
    this.router.navigate(['/user/consultations']);
  }

  navigateToBilling(): void {
    this.router.navigate(['/user/billing']);
  }
}
