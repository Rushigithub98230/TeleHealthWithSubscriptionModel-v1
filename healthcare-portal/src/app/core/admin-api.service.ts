import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  status: string;
}

export interface Plan {
  id: string;
  name: string;
  price: number;
  duration: string;
  status: string;
}

export interface AdminDashboardStats {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalRevenue: number;
  monthlyRecurringRevenue: number;
  totalUsers: number;
  totalProviders: number;
  // Add more fields as needed from backend DTO
}

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private userBaseUrl = '/api/admin/users';
  private planBaseUrl = '/api/SubscriptionPlans';
  private dashboardUrl = '/api/Admin/dashboard';

  constructor(private http: HttpClient) { }

  // User management
  getUsers(): Observable<User[]> {
    return this.http.get<any>(`${this.userBaseUrl}`).pipe(map(res => res.data));
  }
  addUser(user: Partial<User>): Observable<User> {
    return this.http.post<any>(`${this.userBaseUrl}`, user).pipe(map(res => res.data));
  }
  updateUser(id: number, user: Partial<User>): Observable<User> {
    return this.http.put<any>(`${this.userBaseUrl}/${id}`, user).pipe(map(res => res.data));
  }
  deleteUser(id: number): Observable<void> {
    return this.http.delete<any>(`${this.userBaseUrl}/${id}`).pipe(map(res => res.data));
  }
  assignRole(id: number, role: string): Observable<User> {
    return this.http.patch<any>(`${this.userBaseUrl}/${id}/role`, { role }).pipe(map(res => res.data));
  }

  // Plan management
  getPlans(): Observable<Plan[]> {
    return this.http.get<any>(`${this.planBaseUrl}`).pipe(map(res => res.data));
  }
  addPlan(plan: Partial<Plan>): Observable<Plan> {
    return this.http.post<any>(`${this.planBaseUrl}`, plan).pipe(map(res => res.data));
  }
  updatePlan(id: string, plan: Partial<Plan>): Observable<Plan> {
    return this.http.put<any>(`${this.planBaseUrl}/${id}`, plan).pipe(map(res => res.data));
  }
  deletePlan(id: string): Observable<void> {
    return this.http.delete<any>(`${this.planBaseUrl}/${id}`).pipe(map(res => res.data));
  }

  // Dashboard statistics
  getAdminDashboardStats(): Observable<AdminDashboardStats> {
    return this.http.get<any>(this.dashboardUrl).pipe(map(res => res.data));
  }
}
