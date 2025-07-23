import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AdminApiService } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-plan-delete-dialog',
  templateUrl: './plan-delete-dialog.component.html',
  styleUrl: './plan-delete-dialog.component.scss'
})
export class PlanDeleteDialogComponent {
  loading = false;
  error: string | null = null;

  constructor(
    private api: AdminApiService,
    private dialogRef: MatDialogRef<PlanDeleteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { planId: string, planName: string }
  ) {}

  confirmDelete() {
    this.loading = true;
    this.api.deletePlan(this.data.planId).subscribe({
      next: () => {
        this.loading = false;
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.error = err.message || 'Failed to delete plan.';
        this.loading = false;
      }
    });
  }

  close() {
    this.dialogRef.close(false);
  }
}
