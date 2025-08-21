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
		console.log('🔍 [AUTH SERVICE] 🔐 Attempting login for:', email);
		
		return this.commonService.post<any>('/api/auth/login', loginData).pipe(
			tap(response => {
				console.log('🔍 [AUTH SERVICE] 📡 Login response received:', {
					statusCode: response.statusCode,
					hasData: !!response.data,
					hasUser: !!response.data?.user,
					hasToken: !!response.data?.token,
					fullResponse: response
				});
				
				if (response.statusCode === 200 && response.data) {
					const userData = response.data;
					console.log('🔍 [AUTH SERVICE] 🔍 User data structure:', {
						userData: userData,
						userDataKeys: Object.keys(userData),
						userObject: userData.user,
						userObjectKeys: userData.user ? Object.keys(userData.user) : 'No user object',
						token: userData.token,
						role: userData.user?.role
					});
					
					const user: AuthUser = {
						id: userData.user.id,
						email: userData.user.email,
						firstName: userData.user.firstName,
						lastName: userData.user.lastName,
						role: userData.user.role,
						token: userData.token,
						phoneNumber: userData.user.phoneNumber
					};
					
					console.log('🔍 [AUTH SERVICE] 👤 User object created:', {
						id: user.id,
						email: user.email,
						role: user.role,
						roleType: typeof user.role,
						hasToken: !!user.token,
						tokenLength: user.token ? user.token.length : 0
					});
					
					this.setCurrentUser(user);
					this.currentUserSubject.next(user);
					
					console.log('🔍 [AUTH SERVICE] ✅ User stored and subject updated');
					console.log('🔍 [AUTH SERVICE] 🔍 Current localStorage content:', localStorage.getItem('st_auth_user'));
					
					// 🔄 Redirect to appropriate portal after successful login
					console.log('🔍 [AUTH SERVICE] 🔄 Redirecting to portal...');
					this.redirectToPortal();
				} else {
					console.log('🔍 [AUTH SERVICE] ❌ Login failed or invalid response');
				}
			})
		);
	}

	register(userData: RegisterRequest): Observable<any> {
		return this.commonService.post<any>('/api/auth/register', userData);
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

	getToken(): string | null {
		const user = this.getCurrentUser();
		return user?.token || null;
	}

	setCurrentUser(user: AuthUser): void {
		localStorage.setItem(this.STORAGE_KEY, JSON.stringify(user));
	}

	isAdmin(): boolean {
		const user = this.getCurrentUser();
		const role = user?.role?.toLowerCase();
		return role === 'admin' || role === 'unknown' || user?.role === 'Unknown';
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
		console.log('🔍 [AUTH SERVICE] 🔄 redirectToPortal called with user:', {
			hasUser: !!user,
			userId: user?.id,
			userEmail: user?.email,
			userRole: user?.role,
			roleType: typeof user?.role,
			roleLowerCase: user?.role?.toLowerCase()
		});
		
		if (!user) {
			console.log('🔍 [AUTH SERVICE] ❌ No user found, redirecting to home');
			this.router.navigate(['/']);
			return;
		}

		const role = user.role?.toLowerCase();
		console.log('🔍 [AUTH SERVICE] 🔍 Processing role:', {
			originalRole: user.role,
			lowerCaseRole: role,
			roleComparison: {
				isAdmin: role === 'admin' || role === 'unknown',
				isClient: role === 'client',
				isProvider: role === 'provider'
			}
		});

		// Handle backend role mapping - "Unknown" role should be treated as "admin"
		let effectiveRole = role;
		if (role === 'unknown' || role === 'Unknown') {
			effectiveRole = 'admin';
			console.log('🔍 [AUTH SERVICE] 🔄 Mapping "Unknown" role to "admin"');
		}

		switch (effectiveRole) {
			case 'admin':
				console.log('🔍 [AUTH SERVICE] ✅ Redirecting to admin dashboard: /webadmin/dashboard');
				this.router.navigate(['/webadmin/dashboard']);
				break;
			case 'client':
				console.log('🔍 [AUTH SERVICE] ✅ Redirecting to user dashboard: /web/dashboard');
				this.router.navigate(['/web/dashboard']);
				break;
			case 'provider':
				console.log('🔍 [AUTH SERVICE] ✅ Redirecting to provider dashboard: /webprovider/dashboard');
				this.router.navigate(['/webprovider/dashboard']);
				break;
			default:
				console.log('🔍 [AUTH SERVICE] ❌ Unknown role, redirecting to home. Role was:', user.role);
				this.router.navigate(['/']);
		}
	}

	forgotPassword(email: string): Observable<any> {
		return this.commonService.post<any>('/api/auth/forgot-password', { email });
	}

	changePassword(currentPassword: string, newPassword: string, confirmPassword: string): Observable<any> {
		return this.commonService.post<any>('/api/auth/change-password', {
			currentPassword,
			newPassword,
			confirmNewPassword: confirmPassword
		});
	}
}
