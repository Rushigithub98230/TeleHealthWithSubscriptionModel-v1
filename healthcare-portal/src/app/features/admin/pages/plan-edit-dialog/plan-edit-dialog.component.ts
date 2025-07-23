import { Component, Inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AdminApiService, Plan } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-plan-edit-dialog',
  templateUrl: './plan-edit-dialog.component.html',
  styleUrl: './plan-edit-dialog.component.scss'
})
export class PlanEditDialogComponent {
  form;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private api: AdminApiService,
    private dialogRef: MatDialogRef<PlanEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { plan?: Plan }
  ) {
    this.form = this.fb.group({
      name: [data.plan?.name || '', Validators.required],
      price: [data.plan?.price || '', [Validators.required, Validators.min(0)]],
      duration: [data.plan?.duration || '', Validators.required],
      status: [data.plan?.status || 'Active', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    // Ensure no nulls in form values
    const planData: Partial<Plan> = {
      name: this.form.value.name || '',
      price: typeof this.form.value.price === 'string' ? parseFloat(this.form.value.price) : (this.form.value.price ?? 0),
      duration: this.form.value.duration || '',
      status: this.form.value.status || 'Active'
    };
    const obs = this.data.plan
      ? this.api.updatePlan(this.data.plan.id, planData)
      : this.api.addPlan(planData);
    obs.subscribe({
      next: (plan) => {
        this.loading = false;
        this.dialogRef.close(plan);
      },
      error: (err) => {
        this.error = err.message || 'Failed to save plan.';
        this.loading = false;
      }
    });
  }

  close() {
    this.dialogRef.close();
  }
}
