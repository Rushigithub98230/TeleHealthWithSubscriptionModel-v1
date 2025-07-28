import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProviderPayout {
  id: string;
  providerId: string;
  providerName: string;
  periodId: string;
  periodName: string;
  totalAmount: number;
  platformCommission: number;
  providerAmount: number;
  status: string;
  processedAt?: string;
  processedByUserId?: string;
  processedByUserName?: string;
  paymentMethod: string;
  paymentReference?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProcessPayoutDto {
  status: string;
  paymentReference?: string;
  notes?: string;
}

export interface ProviderEarningsDto {
  providerId: string;
  providerName: string;
  totalEarnings: number;
  pendingEarnings: number;
  processedEarnings: number;
  totalConsultations: number;
  averagePerConsultation: number;
  lastPayoutDate?: string;
  nextPayoutDate?: string;
}

export interface PayoutStatisticsDto {
  totalPayouts: number;
  pendingPayouts: number;
  processedPayouts: number;
  totalAmount: number;
  averagePayoutAmount: number;
  totalProviders: number;
  totalPeriods: number;
}

export interface PayoutPeriodDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
  totalProviders: number;
  totalAmount: number;
  processedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePayoutPeriodDto {
  name: string;
  startDate: string;
  endDate: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProviderPayoutService {
  private apiUrl = `${environment.apiUrl}/api/providerpayout`;

  constructor(private http: HttpClient) {}

  // Get payout by ID
  getPayout(id: string): Observable<ApiResponse<ProviderPayout>> {
    return this.http.get<ApiResponse<ProviderPayout>>(`${this.apiUrl}/${id}`);
  }

  // Process payout (Admin only)
  processPayout(id: string, processDto: ProcessPayoutDto): Observable<ApiResponse<ProviderPayout>> {
    return this.http.post<ApiResponse<ProviderPayout>>(`${this.apiUrl}/${id}/process`, processDto);
  }

  // Get payouts by provider
  getPayoutsByProvider(providerId: string): Observable<ApiResponse<ProviderPayout[]>> {
    return this.http.get<ApiResponse<ProviderPayout[]>>(`${this.apiUrl}/provider/${providerId}`);
  }

  // Get payouts by period
  getPayoutsByPeriod(periodId: string): Observable<ApiResponse<ProviderPayout[]>> {
    return this.http.get<ApiResponse<ProviderPayout[]>>(`${this.apiUrl}/period/${periodId}`);
  }

  // Get all payouts with optional filtering
  getAllPayouts(status?: string, page: number = 1, pageSize: number = 50): Observable<ApiResponse<ProviderPayout[]>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (status) {
      params = params.set('status', status);
    }
    
    return this.http.get<ApiResponse<ProviderPayout[]>>(`${this.apiUrl}`, { params });
  }

  // Get pending payouts
  getPendingPayouts(): Observable<ApiResponse<ProviderPayout[]>> {
    return this.http.get<ApiResponse<ProviderPayout[]>>(`${this.apiUrl}/pending`);
  }

  // Get payouts by status
  getPayoutsByStatus(status: string): Observable<ApiResponse<ProviderPayout[]>> {
    return this.http.get<ApiResponse<ProviderPayout[]>>(`${this.apiUrl}/status/${status}`);
  }

  // Get provider earnings
  getProviderEarnings(providerId: string): Observable<ApiResponse<ProviderEarningsDto>> {
    return this.http.get<ApiResponse<ProviderEarningsDto>>(`${this.apiUrl}/earnings/${providerId}`);
  }

  // Get payout statistics
  getPayoutStatistics(): Observable<ApiResponse<PayoutStatisticsDto>> {
    return this.http.get<ApiResponse<PayoutStatisticsDto>>(`${this.apiUrl}/statistics`);
  }

  // Generate payouts for period
  generatePayoutsForPeriod(periodId: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/period/${periodId}/generate`, {});
  }

  // Process all pending payouts
  processAllPendingPayouts(): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/process-all-pending`, {});
  }

  // Payout Period Management
  createPeriod(createDto: CreatePayoutPeriodDto): Observable<ApiResponse<PayoutPeriodDto>> {
    return this.http.post<ApiResponse<PayoutPeriodDto>>(`${this.apiUrl}/periods`, createDto);
  }

  getPeriod(id: string): Observable<ApiResponse<PayoutPeriodDto>> {
    return this.http.get<ApiResponse<PayoutPeriodDto>>(`${this.apiUrl}/periods/${id}`);
  }

  updatePeriod(id: string, updateDto: CreatePayoutPeriodDto): Observable<ApiResponse<PayoutPeriodDto>> {
    return this.http.put<ApiResponse<PayoutPeriodDto>>(`${this.apiUrl}/periods/${id}`, updateDto);
  }

  getAllPeriods(): Observable<ApiResponse<PayoutPeriodDto[]>> {
    return this.http.get<ApiResponse<PayoutPeriodDto[]>>(`${this.apiUrl}/periods`);
  }

  getActivePeriods(): Observable<ApiResponse<PayoutPeriodDto[]>> {
    return this.http.get<ApiResponse<PayoutPeriodDto[]>>(`${this.apiUrl}/periods/active`);
  }

  deletePeriod(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/periods/${id}`);
  }

  processPeriod(id: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/periods/${id}/process`, {});
  }
} 