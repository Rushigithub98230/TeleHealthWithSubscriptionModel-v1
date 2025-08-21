import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { JsonModel } from '../models/index';

@Injectable({
  providedIn: 'root'
})
export class CommonService {
  private readonly baseUrl = 'http://localhost:61376'; // Backend base URL without /api

  constructor(private http: HttpClient) {}

  /**
   * Get headers with basic content type
   * Note: Authentication is handled by AuthInterceptor
   */
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

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
      headers: this.getHeaders(),
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
    const requestOptions = {
      headers: this.getHeaders(),
      ...options
    };

    return this.http.post(url, data, requestOptions).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * PUT request
   */
  put<T>(endpoint: string, data: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    const requestOptions = {
      headers: this.getHeaders(),
      ...options
    };

    return this.http.put(url, data, requestOptions).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * DELETE request
   */
  delete<T>(endpoint: string, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    const requestOptions = {
      headers: this.getHeaders(),
      ...options
    };

    return this.http.delete(url, requestOptions).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * PATCH request
   */
  patch<T>(endpoint: string, data: any, options?: any): Observable<JsonModel<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    const requestOptions = {
      headers: this.getHeaders(),
      ...options
    };

    return this.http.patch(url, data, requestOptions).pipe(
      map((response: any) => response as JsonModel<T>),
      catchError(this.handleError)
    );
  }

  /**
   * GET request for blob data (e.g., file downloads)
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
      headers: this.getHeaders(),
      responseType: 'blob' as 'json',
      ...options
    };

    return this.http.get(url, requestOptions).pipe(
      map((response: any) => response as Blob),
      catchError(this.handleError)
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse) {
    console.error('HTTP Error:', error);
    
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = error.error?.message || error.message || `Error Code: ${error.status}`;
    }
    
    return throwError(() => new Error(errorMessage));
  }
}
