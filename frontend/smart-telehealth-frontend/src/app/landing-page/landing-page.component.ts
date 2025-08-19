import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class LandingPageComponent {
  constructor(private router: Router) {}

  navigateToAdminPortal(): void {
    this.router.navigate(['/admin/login']);
  }

  navigateToUserPortal(): void {
    this.router.navigate(['/user/login']);
  }
}
