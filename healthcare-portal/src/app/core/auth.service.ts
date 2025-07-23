import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = '/api/Auth';

  constructor(private http: HttpClient) { }

  login(data: LoginRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/login`, data).pipe(map(res => res.data));
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/register`, data).pipe(map(res => res.data));
  }

  refreshToken(data: RefreshTokenRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/refresh-token`, data).pipe(map(res => res.data));
  }

  changePassword(data: ChangePasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/change-password`, data).pipe(map(res => res.data));
  }

  forgotPassword(data: ForgotPasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/forgot-password`, data).pipe(map(res => res.data));
  }

  resetPassword(data: ResetPasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/reset-password`, data).pipe(map(res => res.data));
  }

  logout(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/logout`, {}).pipe(map(res => res.data));
  }

  getProfile(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/profile`).pipe(map(res => res.data));
  }

  updateProfile(profile: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/profile`, profile).pipe(map(res => res.data));
  }
}
