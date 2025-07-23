import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../core/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatProgressBarModule, MatButtonModule, MatSnackBarModule, MatDialogModule],
  template: `
    <div style="max-width:400px;margin:2rem auto;padding:2rem;border:1px solid #eee;border-radius:8px;">
      <h2>Profile</h2>
      <div style="text-align:center;margin-bottom:1rem;">
        <img *ngIf="profile?.avatarUrl" [src]="profile.avatarUrl" alt="Avatar" style="width:80px;height:80px;border-radius:50%;object-fit:cover;border:1px solid #ccc;" />
        <div *ngIf="!profile?.avatarUrl" style="width:80px;height:80px;border-radius:50%;background:#eee;display:inline-block;"></div>
        <br />
        <input type="file" accept="image/*" (change)="onAvatarSelected($event)" />
      </div>
      <form (ngSubmit)="updateProfile()" *ngIf="profile">
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput [(ngModel)]="profile.name" name="name" required />
        </mat-form-field>
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput [(ngModel)]="profile.email" name="email" required />
        </mat-form-field>
        <button mat-raised-button color="primary" type="submit" [disabled]="loading">Update</button>
        <mat-progress-bar *ngIf="loading" mode="indeterminate"></mat-progress-bar>
      </form>
      <div *ngIf="error" class="error">{{ error }}</div>
      <hr style="margin:2rem 0;">
      <h3>Change Password</h3>
      <form (ngSubmit)="changePassword()">
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Current Password</mat-label>
          <input matInput [(ngModel)]="currentPassword" name="currentPassword" type="password" required />
        </mat-form-field>
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>New Password</mat-label>
          <input matInput [(ngModel)]="newPassword" name="newPassword" type="password" required minlength="6" />
        </mat-form-field>
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Confirm New Password</mat-label>
          <input matInput [(ngModel)]="confirmNewPassword" name="confirmNewPassword" type="password" required minlength="6" />
        </mat-form-field>
        <button mat-raised-button color="accent" type="submit" [disabled]="loading">Change Password</button>
      </form>
    </div>
  `
})
export class ProfileComponent implements OnInit {
  profile: any = null;
  loading = false;
  error: string | null = null;
  currentPassword = '';
  newPassword = '';
  confirmNewPassword = '';

  constructor(private auth: AuthService, private snackBar: MatSnackBar, private http: HttpClient) {}

  ngOnInit() {
    this.fetchProfile();
  }

  fetchProfile() {
    this.loading = true;
    this.auth.getProfile().subscribe({
      next: (res: any) => {
        this.profile = res.data || res;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load profile.';
        this.snackBar.open('Failed to load profile.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onAvatarSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    this.loading = true;
    this.http.post<any>('/api/FileStorage/upload', formData).subscribe({
      next: (res) => {
        const url = res.data || res;
        this.profile.avatarUrl = url;
        this.snackBar.open('Avatar uploaded!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: () => {
        this.snackBar.open('Failed to upload avatar.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  updateProfile() {
    this.loading = true;
    this.auth.updateProfile(this.profile).subscribe({
      next: () => {
        this.snackBar.open('Profile updated successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to update profile.';
        this.snackBar.open('Failed to update profile.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  changePassword() {
    if (this.newPassword !== this.confirmNewPassword) {
      this.snackBar.open('New passwords do not match.', 'Close', { duration: 3000 });
      return;
    }
    this.loading = true;
    this.auth.changePassword({
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }).subscribe({
      next: () => {
        this.snackBar.open('Password changed successfully!', 'Close', { duration: 3000 });
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmNewPassword = '';
        this.loading = false;
      },
      error: (err: any) => {
        this.snackBar.open('Failed to change password.', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }
} 