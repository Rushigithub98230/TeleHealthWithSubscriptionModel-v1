import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PatientSubscription {
  id: string;
  userId: string;
  planId: string;
  planName: string;
  categoryId: string;
  categoryName: string;
  status: string;
  startDate: string;
  endDate: string;
  price: number;
  consultationsRemaining: number;
  consultationsUsed: number;
  autoRenew: boolean;
  nextBillingDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSubscriptionDto {
  planId: string;
  categoryId: string;
  autoRenew?: boolean;
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
export class PatientSubscriptionService {
  private apiUrl = `${environment.apiUrl}/api/usersubscriptions`;

  constructor(private http: HttpClient) {}

  // Get user subscriptions
  getUserSubscriptions(): Observable<ApiResponse<PatientSubscription[]>> {
    return this.http.get<ApiResponse<PatientSubscription[]>>(`${this.apiUrl}/user`);
  }

  // Get subscription by ID
  getSubscriptionById(id: string): Observable<ApiResponse<PatientSubscription>> {
    return this.http.get<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}`);
  }

  // Create new subscription
  createSubscription(createDto: CreateSubscriptionDto): Observable<ApiResponse<PatientSubscription>> {
    return this.http.post<ApiResponse<PatientSubscription>>(`${this.apiUrl}`, createDto);
  }

  // Update subscription
  updateSubscription(id: string, updateDto: Partial<PatientSubscription>): Observable<ApiResponse<PatientSubscription>> {
    return this.http.put<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}`, updateDto);
  }

  // Cancel subscription
  cancelSubscription(id: string): Observable<ApiResponse<PatientSubscription>> {
    return this.http.post<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  // Pause subscription
  pauseSubscription(id: string): Observable<ApiResponse<PatientSubscription>> {
    return this.http.post<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}/pause`, {});
  }

  // Resume subscription
  resumeSubscription(id: string): Observable<ApiResponse<PatientSubscription>> {
    return this.http.post<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}/resume`, {});
  }

  // Get subscription statistics
  getSubscriptionStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/stats`);
  }

  // Get available plans
  getAvailablePlans(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/plans`);
  }

  // Get subscription usage
  getSubscriptionUsage(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/usage`);
  }

  // Renew subscription
  renewSubscription(id: string): Observable<ApiResponse<PatientSubscription>> {
    return this.http.post<ApiResponse<PatientSubscription>>(`${this.apiUrl}/${id}/renew`, {});
  }

  // Get billing history
  getBillingHistory(id: string): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/${id}/billing`);
  }
} 