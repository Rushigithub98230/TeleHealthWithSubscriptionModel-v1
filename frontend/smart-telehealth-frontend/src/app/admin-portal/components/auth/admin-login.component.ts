import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../../core/services/common.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  templateUrl: './admin-login.component.html',
  styleUrls: ['./admin-login.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class AdminLoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  rememberMe = false;
  loginAttempts = 0;
  maxLoginAttempts = 5;
  lockoutTime = 0;
  lockoutDuration = 15 * 60 * 1000; // 15 minutes in milliseconds

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Check if user is already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/admin/dashboard']);
      return;
    }

    // Check for lockout
    this.checkLockoutStatus();
    
    // Load remembered email if any
    const rememberedEmail = localStorage.getItem('admin_remembered_email');
    if (rememberedEmail) {
      this.loginForm.patchValue({ email: rememberedEmail, rememberMe: true });
    }
  }

  checkLockoutStatus(): void {
    const lockoutInfo = localStorage.getItem('admin_login_lockout');
    if (lockoutInfo) {
      const { attempts, timestamp } = JSON.parse(lockoutInfo);
      const now = Date.now();
      
      if (now - timestamp < this.lockoutDuration) {
        this.loginAttempts = attempts;
        this.lockoutTime = this.lockoutDuration - (now - timestamp);
        this.updateLockoutDisplay();
      } else {
        // Lockout expired, reset
        localStorage.removeItem('admin_login_lockout');
        this.loginAttempts = 0;
        this.lockoutTime = 0;
      }
    }
  }

  updateLockoutDisplay(): void {
    if (this.lockoutTime > 0) {
      const minutes = Math.floor(this.lockoutTime / 60000);
      const seconds = Math.floor((this.lockoutTime % 60000) / 1000);
      
      setTimeout(() => {
        this.lockoutTime -= 1000;
        if (this.lockoutTime > 0) {
          this.updateLockoutDisplay();
        } else {
          this.lockoutTime = 0;
          this.loginAttempts = 0;
          localStorage.removeItem('admin_login_lockout');
        }
      }, 1000);
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.valid && !this.isLockedOut()) {
      this.isLoading = true;
      
      const loginData = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password
      };

      this.authService.login(loginData.email, loginData.password).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            // Handle successful login
            this.handleSuccessfulLogin(response.data);
          } else {
            this.handleFailedLogin(response.message || 'Login failed');
          }
        },
        error: (error) => {
          this.handleFailedLogin('An error occurred during login. Please try again.');
          console.error('Login error:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else if (this.isLockedOut()) {
      this.toastService.showError('Account temporarily locked due to multiple failed attempts. Please try again later.');
    } else {
      this.markFormGroupTouched();
    }
  }

  handleSuccessfulLogin(userData: any): void {
    // Store user data and token
    this.authService.setCurrentUser(userData);
    
    // Handle remember me functionality
    if (this.loginForm.value.rememberMe) {
      localStorage.setItem('admin_remembered_email', userData.email);
    } else {
      localStorage.removeItem('admin_remembered_email');
    }

    // Reset login attempts
    this.loginAttempts = 0;
    localStorage.removeItem('admin_login_lockout');

    // Show success message
    this.toastService.showSuccess('Welcome back! Redirecting to dashboard...');

    // Redirect to dashboard
    setTimeout(() => {
      this.router.navigate(['/admin/dashboard']);
    }, 1000);
  }

  handleFailedLogin(message: string): void {
    this.loginAttempts++;
    
    // Store lockout information
    const lockoutInfo = {
      attempts: this.loginAttempts,
      timestamp: Date.now()
    };
    localStorage.setItem('admin_login_lockout', JSON.stringify(lockoutInfo));

    // Check if account should be locked
    if (this.loginAttempts >= this.maxLoginAttempts) {
      this.lockoutTime = this.lockoutDuration;
      this.updateLockoutDisplay();
      this.toastService.showError('Account locked due to multiple failed attempts. Please try again in 15 minutes.');
    } else {
      const remainingAttempts = this.maxLoginAttempts - this.loginAttempts;
      this.toastService.showError(`${message} (${remainingAttempts} attempts remaining)`);
    }

    // Clear password field
    this.loginForm.patchValue({ password: '' });
  }

  isLockedOut(): boolean {
    return this.lockoutTime > 0;
  }

  getLockoutMessage(): string {
    if (this.lockoutTime > 0) {
      const minutes = Math.floor(this.lockoutTime / 60000);
      const seconds = Math.floor((this.lockoutTime % 60000) / 1000);
      return `Account locked. Try again in ${minutes}:${seconds.toString().padStart(2, '0')}`;
    }
    return '';
  }

  markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (field?.errors && field?.touched) {
      if (field.errors['required']) return `${this.getFieldDisplayName(fieldName)} is required`;
      if (field.errors['email']) return 'Please enter a valid email address';
    }
    return '';
  }

  getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      email: 'Email',
      password: 'Password'
    };
    return displayNames[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field?.invalid && field?.touched);
  }

  goToSignup(): void {
    this.router.navigate(['/admin/signup']);
  }

  goToForgotPassword(): void {
    this.router.navigate(['/admin/forgot-password']);
  }

  // Demo login for development (remove in production)
  demoLogin(): void {
    this.loginForm.patchValue({
      email: 'admin@smarttelehealth.com',
      password: 'Admin@123'
    });
    this.onSubmit();
  }
}
