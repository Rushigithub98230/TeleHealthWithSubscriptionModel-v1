import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../../core/services/common.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-admin-forgot-password',
  templateUrl: './admin-forgot-password.component.html',
  styleUrls: ['./admin-forgot-password.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class AdminForgotPasswordComponent implements OnInit, OnDestroy {
  forgotPasswordForm: FormGroup;
  isLoading = false;
  emailSent = false;
  countdown = 0;
  countdownInterval: any;

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    // Check if user is already logged in
    // If logged in, redirect to dashboard
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.valid) {
      this.isLoading = true;
      
      const resetData = {
        email: this.forgotPasswordForm.value.email,
        userType: 'Admin'
      };

              this.commonService.post<any>('/api/auth/forgot-password', resetData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.handleSuccessfulReset();
          } else {
            this.toastService.showError(response.message || 'Failed to send reset email');
          }
        },
        error: (error) => {
          this.toastService.showError('An error occurred. Please try again.');
          console.error('Forgot password error:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  handleSuccessfulReset(): void {
    this.emailSent = true;
    this.startCountdown();
    this.toastService.showSuccess('Password reset email sent successfully! Please check your inbox.');
  }

  startCountdown(): void {
    this.countdown = 60; // 60 seconds countdown
    this.countdownInterval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }

  resendEmail(): void {
    if (this.countdown <= 0) {
      this.onSubmit();
    }
  }

  markFormGroupTouched(): void {
    Object.keys(this.forgotPasswordForm.controls).forEach(key => {
      const control = this.forgotPasswordForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.forgotPasswordForm.get(fieldName);
    if (field?.errors && field?.touched) {
      if (field.errors['required']) return `${this.getFieldDisplayName(fieldName)} is required`;
      if (field.errors['email']) return 'Please enter a valid email address';
    }
    return '';
  }

  getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      email: 'Email'
    };
    return displayNames[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.forgotPasswordForm.get(fieldName);
    return !!(field?.invalid && field?.touched);
  }

  goToLogin(): void {
    this.router.navigate(['/admin/login']);
  }

  goToSignup(): void {
    this.router.navigate(['/admin/signup']);
  }

  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }
}
