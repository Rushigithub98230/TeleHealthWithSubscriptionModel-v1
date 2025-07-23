import { CanActivateFn, Router } from '@angular/router';

function decodeJwt(token: string): any {
  try {
    const payload = token.split('.')[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('token');
  if (!token) {
    window.location.href = '/patient/login'; // Adjust as needed for route context
    return false;
  }
  // Role-based access
  const requiredRoles = route.data && route.data['roles'];
  if (requiredRoles && requiredRoles.length > 0) {
    const decoded = decodeJwt(token);
    const userRole = decoded?.role || decoded?.roles || decoded?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (Array.isArray(userRole)) {
      if (!userRole.some((r) => requiredRoles.includes(r))) {
        window.location.href = '/forbidden';
        return false;
      }
    } else {
      if (!requiredRoles.includes(userRole)) {
        window.location.href = '/forbidden';
        return false;
      }
    }
  }
  return true;
};
