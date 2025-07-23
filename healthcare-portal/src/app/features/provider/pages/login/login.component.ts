import { Component } from '@angular/core';
import { AuthService } from '../../../../core/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  loading = false;
  error: string | null = null;

  constructor(private auth: AuthService, private router: Router, private snackBar: MatSnackBar) {}

  onSubmit() {
    this.loading = true;
    this.error = null;
    this.auth.login({ email: this.username, password: this.password }).subscribe({
      next: (res: any) => {
        const token = res.data?.token || res.token;
        if (token) {
          localStorage.setItem('token', token);
          this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
          this.router.navigate(['/provider/dashboard']);
        } else {
          this.error = 'Invalid response from server.';
          this.snackBar.open('Login failed: Invalid response from server.', 'Close', { duration: 3000 });
        }
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.message || 'Login failed.';
        this.snackBar.open('Login failed: ' + this.error, 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  register() {
    const email = window.prompt('Enter email:');
    if (!email) return;
    const password = window.prompt('Enter password:');
    if (!password) return;
    const firstName = window.prompt('Enter first name:');
    if (!firstName) return;
    const lastName = window.prompt('Enter last name:');
    if (!lastName) return;
    this.loading = true;
    this.error = null;
    this.auth.register({ email, password, firstName, lastName }).subscribe({
      next: (res: any) => {
        this.snackBar.open('Registration successful! You can now log in.', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.message || 'Registration failed.';
        this.snackBar.open('Registration failed: ' + this.error, 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
}
