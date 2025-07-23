import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientApiService, Patient } from '../../../../core/patient-api.service';
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
  patients: Patient[] = [];
  loading = false;
  error: string | null = null;
  userName: string | null = null;
  userRole: string | null = null;
  avatarUrl: string | null = null;

  constructor(
    private patientApi: PatientApiService,
    private auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.fetchPatients();
    this.setUserInfo();
  }

  setUserInfo() {
    const token = localStorage.getItem('token');
    if (token) {
      const decoded = decodeJwt(token);
      this.userName = decoded?.name || decoded?.email || 'Unknown';
      this.userRole = decoded?.role || decoded?.roles || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Patient';
      this.avatarUrl = decoded?.avatarUrl || null;
      if (Array.isArray(this.userRole)) {
        this.userRole = this.userRole.join(', ');
      }
    }
  }

  openProfileDialog() {
    this.dialog.open(ProfileComponent, { width: '420px' });
  }

  fetchPatients() {
    this.loading = true;
    this.patientApi.getAllPatients().subscribe({
      next: (patients: Patient[]) => {
        this.patients = patients;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load patients';
        this.snackBar.open('Failed to load patients', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  addPatient() {
    const name = window.prompt('Enter patient name:');
    if (!name) return;
    const email = window.prompt('Enter patient email:');
    if (!email) return;
    this.loading = true;
    this.patientApi.createPatient({ name, email }).subscribe({
      next: (patient: Patient) => {
        this.patients.push(patient);
        this.snackBar.open('Patient added successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to add patient';
        this.snackBar.open('Failed to add patient', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  editPatient(patient: Patient) {
    const name = window.prompt('Edit patient name:', patient.name);
    if (!name) return;
    const email = window.prompt('Edit patient email:', patient.email);
    if (!email) return;
    this.loading = true;
    this.patientApi.updatePatient(patient.id, { name, email }).subscribe({
      next: (updated: Patient) => {
        this.patients = this.patients.map(p => p.id === patient.id ? updated : p);
        this.snackBar.open('Patient updated successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to update patient';
        this.snackBar.open('Failed to update patient', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  deletePatient(patient: Patient) {
    if (!window.confirm(`Are you sure you want to delete patient ${patient.name}?`)) {
      return;
    }
    this.loading = true;
    this.patientApi.deletePatient(patient.id).subscribe({
      next: () => {
        this.patients = this.patients.filter(p => p.id !== patient.id);
        this.snackBar.open('Patient deleted successfully!', 'Close', { duration: 3000 });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to delete patient';
        this.snackBar.open('Failed to delete patient', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/patient/login']);
      },
      error: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/patient/login']);
      }
    });
  }
}
