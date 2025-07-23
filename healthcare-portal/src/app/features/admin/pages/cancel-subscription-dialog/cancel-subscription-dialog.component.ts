import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { AdminApiService } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-cancel-subscription-dialog',
  templateUrl: './cancel-subscription-dialog.component.html',
  styleUrl: './cancel-subscription-dialog.component.scss'
})
export class CancelSubscriptionDialogComponent {
  form;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private api: AdminApiService,
    private dialogRef: MatDialogRef<CancelSubscriptionDialogComponent>
  ) {
    this.form = this.fb.group({
      reason: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    // TODO: Pass the correct subscription ID and use the API
    // Example: this.api.cancelSubscription(subscriptionId, this.form.value.reason).subscribe(...)
    setTimeout(() => { // Simulate API call
      this.loading = false;
      this.dialogRef.close(this.form.value.reason);
    }, 1000);
  }
}
