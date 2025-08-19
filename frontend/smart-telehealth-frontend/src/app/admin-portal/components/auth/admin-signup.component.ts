import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CommonService } from '../../../core/services/common.service';
import { UserRole } from '../../../core/models';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';


@Component({
  selector: 'app-admin-signup',
  templateUrl: './admin-signup.component.html',
  styleUrls: ['./admin-signup.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class AdminSignupComponent implements OnInit {
  signupForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  passwordStrength = 0;

  adminRoles: UserRole[] = [
    { id: 1, name: 'Admin', description: 'System administrator with full access', sortOrder: 1 }
  ];

  constructor(
    private fb: FormBuilder,
    private commonService: CommonService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.pattern(/^\+?[\d\s\-\(\)]+$/)]],
      roleId: [null, Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)
      ]],
      confirmPassword: ['', Validators.required],
      acceptTerms: [false, Validators.requiredTrue]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.signupForm.get('password')?.valueChanges.subscribe(password => {
      this.passwordStrength = this.calculatePasswordStrength(password);
    });
  }

  passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  calculatePasswordStrength(password: string): number {
    if (!password) return 0;
    
    let strength = 0;
    if (password.length >= 8) strength += 1;
    if (/[a-z]/.test(password)) strength += 1;
    if (/[A-Z]/.test(password)) strength += 1;
    if (/\d/.test(password)) strength += 1;
    if (/[@$!%*?&]/.test(password)) strength += 1;
    
    return strength;
  }

  getPasswordStrengthText(): string {
    switch (this.passwordStrength) {
      case 0:
      case 1:
        return 'Very Weak';
      case 2:
        return 'Weak';
      case 3:
        return 'Fair';
      case 4:
        return 'Good';
      case 5:
        return 'Strong';
      default:
        return '';
    }
  }

  getPasswordStrengthColor(): string {
    switch (this.passwordStrength) {
      case 0:
      case 1:
        return '#ff4444';
      case 2:
        return '#ff8800';
      case 3:
        return '#ffaa00';
      case 4:
        return '#00aa00';
      case 5:
        return '#008800';
      default:
        return '#cccccc';
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.signupForm.valid) {
      this.isLoading = true;
      
      const signupData = {
        firstName: this.signupForm.value.firstName,
        lastName: this.signupForm.value.lastName,
        email: this.signupForm.value.email,
        phoneNumber: this.signupForm.value.phoneNumber,
        password: this.signupForm.value.password,
        confirmPassword: this.signupForm.value.confirmPassword,
        gender: 'Not Specified',
        address: 'Not Specified',
        city: 'Not Specified',
        state: 'Not Specified',
        zipCode: 'Not Specified',
        role: 'Admin'
      };

      this.authService.register(signupData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.toastService.showSuccess('Admin account created successfully! Please check your email for verification.');
            this.router.navigate(['/admin/login']);
          } else {
            this.toastService.showError(response.message || 'Failed to create admin account');
          }
        },
        error: (error) => {
          this.toastService.showError('An error occurred during signup. Please try again.');
          console.error('Signup error:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  markFormGroupTouched(): void {
    Object.keys(this.signupForm.controls).forEach(key => {
      const control = this.signupForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.signupForm.get(fieldName);
    if (field?.errors && field?.touched) {
      if (field.errors['required']) return `${this.getFieldDisplayName(fieldName)} is required`;
      if (field.errors['email']) return 'Please enter a valid email address';
      if (field.errors['minlength']) return `${this.getFieldDisplayName(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['maxlength']) return `${this.getFieldDisplayName(fieldName)} must not exceed ${field.errors['maxlength'].requiredLength} characters`;
      if (field.errors['pattern']) return `${this.getFieldDisplayName(fieldName)} format is invalid`;
      if (field.errors['passwordMismatch']) return 'Passwords do not match';
    }
    return '';
  }

  getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      firstName: 'First Name',
      lastName: 'Last Name',
      email: 'Email',
      phoneNumber: 'Phone Number',
      roleId: 'Role',
      password: 'Password',
      confirmPassword: 'Confirm Password'
    };
    return displayNames[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.signupForm.get(fieldName);
    return !!(field?.invalid && field?.touched);
  }

  goToLogin(): void {
    this.router.navigate(['/admin/login']);
  }

  // Password requirement check methods
  hasLowercase(): boolean {
    const password = this.signupForm.get('password')?.value || '';
    return /[a-z]/.test(password);
  }

  hasUppercase(): boolean {
    const password = this.signupForm.get('password')?.value || '';
    return /[A-Z]/.test(password);
  }

  hasNumber(): boolean {
    const password = this.signupForm.get('password')?.value || '';
    return /\d/.test(password);
  }

  hasSpecialChar(): boolean {
    const password = this.signupForm.get('password')?.value || '';
    return /[@$!%*?&]/.test(password);
  }

  hasMinLength(): boolean {
    const password = this.signupForm.get('password')?.value || '';
    return password.length >= 8;
  }
}
