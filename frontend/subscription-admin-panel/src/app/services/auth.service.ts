import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  data: {
    token: string;
    refreshToken?: string;
    user: {
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      role: string;
      phoneNumber?: string;
      isActive?: boolean;
    };
  };
  message: string;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private tokenSubject = new BehaviorSubject<string | null>(null);
  public token$ = this.tokenSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredAuth();
  }

  private loadStoredAuth(): void {
    try {
      const token = localStorage.getItem('auth_token');
      const user = localStorage.getItem('current_user');
      
      if (token && user && user !== 'undefined') {
        this.tokenSubject.next(token);
        const parsedUser = JSON.parse(user);
        this.currentUserSubject.next(parsedUser);
      }
    } catch (error) {
      console.warn('Error loading stored auth, clearing localStorage:', error);
      // Clear corrupted data
      localStorage.removeItem('auth_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('current_user');
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          console.log('Login response:', response);
          
          if (response.success && response.data) {
            localStorage.setItem('auth_token', response.data.token);
            if (response.data.refreshToken) {
              localStorage.setItem('refresh_token', response.data.refreshToken);
            }
            localStorage.setItem('current_user', JSON.stringify(response.data.user));
            
            this.tokenSubject.next(response.data.token);
            // Add isActive property if not present
            const userWithIsActive = {
              ...response.data.user,
              isActive: response.data.user.isActive ?? true
            };
            this.currentUserSubject.next(userWithIsActive);
          } else {
            throw new Error(response.message || 'Login failed');
          }
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('current_user');
    
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin' || user?.role === 'Superadmin';
  }

  refreshToken(): Observable<any> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post(`${environment.apiUrl}/api/auth/refresh`, { refreshToken })
      .pipe(
        tap((response: any) => {
          localStorage.setItem('auth_token', response.token);
          this.tokenSubject.next(response.token);
        }),
        catchError(error => {
          this.logout();
          return throwError(() => error);
        })
      );
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }
}
