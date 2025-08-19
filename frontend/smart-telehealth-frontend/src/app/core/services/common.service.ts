import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { JsonModel } from '../models/index';

@Injectable({
  providedIn: 'root'
})
export class CommonService {
  private readonly baseUrl = 'http://localhost:61376/api'; // Updated to match backend port

  constructor(private http: HttpClient) {}

  /**
   * GET request
   */
  get<T>(endpoint: string, params?: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          if (Array.isArray(params[key])) {
            params[key].forEach((value: any) => {
              httpParams = httpParams.append(key, value);
            });
          } else {
            httpParams = httpParams.set(key, params[key]);
          }
        }
      });
    }

    const requestOptions = {
      params: httpParams,
      ...options
    };

    return this.http.get(url, requestOptions).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * POST request
   */
  post<T>(endpoint: string, data: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.post(url, data, options).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * PUT request
   */
  put<T>(endpoint: string, data: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.put(url, data, options).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * DELETE request
   */
  delete<T>(endpoint: string, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.delete(url, options).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * PATCH request
   */
  patch<T>(endpoint: string, data: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    return this.http.patch(url, data, options).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * GET request for blob responses (like exports)
   */
  getBlob(endpoint: string, params?: any, options?: any): Observable<Blob> {
    const url = `${this.baseUrl}${endpoint}`;
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          if (Array.isArray(params[key])) {
            params[key].forEach((value: any) => {
              httpParams = httpParams.append(key, value);
            });
          } else {
            httpParams = httpParams.set(key, params[key]);
          }
        }
      });
    }

    const requestOptions = {
      params: httpParams,
      responseType: 'blob' as 'json',
      ...options
    };

    return this.http.get(url, requestOptions).pipe(
      map(response => response as unknown as Blob),
      catchError(this.handleError)
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      if (error.status === 0) {
        errorMessage = 'Unable to connect to the server. Please check your internet connection.';
      } else if (error.status === 401) {
        errorMessage = 'Unauthorized. Please log in again.';
      } else if (error.status === 403) {
        errorMessage = 'Access denied. You do not have permission to perform this action.';
      } else if (error.status === 404) {
        errorMessage = 'The requested resource was not found.';
      } else if (error.status === 500) {
        errorMessage = 'Internal server error. Please try again later.';
      } else {
        errorMessage = error.error?.message || `Server error: ${error.status}`;
      }
    }

    console.error('HTTP Error:', error);
    return throwError(() => new Error(errorMessage));
  }

  /**
   * Set base URL (useful for different environments)
   */
  setBaseUrl(url: string): void {
    (this as any).baseUrl = url;
  }

  /**
   * Get current base URL
   */
  getBaseUrl(): string {
    return (this as any).baseUrl;
  }
}
