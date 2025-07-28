import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface HealthAssessment {
  id: string;
  userId: string;
  categoryId: string;
  categoryName: string;
  status: string;
  score?: number;
  totalQuestions: number;
  answeredQuestions: number;
  createdAt: string;
  completedAt?: string;
  updatedAt: string;
}

export interface AssessmentQuestion {
  id: string;
  categoryId: string;
  question: string;
  type: string;
  options?: string[];
  required: boolean;
  order: number;
}

export interface AssessmentAnswer {
  questionId: string;
  answer: string;
  answerOptions?: string[];
}

export interface CreateAssessmentDto {
  categoryId: string;
}

export interface SubmitAssessmentDto {
  assessmentId: string;
  answers: AssessmentAnswer[];
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
export class HealthAssessmentService {
  private apiUrl = `${environment.apiUrl}/api/healthassessments`;

  constructor(private http: HttpClient) {}

  // Get user assessments
  getUserAssessments(): Observable<ApiResponse<HealthAssessment[]>> {
    return this.http.get<ApiResponse<HealthAssessment[]>>(`${this.apiUrl}/user`);
  }

  // Get assessment by ID
  getAssessmentById(id: string): Observable<ApiResponse<HealthAssessment>> {
    return this.http.get<ApiResponse<HealthAssessment>>(`${this.apiUrl}/${id}`);
  }

  // Create new assessment
  createAssessment(createDto: CreateAssessmentDto): Observable<ApiResponse<HealthAssessment>> {
    return this.http.post<ApiResponse<HealthAssessment>>(`${this.apiUrl}`, createDto);
  }

  // Get assessment questions
  getAssessmentQuestions(categoryId: string): Observable<ApiResponse<AssessmentQuestion[]>> {
    return this.http.get<ApiResponse<AssessmentQuestion[]>>(`${this.apiUrl}/questions/${categoryId}`);
  }

  // Submit assessment answers
  submitAssessment(submitDto: SubmitAssessmentDto): Observable<ApiResponse<HealthAssessment>> {
    return this.http.post<ApiResponse<HealthAssessment>>(`${this.apiUrl}/submit`, submitDto);
  }

  // Get assessment results
  getAssessmentResults(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/results`);
  }

  // Get assessment statistics
  getAssessmentStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/stats`);
  }

  // Get assessments by status
  getAssessmentsByStatus(status: string): Observable<ApiResponse<HealthAssessment[]>> {
    return this.http.get<ApiResponse<HealthAssessment[]>>(`${this.apiUrl}/status/${status}`);
  }

  // Get completed assessments
  getCompletedAssessments(): Observable<ApiResponse<HealthAssessment[]>> {
    return this.http.get<ApiResponse<HealthAssessment[]>>(`${this.apiUrl}/completed`);
  }

  // Get pending assessments
  getPendingAssessments(): Observable<ApiResponse<HealthAssessment[]>> {
    return this.http.get<ApiResponse<HealthAssessment[]>>(`${this.apiUrl}/pending`);
  }

  // Get available categories
  getAvailableCategories(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/categories`);
  }

  // Get assessment history
  getAssessmentHistory(): Observable<ApiResponse<HealthAssessment[]>> {
    return this.http.get<ApiResponse<HealthAssessment[]>>(`${this.apiUrl}/history`);
  }

  // Get assessment recommendations
  getAssessmentRecommendations(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/recommendations`);
  }

  // Save assessment progress
  saveAssessmentProgress(id: string, answers: AssessmentAnswer[]): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/${id}/progress`, { answers });
  }

  // Get assessment progress
  getAssessmentProgress(id: string): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/${id}/progress`);
  }

  // Delete assessment
  deleteAssessment(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  // Restart assessment
  restartAssessment(id: string): Observable<ApiResponse<HealthAssessment>> {
    return this.http.post<ApiResponse<HealthAssessment>>(`${this.apiUrl}/${id}/restart`, {});
  }
} 