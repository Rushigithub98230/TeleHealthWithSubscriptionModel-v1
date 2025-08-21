import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="admin-login-container">
      <div class="login-card">
        <div class="login-header">
          <div class="logo">
            <i class="fas fa-heartbeat"></i>
            <h1>Smart Telehealth</h1>
          </div>
          <p class="subtitle">Admin Portal</p>
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
          <div class="form-group">
            <label for="email">Email Address</label>
            <input
              type="email"
              id="email"
              formControlName="email"
              placeholder="Enter your email"
              [class.error]="isFieldInvalid('email')"
            />
            <div *ngIf="isFieldInvalid('email')" class="error-message">
              <span *ngIf="loginForm.get('email')?.errors?.['required']">Email is required</span>
              <span *ngIf="loginForm.get('email')?.errors?.['email']">Please enter a valid email</span>
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              type="password"
              id="password"
              formControlName="password"
              placeholder="Enter your password"
              [class.error]="isFieldInvalid('password')"
            />
            <div *ngIf="isFieldInvalid('password')" class="error-message">
              <span *ngIf="loginForm.get('password')?.errors?.['required']">Password is required</span>
            </div>
          </div>

          <div class="form-group checkbox-group">
            <label class="checkbox-label">
              <input type="checkbox" formControlName="rememberMe" />
              <span class="checkmark"></span>
              Remember me
            </label>
          </div>

          <button 
            type="submit" 
            class="login-btn"
            [disabled]="loginForm.invalid || loading"
          >
            <span *ngIf="!loading">Sign In</span>
            <span *ngIf="loading">Signing In...</span>
          </button>

          <div class="form-footer">
            <a routerLink="/webadmin/forgot-password" class="forgot-password">
              Forgot your password?
            </a>
            <span class="separator">|</span>
            <a routerLink="/webadmin/signup" class="signup-link">
              Create Admin Account
            </a>
          </div>
        </form>

        <div *ngIf="error" class="error-alert">
          <i class="fas fa-exclamation-circle"></i>
          <span>{{ error }}</span>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./admin-login.component.scss']
})
export class AdminLoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Check if user is already logged in
    if (this.authService.isAuthenticated() && this.authService.isAdmin()) {
      this.router.navigate(['/webadmin/dashboard']);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.loading = true;
      this.error = null;

      const { email, password } = this.loginForm.value;

      this.authService.login(email, password).subscribe({
        next: (response) => {
          this.loading = false;
          if (response.statusCode === 200) {
            // Login successful - AuthService will handle redirect
            console.log('🔍 [ADMIN LOGIN] ✅ Login successful, redirecting...');
          } else {
            this.error = response.message || 'Login failed';
          }
        },
        error: (error) => {
          this.loading = false;
          this.error = error.message || 'An error occurred during login';
          console.error('🔍 [ADMIN LOGIN] ❌ Login error:', error);
        }
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
