import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { AdminApiService } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-refund-dialog',
  templateUrl: './refund-dialog.component.html',
  styleUrl: './refund-dialog.component.scss'
})
export class RefundDialogComponent {
  form;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private api: AdminApiService,
    private dialogRef: MatDialogRef<RefundDialogComponent>
  ) {
    this.form = this.fb.group({
      amount: [null, [Validators.required, Validators.min(0.01)]],
      reason: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    // TODO: Pass the correct billing record ID and use the API
    // Example: this.api.processRefund(billingId, this.form.value.amount).subscribe(...)
    setTimeout(() => { // Simulate API call
      this.loading = false;
      this.dialogRef.close({ ...this.form.value });
    }, 1000);
  }
}
