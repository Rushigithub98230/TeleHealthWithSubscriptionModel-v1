import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { CommonService } from '../../core/services/common.service';
import { AuthService } from '../../core/services/auth.service';
import { JsonModel } from '../../core/models/json-model.interface';
import { PaymentMethod, CreatePaymentMethodDto, UpdatePaymentMethodDto, PaymentMethodTypeInfo, PaymentMethodCountry } from '../models/payment.interface';

@Injectable({
  providedIn: 'root'
})
export class UserPaymentService {
  private paymentMethodsSubject = new BehaviorSubject<PaymentMethod[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public paymentMethods$ = this.paymentMethodsSubject.asObservable();
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

  // Get user payment methods
  getUserPaymentMethods(): Observable<JsonModel<PaymentMethod[]>> {
    return this.commonService.get<PaymentMethod[]>('/api/Payment/payment-methods');
  }

  // Add new payment method
  addPaymentMethod(paymentMethodId: string): Observable<JsonModel<PaymentMethod>> {
    return this.commonService.post<PaymentMethod>('/api/Payment/payment-methods', { PaymentMethodId: paymentMethodId });
  }

  // Delete payment method (maps to RemovePaymentMethod in backend)
  deletePaymentMethod(paymentMethodId: string): Observable<JsonModel<any>> {
    return this.commonService.delete<any>(`/api/Payment/payment-methods/${paymentMethodId}`);
  }

  // Set default payment method
  setDefaultPaymentMethod(paymentMethodId: string): Observable<JsonModel<any>> {
    return this.commonService.put<any>(`/api/Payment/payment-methods/${paymentMethodId}/default`, {});
  }

  // Validate payment method (maps to add payment method validation)
  validatePaymentMethod(paymentMethodId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>('/api/Payment/payment-methods', { PaymentMethodId: paymentMethodId });
  }

  // Process payment (maps to billing process-payment)
  processPayment(billingRecordId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/process-payment`, {});
  }

  // Retry payment (maps to billing retry)
  retryPayment(billingRecordId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/retry`, {});
  }

  // Process refund (maps to billing refund)
  processRefund(billingRecordId: string, amount: number, reason: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/process-refund`, { Amount: amount, Reason: reason });
  }

  // Get payment history
  getPaymentHistory(startDate?: Date, endDate?: Date): Observable<JsonModel<any[]>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    return this.commonService.get<any[]>(`/api/Billing/payment-history`, params);
  }

  // Get payment method types (mocked since backend endpoint is missing)
  getPaymentMethodTypes(): Observable<PaymentMethodTypeInfo[]> {
    // Mock data since backend endpoint is missing
    const mockTypes: PaymentMethodTypeInfo[] = [
      { 
        type: 'visa', 
        name: 'Visa', 
        description: 'Visa credit and debit cards', 
        logo: 'visa-logo.png', 
        supportedCountries: ['US', 'CA', 'GB', 'AU'], 
        features: ['Secure', 'Widely accepted'], 
        securityFeatures: ['3D Secure', 'Fraud protection'] 
      },
      { 
        type: 'mastercard', 
        name: 'MasterCard', 
        description: 'MasterCard credit and debit cards', 
        logo: 'mastercard-logo.png', 
        supportedCountries: ['US', 'CA', 'GB', 'AU'], 
        features: ['Secure', 'Widely accepted'], 
        securityFeatures: ['3D Secure', 'Fraud protection'] 
      },
      { 
        type: 'amex', 
        name: 'American Express', 
        description: 'American Express credit cards', 
        logo: 'amex-logo.png', 
        supportedCountries: ['US', 'CA', 'GB'], 
        features: ['Premium service', 'Rewards'], 
        securityFeatures: ['Advanced fraud protection'] 
      }
    ];
    return of(mockTypes);
  }

  // Get supported countries (mocked since backend endpoint is missing)
  getSupportedCountries(): Observable<PaymentMethodCountry[]> {
    // Mock data since backend endpoint is missing
    const mockCountries: PaymentMethodCountry[] = [
      { 
        code: 'US', 
        name: 'United States', 
        supported: true, 
        currency: 'USD', 
        paymentMethods: ['visa', 'mastercard', 'amex'] 
      },
      { 
        code: 'CA', 
        name: 'Canada', 
        supported: true, 
        currency: 'CAD', 
        paymentMethods: ['visa', 'mastercard', 'amex'] 
      },
      { 
        code: 'GB', 
        name: 'United Kingdom', 
        supported: true, 
        currency: 'GBP', 
        paymentMethods: ['visa', 'mastercard', 'amex'] 
      }
    ];
    return of(mockCountries);
  }

  // Get payment method analytics
  getPaymentMethodAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    return this.commonService.get<any>('/api/Billing/analytics', params);
  }

  // Refresh payment methods data
  refreshPaymentMethods(): void {
    this.getUserPaymentMethods().subscribe({
      next: (response) => {
        if (response.statusCode === 200 && response.data) {
          this.paymentMethodsSubject.next(response.data);
        }
      },
      error: (error) => {
        console.error('Failed to refresh payment methods:', error);
      }
    });
  }

  // Set loading state
  setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }
}
