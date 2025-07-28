import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProviderConsultation {
  id: string;
  providerId: string;
  patientId: string;
  patientName: string;
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
  patientId: string;
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
export class ProviderConsultationService {
  private apiUrl = `${environment.apiUrl}/api/consultations`;

  constructor(private http: HttpClient) {}

  // Get provider consultations
  getProviderConsultations(): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider`);
  }

  // Get consultation by ID
  getConsultationById(id: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.get<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}`);
  }

  // Create new consultation
  createConsultation(createDto: CreateConsultationDto): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}`, createDto);
  }

  // Update consultation
  updateConsultation(id: string, updateDto: Partial<ProviderConsultation>): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.put<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}`, updateDto);
  }

  // Start consultation
  startConsultation(id: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}/start`, {});
  }

  // End consultation
  endConsultation(id: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}/end`, {});
  }

  // Cancel consultation
  cancelConsultation(id: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}/cancel`, {});
  }

  // Get consultation statistics
  getConsultationStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/provider/stats`);
  }

  // Get consultations by status
  getConsultationsByStatus(status: string): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider/status/${status}`);
  }

  // Get upcoming consultations
  getUpcomingConsultations(): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider/upcoming`);
  }

  // Get completed consultations
  getCompletedConsultations(): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider/completed`);
  }

  // Get in-progress consultations
  getInProgressConsultations(): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider/in-progress`);
  }

  // Get consultation summary
  getConsultationSummary(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/summary`);
  }

  // Schedule follow-up
  scheduleFollowUp(id: string, followUpDate: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}/followup`, { followUpDate });
  }

  // Get available patients
  getAvailablePatients(categoryId: string): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/provider/patients/${categoryId}`);
  }

  // Get consultation history
  getConsultationHistory(): Observable<ApiResponse<ProviderConsultation[]>> {
    return this.http.get<ApiResponse<ProviderConsultation[]>>(`${this.apiUrl}/provider/history`);
  }

  // Add consultation notes
  addConsultationNotes(id: string, notes: string): Observable<ApiResponse<ProviderConsultation>> {
    return this.http.post<ApiResponse<ProviderConsultation>>(`${this.apiUrl}/${id}/notes`, { notes });
  }

  // Get consultation earnings
  getConsultationEarnings(period: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/provider/earnings/${period}`);
  }
} 