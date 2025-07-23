import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AdminApiService } from './admin-api.service';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'auth_token';

  constructor(
    private api: AdminApiService,
    private router: Router
  ) {}

  login(username: string, password: string): Observable<{ token: string }> {
    return this.api.login({ username, password }).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['/admin/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
} 