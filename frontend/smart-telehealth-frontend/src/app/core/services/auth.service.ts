import { Injectable } from '@angular/core';
import { CommonService } from './common.service';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';

export interface AuthUser {
	id: number;
	email: string;
	firstName?: string;
	lastName?: string;
	role?: string;
	token?: string;
	phoneNumber?: string;
}

export interface LoginRequest {
	email: string;
	password: string;
}

export interface RegisterRequest {
	firstName: string;
	lastName: string;
	email: string;
	password: string;
	confirmPassword: string;
	phoneNumber: string;
	gender: string;
	address: string;
	city: string;
	state: string;
	zipCode: string;
	role?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
	private readonly STORAGE_KEY = 'st_auth_user';
	private currentUserSubject = new BehaviorSubject<AuthUser | null>(null);
	public currentUser$ = this.currentUserSubject.asObservable();

	constructor(
		private commonService: CommonService,
		private router: Router
	) {
		// Initialize current user from localStorage
		const user = this.getCurrentUser();
		if (user) {
			this.currentUserSubject.next(user);
		}
	}

	login(email: string, password: string): Observable<any> {
		const loginData: LoginRequest = { email, password };
		return this.commonService.post<any>('/auth/login', loginData).pipe(
			tap(response => {
				if (response.statusCode === 200 && response.data) {
					const userData = response.data;
					const user: AuthUser = {
						id: userData.user.id,
						email: userData.user.email,
						firstName: userData.user.firstName,
						lastName: userData.user.lastName,
						role: userData.user.role,
						token: userData.token,
						phoneNumber: userData.user.phoneNumber
					};
					this.setCurrentUser(user);
					this.currentUserSubject.next(user);
				}
			})
		);
	}

	register(userData: RegisterRequest): Observable<any> {
		return this.commonService.post<any>('/auth/register', userData);
	}

	logout(): void {
		localStorage.removeItem(this.STORAGE_KEY);
		this.currentUserSubject.next(null);
		this.router.navigate(['/']);
	}

	isAuthenticated(): boolean {
		const user = this.getCurrentUser();
		return !!(user?.token);
	}

	getCurrentUser(): AuthUser | null {
		const raw = localStorage.getItem(this.STORAGE_KEY);
		return raw ? (JSON.parse(raw) as AuthUser) : null;
	}

	setCurrentUser(user: AuthUser): void {
		localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
	}

	isAdmin(): boolean {
		const user = this.getCurrentUser();
		return user?.role?.toLowerCase() === 'admin';
	}

	isUser(): boolean {
		const user = this.getCurrentUser();
		return user?.role?.toLowerCase() === 'client';
	}

	isProvider(): boolean {
		const user = this.getCurrentUser();
		return user?.role?.toLowerCase() === 'provider';
	}

	redirectToPortal(): void {
		const user = this.getCurrentUser();
		if (!user) {
			this.router.navigate(['/']);
			return;
		}

		switch (user.role?.toLowerCase()) {
			case 'admin':
				this.router.navigate(['/admin/dashboard']);
				break;
			case 'client':
				this.router.navigate(['/user/dashboard']);
				break;
			case 'provider':
				this.router.navigate(['/provider/dashboard']);
				break;
			default:
				this.router.navigate(['/']);
		}
	}

	forgotPassword(email: string): Observable<any> {
		return this.commonService.post<any>('/auth/forgot-password', { email });
	}

	changePassword(currentPassword: string, newPassword: string, confirmPassword: string): Observable<any> {
		return this.commonService.post<any>('/auth/change-password', {
			currentPassword,
			newPassword,
			confirmNewPassword: confirmPassword
		});
	}
}
