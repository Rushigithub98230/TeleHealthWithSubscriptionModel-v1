import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProviderFee {
  id: string;
  providerId: string;
  providerName: string;
  categoryId: string;
  categoryName: string;
  proposedFee: number;
  approvedFee: number;
  status: string;
  adminRemarks?: string;
  providerNotes?: string;
  proposedAt?: string;
  reviewedAt?: string;
  reviewedByUserId?: string;
  reviewedByUserName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProviderFeeDto {
  providerId: string;
  categoryId: string;
  proposedFee: number;
  providerNotes?: string;
}

export interface UpdateProviderFeeDto {
  proposedFee: number;
  providerNotes?: string;
}

export interface ReviewProviderFeeDto {
  status: string;
  approvedFee?: number;
  adminRemarks?: string;
}

export interface FeeStatisticsDto {
  totalFees: number;
  pendingFees: number;
  approvedFees: number;
  rejectedFees: number;
  averageProposedFee: number;
  averageApprovedFee: number;
  totalProviders: number;
  totalCategories: number;
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
export class ProviderFeeService {
  private apiUrl = `${environment.apiUrl}/api/providerfee`;

  constructor(private http: HttpClient) {}

  // Create new fee proposal
  createFee(createDto: CreateProviderFeeDto): Observable<ApiResponse<ProviderFee>> {
    return this.http.post<ApiResponse<ProviderFee>>(`${this.apiUrl}`, createDto);
  }

  // Get fee by ID
  getFee(id: string): Observable<ApiResponse<ProviderFee>> {
    return this.http.get<ApiResponse<ProviderFee>>(`${this.apiUrl}/${id}`);
  }

  // Get fee by provider and category
  getFeeByProviderAndCategory(providerId: string, categoryId: string): Observable<ApiResponse<ProviderFee>> {
    return this.http.get<ApiResponse<ProviderFee>>(`${this.apiUrl}/provider/${providerId}/category/${categoryId}`);
  }

  // Update fee proposal
  updateFee(id: string, updateDto: UpdateProviderFeeDto): Observable<ApiResponse<ProviderFee>> {
    return this.http.put<ApiResponse<ProviderFee>>(`${this.apiUrl}/${id}`, updateDto);
  }

  // Submit fee proposal for review
  proposeFee(id: string): Observable<ApiResponse<ProviderFee>> {
    return this.http.post<ApiResponse<ProviderFee>>(`${this.apiUrl}/${id}/propose`, {});
  }

  // Review fee proposal (Admin only)
  reviewFee(id: string, reviewDto: ReviewProviderFeeDto): Observable<ApiResponse<ProviderFee>> {
    return this.http.post<ApiResponse<ProviderFee>>(`${this.apiUrl}/${id}/review`, reviewDto);
  }

  // Get fees by provider
  getFeesByProvider(providerId: string): Observable<ApiResponse<ProviderFee[]>> {
    return this.http.get<ApiResponse<ProviderFee[]>>(`${this.apiUrl}/provider/${providerId}`);
  }

  // Get fees by category
  getFeesByCategory(categoryId: string): Observable<ApiResponse<ProviderFee[]>> {
    return this.http.get<ApiResponse<ProviderFee[]>>(`${this.apiUrl}/category/${categoryId}`);
  }

  // Get all fees with optional filtering
  getAllFees(status?: string, page: number = 1, pageSize: number = 50): Observable<ApiResponse<ProviderFee[]>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (status) {
      params = params.set('status', status);
    }
    
    return this.http.get<ApiResponse<ProviderFee[]>>(`${this.apiUrl}`, { params });
  }

  // Get pending fees
  getPendingFees(): Observable<ApiResponse<ProviderFee[]>> {
    return this.http.get<ApiResponse<ProviderFee[]>>(`${this.apiUrl}/pending`);
  }

  // Get fees by status
  getFeesByStatus(status: string): Observable<ApiResponse<ProviderFee[]>> {
    return this.http.get<ApiResponse<ProviderFee[]>>(`${this.apiUrl}/status/${status}`);
  }

  // Delete fee
  deleteFee(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  // Get fee statistics
  getFeeStatistics(): Observable<ApiResponse<FeeStatisticsDto>> {
    return this.http.get<ApiResponse<FeeStatisticsDto>>(`${this.apiUrl}/statistics`);
  }
} 