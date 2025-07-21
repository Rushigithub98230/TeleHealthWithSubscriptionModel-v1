import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-payment-form',
  templateUrl: './payment-form.component.html',
  styleUrls: ['./payment-form.component.css']
})
export class PaymentFormComponent {
  result?: string;

  constructor(private http: HttpClient) {}

  subscribe() {
    const successUrl = window.location.origin + '/success';
    const cancelUrl = window.location.origin + '/cancel';
    this.http.post<any>('/api/stripe/create-checkout-session', { successUrl, cancelUrl })
      .subscribe({
        next: (res) => {
          if (res.url) {
            window.location.href = res.url;
          } else {
            this.result = 'Failed to get Stripe Checkout URL';
          }
        },
        error: err => this.result = 'Subscription failed: ' + (err.error?.message || err.message)
      });
  }
}