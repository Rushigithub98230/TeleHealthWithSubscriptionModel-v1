import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProviderOnboarding {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  specialty: string;
  subSpecialty?: string;
  licenseNumber: string;
  licenseState: string;
  npiNumber?: string;
  deaNumber?: string;
  education?: string;
  workHistory?: string;
  malpracticeInsurance?: string;
  bio?: string;
  profilePhotoUrl?: string;
  governmentIdUrl?: string;
  licenseDocumentUrl?: string;
  certificationDocumentUrl?: string;
  malpracticeInsuranceUrl?: string;
  status: string;
  adminRemarks?: string;
  submittedAt?: string;
  reviewedAt?: string;
  reviewedByUserId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProviderOnboardingDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  specialty: string;
  subSpecialty?: string;
  licenseNumber: string;
  licenseState: string;
  npiNumber?: string;
  deaNumber?: string;
  education?: string;
  workHistory?: string;
  malpracticeInsurance?: string;
  bio?: string;
  profilePhotoUrl?: string;
  governmentIdUrl?: string;
  licenseDocumentUrl?: string;
  certificationDocumentUrl?: string;
  malpracticeInsuranceUrl?: string;
}

export interface UpdateProviderOnboardingDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  specialty?: string;
  subSpecialty?: string;
  licenseNumber?: string;
  licenseState?: string;
  npiNumber?: string;
  deaNumber?: string;
  education?: string;
  workHistory?: string;
  malpracticeInsurance?: string;
  bio?: string;
  profilePhotoUrl?: string;
  governmentIdUrl?: string;
  licenseDocumentUrl?: string;
  certificationDocumentUrl?: string;
  malpracticeInsuranceUrl?: string;
}

export interface ReviewProviderOnboardingDto {
  status: string;
  adminRemarks?: string;
}

export interface OnboardingStatistics {
  totalOnboardings: number;
  pendingOnboardings: number;
  underReviewOnboardings: number;
  approvedOnboardings: number;
  rejectedOnboardings: number;
  requiresMoreInfoOnboardings: number;
  approvalRate: number;
  averageProcessingTimeDays: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  statusCode: number;
  errors?: any;
}

@Injectable({
  providedIn: 'root'
})
export class ProviderOnboardingService {
  private apiUrl = `${environment.apiUrl}/api/provideronboarding`;

  constructor(private http: HttpClient) {}

  // Create new onboarding application
  createOnboarding(createDto: CreateProviderOnboardingDto): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.post<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}`, createDto);
  }

  // Get onboarding by ID
  getOnboarding(id: string): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.get<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}/${id}`);
  }

  // Get onboarding by user ID
  getOnboardingByUser(userId: string): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.get<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}/user/${userId}`);
  }

  // Update onboarding application
  updateOnboarding(id: string, updateDto: UpdateProviderOnboardingDto): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.put<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}/${id}`, updateDto);
  }

  // Submit onboarding for review
  submitOnboarding(id: string): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.post<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}/${id}/submit`, {});
  }

  // Review onboarding (Admin only)
  reviewOnboarding(id: string, reviewDto: ReviewProviderOnboardingDto): Observable<ApiResponse<ProviderOnboarding>> {
    return this.http.post<ApiResponse<ProviderOnboarding>>(`${this.apiUrl}/${id}/review`, reviewDto);
  }

  // Get all onboarding applications with optional filtering
  getAllOnboardings(status?: string, page: number = 1, pageSize: number = 50): Observable<ApiResponse<ProviderOnboarding[]>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<ApiResponse<ProviderOnboarding[]>>(`${this.apiUrl}`, { params });
  }

  // Get pending onboarding applications
  getPendingOnboardings(): Observable<ApiResponse<ProviderOnboarding[]>> {
    return this.http.get<ApiResponse<ProviderOnboarding[]>>(`${this.apiUrl}/pending`);
  }

  // Get onboarding applications by status
  getOnboardingsByStatus(status: string): Observable<ApiResponse<ProviderOnboarding[]>> {
    return this.http.get<ApiResponse<ProviderOnboarding[]>>(`${this.apiUrl}/status/${status}`);
  }

  // Delete onboarding application
  deleteOnboarding(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  // Get onboarding statistics
  getOnboardingStatistics(): Observable<ApiResponse<OnboardingStatistics>> {
    return this.http.get<ApiResponse<OnboardingStatistics>>(`${this.apiUrl}/statistics`);
  }

  // Helper method to handle API errors
  handleError(error: any): string {
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.message) {
      return error.message;
    }
    return 'An unexpected error occurred';
  }

  // Helper method to check if response is successful
  isSuccessResponse(response: ApiResponse<any>): boolean {
    return response.success && response.statusCode >= 200 && response.statusCode < 300;
  }
} 