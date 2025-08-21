import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface AdminUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
  lastLoginAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: string;
  password: string;
  confirmPassword: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface LoginResponse {
  user: AdminUser;
  token: string;
  refreshToken: string;
  expiresAt: Date;
}

export interface JsonModel<T = any> {
  data: T;
  message: string;
  statusCode: number;
  meta?: any;
}

@Injectable({
  providedIn: 'root'
})
export class AdminAuthService {
  private readonly baseUrl = environment.apiUrl;
  private readonly tokenKey = 'admin_auth_token';
  private readonly refreshTokenKey = 'admin_refresh_token';
  private readonly userKey = 'admin_user';

  private currentUserSubject = new BehaviorSubject<AdminUser | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = this.getStoredToken();
    const user = this.getStoredUser();

    if (token && user && this.isTokenValid(token)) {
      this.tokenSubject.next(token);
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    } else {
      this.clearStoredAuth();
    }
  }

  // Login
  login(credentials: LoginRequest): Observable<JsonModel<LoginResponse>> {
    return this.http.post<JsonModel<LoginResponse>>(
      `${this.baseUrl}/api/auth/login`,
      credentials
    ).pipe(
      tap(response => {
        if (response.statusCode === 200 && response.data) {
          this.handleSuccessfulLogin(response.data, credentials.rememberMe);
        }
      }),
      catchError(error => {
        this.handleLoginError(error);
        throw error;
      })
    );
  }

  // Logout
  logout(): void {
    const token = this.getStoredToken();
    
    if (token) {
      // Call logout endpoint to invalidate token on server
      this.http.post(`${this.baseUrl}/api/auth/logout`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      }).subscribe({
        error: () => {
          // Continue with local logout even if server call fails
        }
      });
    }

    this.clearAuth();
    this.router.navigate(['/webadmin/login']);
  }

  // Refresh token
  refreshToken(): Observable<JsonModel<{ token: string; refreshToken: string }>> {
    const refreshToken = this.getStoredRefreshToken();
    
    if (!refreshToken) {
      return of({ data: { token: '', refreshToken: '' }, message: 'No refresh token', statusCode: 401 });
    }

    return this.http.post<JsonModel<{ token: string; refreshToken: string }>>(
      `${this.baseUrl}/api/auth/refresh-token`,
      { refreshToken }
    ).pipe(
      tap(response => {
        if (response.statusCode === 200 && response.data) {
          this.updateToken(response.data.token, response.data.refreshToken);
        }
      }),
      catchError(error => {
        this.handleTokenRefreshError(error);
        throw error;
      })
    );
  }

  // Forgot password
  forgotPassword(email: string): Observable<JsonModel<boolean>> {
    return this.http.post<JsonModel<boolean>>(
      `${this.baseUrl}/api/auth/forgot-password`,
      { email }
    );
  }

  // Reset password
  resetPassword(token: string, newPassword: string): Observable<JsonModel<boolean>> {
    return this.http.post<JsonModel<boolean>>(
      `${this.baseUrl}/api/auth/reset-password`,
      { token, newPassword }
    );
  }

  // Change password
  changePassword(currentPassword: string, newPassword: string): Observable<JsonModel<boolean>> {
    const token = this.getStoredToken();
    
    if (!token) {
      return of({ data: false, message: 'Not authenticated', statusCode: 401 });
    }

    return this.http.post<JsonModel<boolean>>(
      `${this.baseUrl}/api/auth/change-password`,
      { currentPassword, newPassword },
      { headers: { Authorization: `Bearer ${token}` } }
    );
  }

  // Register
  register(user: RegisterRequest): Observable<JsonModel<AdminUser>> {
    return this.http.post<JsonModel<AdminUser>>(
      `${this.baseUrl}/api/auth/register`,
      user
    );
  }

  // Get current user
  getCurrentUser(): AdminUser | null {
    return this.currentUserSubject.value;
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  // Check if user has specific role
  hasRole(role: string | string[]): boolean {
    const user = this.getCurrentUser();
    if (!user) return false;

    if (Array.isArray(role)) {
      return role.includes(user.role);
    }

    return user.role === role;
  }

  // Check if user is admin
  isAdmin(): boolean {
    return this.hasRole(['Admin', 'SuperAdmin']);
  }

  // Check if user is super admin
  isSuperAdmin(): boolean {
    return this.hasRole('SuperAdmin');
  }

  // Get stored token
  getStoredToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Get stored refresh token
  getStoredRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  // Get stored user
  getStoredUser(): AdminUser | null {
    const userStr = localStorage.getItem(this.userKey);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  // Check if token is valid
  private isTokenValid(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = new Date(payload.exp * 1000);
      return expiry > new Date();
    } catch {
      return false;
    }
  }

  // Handle successful login
  private handleSuccessfulLogin(response: LoginResponse, rememberMe: boolean = false): void {
    this.updateToken(response.token, response.refreshToken);
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);

    if (rememberMe) {
      this.storeAuth(response.token, response.refreshToken, response.user);
    } else {
      this.storeAuth(response.token, response.refreshToken, response.user, true);
    }
  }

  // Update token
  private updateToken(token: string, refreshToken: string): void {
    this.tokenSubject.next(token);
    this.storeToken(token, refreshToken);
  }

  // Store authentication data
  private storeAuth(token: string, refreshToken: string, user: AdminUser, sessionOnly: boolean = false): void {
    if (sessionOnly) {
      sessionStorage.setItem(this.tokenKey, token);
      sessionStorage.setItem(this.refreshTokenKey, refreshToken);
      sessionStorage.setItem(this.userKey, JSON.stringify(user));
    } else {
      localStorage.setItem(this.tokenKey, token);
      localStorage.setItem(this.refreshTokenKey, refreshToken);
      localStorage.setItem(this.userKey, JSON.stringify(user));
    }
  }

  // Store token
  private storeToken(token: string, refreshToken: string): void {
    const currentToken = this.getStoredToken();
    const currentRefreshToken = this.getStoredRefreshToken();

    if (currentToken && currentRefreshToken) {
      // Update existing storage
      if (localStorage.getItem(this.tokenKey)) {
        localStorage.setItem(this.tokenKey, token);
        localStorage.setItem(this.refreshTokenKey, refreshToken);
      } else {
        sessionStorage.setItem(this.tokenKey, token);
        sessionStorage.setItem(this.refreshTokenKey, refreshToken);
      }
    } else {
      // Store in session storage by default
      sessionStorage.setItem(this.tokenKey, token);
      sessionStorage.setItem(this.refreshTokenKey, refreshToken);
    }
  }

  // Clear stored authentication
  private clearStoredAuth(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.refreshTokenKey);
    sessionStorage.removeItem(this.userKey);
  }

  // Clear authentication state
  private clearAuth(): void {
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.tokenSubject.next(null);
    this.clearStoredAuth();
  }

  // Handle login error
  private handleLoginError(error: any): void {
    console.error('Login error:', error);
    this.clearAuth();
  }

  // Handle token refresh error
  private handleTokenRefreshError(error: any): void {
    console.error('Token refresh error:', error);
    this.clearAuth();
    this.router.navigate(['/webadmin/login']);
  }

  // Auto-refresh token before expiry
  startTokenRefreshTimer(): void {
    const token = this.getStoredToken();
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = new Date(payload.exp * 1000);
      const now = new Date();
      const timeUntilExpiry = expiry.getTime() - now.getTime();
      
      // Refresh token 5 minutes before expiry
      const refreshTime = Math.max(timeUntilExpiry - (5 * 60 * 1000), 0);
      
      setTimeout(() => {
        this.refreshToken().subscribe({
          next: () => {
            // Restart timer for new token
            this.startTokenRefreshTimer();
          },
          error: () => {
            // Token refresh failed, logout user
            this.logout();
          }
        });
      }, refreshTime);
    } catch {
      // Invalid token, logout user
      this.logout();
    }
  }

  // Validate current session
  validateSession(): Observable<boolean> {
    const token = this.getStoredToken();
    
    if (!token) {
      return of(false);
    }

    return this.http.get<JsonModel<AdminUser>>(
      `${this.baseUrl}/webadmin/validate-session`,
      { headers: { Authorization: `Bearer ${token}` } }
    ).pipe(
      map(response => response.statusCode === 200),
      catchError(() => of(false))
    );
  }

  // Get user permissions
  getUserPermissions(): Observable<string[]> {
    const token = this.getStoredToken();
    
    if (!token) {
      return of([]);
    }

    return this.http.get<JsonModel<string[]>>(
      `${this.baseUrl}/webadmin/permissions`,
      { headers: { Authorization: `Bearer ${token}` } }
    ).pipe(
      map(response => response.data || []),
      catchError(() => of([]))
    );
  }

  // Check if user has permission
  hasPermission(permission: string | string[]): Observable<boolean> {
    return this.getUserPermissions().pipe(
      map(permissions => {
        if (Array.isArray(permission)) {
          return permission.some(p => permissions.includes(p));
        }
        return permissions.includes(permission);
      })
    );
  }
}
