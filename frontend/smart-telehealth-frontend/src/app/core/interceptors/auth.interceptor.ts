import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

export const AuthInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const router = inject(Router);

  // 🔍 DEBUG: Log the request details
  console.log('🔍 [AUTH INTERCEPTOR] Intercepting request:', {
    url: request.url,
    method: request.method,
    hasAuthHeader: !!request.headers.get('Authorization')
  });

  // Get the current user and token directly from localStorage to avoid circular dependency
  const authUser = localStorage.getItem('st_auth_user');
  console.log('🔍 [AUTH INTERCEPTOR] Raw localStorage data:', authUser);
  
  const token = authUser ? JSON.parse(authUser)?.token : null;
  console.log('🔍 [AUTH INTERCEPTOR] Extracted token:', {
    hasToken: !!token,
    tokenLength: token ? token.length : 0,
    tokenPreview: token ? `${token.substring(0, 20)}...` : 'null'
  });

  // Clone the request and add the authorization header if token exists
  if (token) {
    console.log('🔍 [AUTH INTERCEPTOR] ✅ Adding token to request headers');
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('🔍 [AUTH INTERCEPTOR] ✅ Request headers after adding token:', {
      hasAuthHeader: !!request.headers.get('Authorization'),
      authHeaderValue: request.headers.get('Authorization')
    });
  } else {
    console.log('🔍 [AUTH INTERCEPTOR] ❌ No token found, request will proceed without auth');
  }

  // Handle the request and catch 401 errors
  console.log('🔍 [AUTH INTERCEPTOR] Sending request to backend...');
  
  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      console.log('🔍 [AUTH INTERCEPTOR] ❌ Request failed with error:', {
        status: error.status,
        statusText: error.statusText,
        url: error.url,
        message: error.message
      });
      
      if (error.status === 401) {
        // Token expired or invalid, logout user and redirect to login
        console.warn('🔍 [AUTH INTERCEPTOR] 🚨 401 Unauthorized - Token expired or invalid, logging out user');
        
        // Clear localStorage directly to avoid circular dependency
        localStorage.removeItem('st_auth_user');
        console.log('🔍 [AUTH INTERCEPTOR] ✅ Cleared localStorage');
        
        // Redirect to appropriate login page based on current route
        const currentUrl = router.url;
        console.log('🔍 [AUTH INTERCEPTOR] Current URL for redirect:', currentUrl);
        
        if (currentUrl.includes('/admin') || currentUrl.includes('/webadmin')) {
          console.log('🔍 [AUTH INTERCEPTOR] 🔄 Redirecting to admin login');
          router.navigate(['/webadmin/login']);
        } else if (currentUrl.includes('/user') || currentUrl.includes('/web')) {
          console.log('🔍 [AUTH INTERCEPTOR] 🔄 Redirecting to user login');
          router.navigate(['/web/login']);
        } else {
          console.log('🔍 [AUTH INTERCEPTOR] 🔄 Redirecting to home');
          router.navigate(['/']);
        }
      }
      return throwError(() => error);
    })
  );
};
