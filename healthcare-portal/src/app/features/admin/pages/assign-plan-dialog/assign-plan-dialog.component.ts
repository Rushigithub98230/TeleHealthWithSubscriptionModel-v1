import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { AdminApiService } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-assign-plan-dialog',
  templateUrl: './assign-plan-dialog.component.html',
  styleUrl: './assign-plan-dialog.component.scss'
})
export class AssignPlanDialogComponent {
  form;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private api: AdminApiService,
    private dialogRef: MatDialogRef<AssignPlanDialogComponent>
  ) {
    this.form = this.fb.group({
      userId: ['', Validators.required],
      planId: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    // TODO: Use the API to assign the plan to the user
    // Example: this.api.assignPlan(this.form.value.userId, this.form.value.planId).subscribe(...)
    setTimeout(() => { // Simulate API call
      this.loading = false;
      this.dialogRef.close(this.form.value);
    }, 1000);
  }
}
