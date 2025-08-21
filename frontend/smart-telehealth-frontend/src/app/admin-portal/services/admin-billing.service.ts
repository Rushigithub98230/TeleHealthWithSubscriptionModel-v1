import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CommonService } from '../../core/services/common.service';
import { 
  BillingRecord, 
  CreateBillingRecordDto, 
  UpdateBillingRecordDto, 
  BillingListParams,
  PaymentProcessingDto,
  RefundRequestDto,
  CreateInvoiceDto,
  InvoiceDto,
  BillingAnalytics
} from '../models/billing.interface';
import { JsonModel } from '../../core/models/json-model.interface';

@Injectable({
  providedIn: 'root'
})
export class AdminBillingService {
  private billingRecordsSubject = new BehaviorSubject<BillingRecord[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  public billingRecords$ = this.billingRecordsSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  constructor(private commonService: CommonService) {}

  // Get all billing records with pagination and filtering
  getAllBillingRecords(params: BillingListParams = {}): Observable<JsonModel<BillingRecord[]>> {
    this.loadingSubject.next(true);

    return this.commonService.get<BillingRecord[]>(
      '/api/Billing',
      params
    ).pipe(
      tap(response => {
        if (response.statusCode === 200 && response.data) {
          this.billingRecordsSubject.next(response.data);
        }
        this.loadingSubject.next(false);
      }),
      tap({
        error: (error) => {
          console.error('Failed to fetch billing records:', error);
          this.loadingSubject.next(false);
        }
      })
    );
  }

  // Get billing record by ID
  getBillingRecordById(id: string): Observable<JsonModel<BillingRecord>> {
    return this.commonService.get<BillingRecord>(`/api/Billing/${id}`);
  }

  // Create new billing record
  createBillingRecord(createDto: CreateBillingRecordDto): Observable<JsonModel<BillingRecord>> {
    return this.commonService.post<BillingRecord>('/api/Billing', createDto);
  }

  // Update billing record
  updateBillingRecord(id: string, updateDto: UpdateBillingRecordDto): Observable<JsonModel<BillingRecord>> {
    return this.commonService.put<BillingRecord>(`/api/Billing/${id}`, updateDto);
  }

  // Delete billing record
  deleteBillingRecord(id: string): Observable<JsonModel<boolean>> {
    return this.commonService.delete<boolean>(`/api/Billing/${id}`);
  }

  // Process payment
  processPayment(paymentDto: PaymentProcessingDto): Observable<JsonModel<any>> {
    return this.commonService.post<any>('/api/Billing/process-payment', paymentDto);
  }

  // Process refund
  processRefund(refundDto: RefundRequestDto): Observable<JsonModel<any>> {
    return this.commonService.post<any>('/api/Billing/refund', refundDto);
  }

  // Create invoice
  createInvoice(createDto: CreateInvoiceDto): Observable<JsonModel<InvoiceDto>> {
    return this.commonService.post<InvoiceDto>('/api/Billing/invoice', createDto);
  }

  // Generate invoice PDF
  generateInvoicePdf(billingRecordId: string): Observable<Blob> {
    return this.commonService.getBlob(`/api/Billing/invoice/${billingRecordId}/pdf`);
  }

  // Get billing analytics
  getBillingAnalytics(startDate?: Date, endDate?: Date): Observable<JsonModel<BillingAnalytics>> {
    let params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.commonService.get<BillingAnalytics>('/api/Billing/analytics', params);
  }

  // Get overdue billing records
  getOverdueBillingRecords(): Observable<JsonModel<BillingRecord[]>> {
    return this.commonService.get<BillingRecord[]>('/api/Billing/overdue');
  }

  // Get pending payments
  getPendingPayments(): Observable<JsonModel<BillingRecord[]>> {
    return this.commonService.get<BillingRecord[]>('/api/Billing/pending');
  }

  // Get failed payments
  getFailedPayments(): Observable<JsonModel<BillingRecord[]>> {
    return this.commonService.get<BillingRecord[]>('/api/Billing', {
      status: ['Failed'],
      page: 1,
      pageSize: 1000
    });
  }

  // Retry failed payment
  retryFailedPayment(billingRecordId: string): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/retry`, {});
  }

  // Get billing adjustments
  getBillingAdjustments(billingRecordId: string): Observable<JsonModel<any[]>> {
    return this.commonService.get<any[]>(`/api/Billing/${billingRecordId}/adjustments`);
  }

  // Apply billing adjustment
  applyBillingAdjustment(billingRecordId: string, adjustmentDto: any): Observable<JsonModel<any>> {
    return this.commonService.post<any>(`/api/Billing/${billingRecordId}/adjustments`, adjustmentDto);
  }

  // Get user billing history
  getUserBillingHistory(userId: number, startDate?: Date, endDate?: Date): Observable<JsonModel<BillingRecord[]>> {
    let params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.commonService.get<BillingRecord[]>(`/api/Billing/user/${userId}`, params);
  }

  // Get subscription billing history
  getSubscriptionBillingHistory(subscriptionId: string): Observable<JsonModel<BillingRecord[]>> {
    return this.commonService.get<BillingRecord[]>(`/api/Billing/subscription/${subscriptionId}`);
  }

  // Calculate total amount
  calculateTotalAmount(baseAmount: number, taxAmount: number, shippingAmount: number): Observable<JsonModel<number>> {
    const params = {
      baseAmount,
      taxAmount,
      shippingAmount
    };

    return this.commonService.post<number>('/api/Billing/calculate-total', params);
  }

  // Calculate tax amount
  calculateTaxAmount(baseAmount: number, state: string): Observable<JsonModel<number>> {
    let params: any = {
      baseAmount: baseAmount.toString(),
      state
    };

    return this.commonService.get<number>('/api/Billing/calculate-tax', params);
  }

  // Calculate shipping amount
  calculateShippingAmount(deliveryAddress: string, isExpress: boolean): Observable<JsonModel<number>> {
    const params = {
      deliveryAddress,
      isExpress
    };

    return this.commonService.post<number>('/api/Billing/calculate-shipping', params);
  }

  // Check if payment is overdue
  isPaymentOverdue(billingRecordId: string): Observable<JsonModel<boolean>> {
    return this.commonService.get<boolean>(`/api/Billing/${billingRecordId}/overdue`);
  }

  // Calculate due date
  calculateDueDate(billingDate: Date, gracePeriodDays: number): Observable<JsonModel<Date>> {
    const params = {
      billingDate: billingDate.toISOString(),
      gracePeriodDays
    };

    return this.commonService.post<Date>('/api/Billing/calculate-due-date', params);
  }

  // Get payment history
  getPaymentHistory(userId: number, startDate?: Date, endDate?: Date): Observable<JsonModel<any[]>> {
    let params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.commonService.get<any[]>(`/api/Billing/payment-history/${userId}`, params);
  }

  // Get payment analytics
  getPaymentAnalytics(startDate?: Date, endDate?: Date, userId?: number): Observable<JsonModel<any>> {
    let params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    if (userId) params.userId = userId.toString();

    return this.commonService.get<any>('/api/Billing/payment-analytics', params);
  }

  // Generate billing report
  generateBillingReport(startDate: Date, endDate: Date, format: string = 'pdf'): Observable<JsonModel<any>> {
    const params = {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      format
    };

    return this.commonService.post<any>('/api/Billing/report', params);
  }

  // Export billing records
  exportBillingRecords(params: BillingListParams, format: 'csv' | 'excel' = 'csv'): Observable<Blob> {
    let exportParams: any = {};
    
    if (params.page) exportParams.page = params.page.toString();
    if (params.pageSize) exportParams.pageSize = params.pageSize.toString();
    if (params.searchTerm) exportParams.searchTerm = params.searchTerm;
    if (params.status && params.status.length > 0) {
      exportParams.status = params.status;
    }
    if (params.type && params.type.length > 0) {
      exportParams.type = params.type;
    }
    if (params.userId && params.userId.length > 0) {
      exportParams.userId = params.userId;
    }
    if (params.subscriptionId && params.subscriptionId.length > 0) {
      exportParams.subscriptionId = params.subscriptionId;
    }
    if (params.dateFrom) exportParams.startDate = params.dateFrom.toISOString();
    if (params.dateTo) exportParams.endDate = params.dateTo.toISOString();
    
    exportParams.format = format;

    return this.commonService.getBlob('/api/Billing/export', exportParams);
  }

  // Clear error
  clearError(): void {
    // The original code had an errorSubject, but it's removed.
    // If error handling is needed, it should be re-added or handled differently.
  }

  // Refresh billing records
  refreshBillingRecords(): void {
    this.getAllBillingRecords().subscribe();
  }
}
