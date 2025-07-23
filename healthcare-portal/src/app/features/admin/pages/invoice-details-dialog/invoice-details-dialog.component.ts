import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AdminApiService, BillingRecord } from '../../../../core/admin-api.service';

@Component({
  selector: 'app-invoice-details-dialog',
  templateUrl: './invoice-details-dialog.component.html',
  styleUrl: './invoice-details-dialog.component.scss'
})
export class InvoiceDetailsDialogComponent implements OnInit {
  invoice: BillingRecord | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private api: AdminApiService,
    private dialogRef: MatDialogRef<InvoiceDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { invoiceId: string }
  ) {}

  ngOnInit() {
    this.loading = true;
    this.api.getBillingRecord(this.data.invoiceId).subscribe({
      next: (invoice) => {
        this.invoice = invoice;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load invoice details.';
        this.loading = false;
      }
    });
  }

  close() {
    this.dialogRef.close();
  }
}
