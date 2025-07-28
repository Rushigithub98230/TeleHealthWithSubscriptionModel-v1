import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PatientConsultation {
  id: string;
  userId: string;
  providerId: string;
  providerName: string;
  categoryId: string;
  categoryName: string;
  status: string;
  scheduledAt: string;
  startedAt?: string;
  endedAt?: string;
  fee: number;
  duration: number;
  notes?: string;
  requiresFollowUp: boolean;
  followUpDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateConsultationDto {
  providerId: string;
  categoryId: string;
  scheduledAt: string;
  notes?: string;
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
export class PatientConsultationService {
  private apiUrl = `${environment.apiUrl}/api/consultations`;

  constructor(private http: HttpClient) {}

  // Get user consultations
  getUserConsultations(): Observable<ApiResponse<PatientConsultation[]>> {
    return this.http.get<ApiResponse<PatientConsultation[]>>(`${this.apiUrl}/user`);
  }

  // Get consultation by ID
  getConsultationById(id: string): Observable<ApiResponse<PatientConsultation>> {
    return this.http.get<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}`);
  }

  // Create new consultation
  createConsultation(createDto: CreateConsultationDto): Observable<ApiResponse<PatientConsultation>> {
    return this.http.post<ApiResponse<PatientConsultation>>(`${this.apiUrl}`, createDto);
  }

  // Update consultation
  updateConsultation(id: string, updateDto: Partial<PatientConsultation>): Observable<ApiResponse<PatientConsultation>> {
    return this.http.put<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}`, updateDto);
  }

  // Cancel consultation
  cancelConsultation(id: string): Observable<ApiResponse<PatientConsultation>> {
    return this.http.post<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  // Start consultation
  startConsultation(id: string): Observable<ApiResponse<PatientConsultation>> {
    return this.http.post<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}/start`, {});
  }

  // End consultation
  endConsultation(id: string): Observable<ApiResponse<PatientConsultation>> {
    return this.http.post<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}/end`, {});
  }

  // Get consultation statistics
  getConsultationStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/stats`);
  }

  // Get consultations by status
  getConsultationsByStatus(status: string): Observable<ApiResponse<PatientConsultation[]>> {
    return this.http.get<ApiResponse<PatientConsultation[]>>(`${this.apiUrl}/status/${status}`);
  }

  // Get upcoming consultations
  getUpcomingConsultations(): Observable<ApiResponse<PatientConsultation[]>> {
    return this.http.get<ApiResponse<PatientConsultation[]>>(`${this.apiUrl}/upcoming`);
  }

  // Get completed consultations
  getCompletedConsultations(): Observable<ApiResponse<PatientConsultation[]>> {
    return this.http.get<ApiResponse<PatientConsultation[]>>(`${this.apiUrl}/completed`);
  }

  // Get consultation summary
  getConsultationSummary(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/summary`);
  }

  // Schedule follow-up
  scheduleFollowUp(id: string, followUpDate: string): Observable<ApiResponse<PatientConsultation>> {
    return this.http.post<ApiResponse<PatientConsultation>>(`${this.apiUrl}/${id}/followup`, { followUpDate });
  }

  // Get available providers
  getAvailableProviders(categoryId: string): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/providers/${categoryId}`);
  }

  // Get consultation history
  getConsultationHistory(): Observable<ApiResponse<PatientConsultation[]>> {
    return this.http.get<ApiResponse<PatientConsultation[]>>(`${this.apiUrl}/history`);
  }
} 