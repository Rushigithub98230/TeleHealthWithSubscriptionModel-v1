import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { CommonService } from '../../core/services/common.service';
import { AuthService } from '../../core/services/auth.service';
import { JsonModel } from '../../core/models/json-model.interface';
import { BillingRecord, BillingAnalytics, BillingPreferences } from '../models/billing.interface';

@Injectable({
  providedIn: 'root'
})
export class UserBillingService {
  private billingRecordsSubject = new BehaviorSubject<BillingRecord[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public billingRecords$ = this.billingRecordsSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(
    private commonService: CommonService,
    private authService: AuthService
  ) {}

  // Get current user ID from auth service
  private getCurrentUserId(): number {
    const user = this.authService.getCurrentUser();
    return user?.id || 1; // Fallback to 1 if user not found
  }

  // Get user billing records
  getUserBillingRecords(): Observable<JsonModel<BillingRecord[]>> {
    const userId = this.getCurrentUserId();
    return this.commonService.get<BillingRecord[]>(`/api/Billing/user/${userId}`);
  }

  // Get billing record by ID
  getBillingRecordById(id: string): Observable<JsonModel<BillingRecord>> {
    return this.commonService.get<BillingRecord>(`/api/Billing/${id}`);
  }

  // Get billing records for a specific subscription
  getBillingRecordsForSubscription(subscriptionId: string): Observable<JsonModel<BillingRecord[]>> {
    return this.commonService.get<BillingRecord[]>(`/api/Billing/subscription/${subscriptionId}`);
  }

  // Process payment
  processPayment(billingRecordId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/process-payment`, {});
  }

  // Download invoice
  downloadInvoice(billingRecordId: string): Observable<Blob> {
    return this.commonService.getBlob(`/api/Billing/${billingRecordId}/invoice-pdf`);
  }

  // Get billing analytics (Admin only - users should not access this directly)
  getBillingAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<BillingAnalytics>> {
    // Note: This endpoint doesn't exist in backend for users
    // Return empty data for now
    return of({
      data: {},
      message: "Analytics not available for users",
      statusCode: 501
    } as JsonModel<BillingAnalytics>);
  }

  // Get user billing summary (maps to backend getBillingSummary)
  getBillingSummary(startDate?: Date, endDate?: Date): Observable<JsonModel<any>> {
    const userId = this.getCurrentUserId();
    const params: any = { userId };
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    // Note: This endpoint doesn't exist in backend
    // Return empty data for now
    return of({
      data: {},
      message: "Summary not available",
      statusCode: 501
    } as JsonModel<any>);
  }

  // Get payment history
  getPaymentHistory(startDate?: Date, endDate?: Date): Observable<JsonModel<BillingRecord[]>> {
    const userId = this.getCurrentUserId();
    const params: any = { userId };
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    return this.commonService.get<BillingRecord[]>(`/api/Billing/payment-history`, params);
  }

  // Request refund (maps to backend processRefund)
  requestRefund(billingRecordId: string, amount: number, reason: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/process-refund`, { Amount: amount, Reason: reason });
  }

  // Check overdue status (maps to backend overdue check)
  checkOverdueStatus(billingRecordId: string): Observable<JsonModel<any>> {
    // Note: This specific endpoint doesn't exist in backend
    // Return empty data for now
    return of({
      data: {},
      message: "Overdue status not available",
      statusCode: 501
    } as JsonModel<any>);
  }

  // Update payment method for a billing record (maps to backend update)
  updatePaymentMethod(billingRecordId: string, paymentMethodId: string): Observable<JsonModel<any>> {
    // Note: This specific endpoint doesn't exist in backend
    // Return empty data for now
    return of({
      data: {},
      message: "Payment method update not available",
      statusCode: 501
    } as JsonModel<any>);
  }

  // Get payment schedule for a subscription (maps to backend schedule)
  getPaymentSchedule(subscriptionId: string): Observable<JsonModel<any>> {
    // Note: This specific endpoint doesn't exist in backend
    // Return empty data for now
    return of({
      data: {},
      message: "Payment schedule not available",
      statusCode: 501
    } as JsonModel<any>);
  }

  // Process partial payment (maps to backend partial payment)
  processPartialPayment(billingRecordId: string, amount: number): Observable<JsonModel<any>> {
    // Note: This specific endpoint doesn't exist in backend
    // Return empty data for now
    return of({
      data: {},
      message: "Partial payment not available",
      statusCode: 501
    } as JsonModel<any>);
  }

  // Retry failed payment
  retryPayment(billingRecordId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/retry`, {});
  }

  // Note: Billing preferences and auto-pay endpoints don't exist in the backend
  // These methods are removed as they don't have corresponding backend endpoints

  // Refresh billing records data
  refreshBillingRecords(): void {
    this.getUserBillingRecords().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.billingRecordsSubject.next(response.data);
        }
      },
      error: (error) => {
        console.error('Failed to refresh billing records:', error);
      }
    });
  }

  // Set loading state
  setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }
}
