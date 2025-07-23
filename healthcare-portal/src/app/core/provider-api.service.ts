import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Provider {
  id: string;
  name: string;
  email: string;
  // Add more fields as needed
}

@Injectable({
  providedIn: 'root'
})
export class ProviderApiService {
  private baseUrl = '/api/Providers';

  constructor(private http: HttpClient) { }

  getAllProviders(): Observable<Provider[]> {
    return this.http.get<any>(`${this.baseUrl}`).pipe(map(res => res.data));
  }

  getProvider(id: string): Observable<Provider> {
    return this.http.get<any>(`${this.baseUrl}/${id}`).pipe(map(res => res.data));
  }

  createProvider(provider: Partial<Provider>): Observable<Provider> {
    return this.http.post<any>(`${this.baseUrl}`, provider).pipe(map(res => res.data));
  }

  updateProvider(id: string, provider: Partial<Provider>): Observable<Provider> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, provider).pipe(map(res => res.data));
  }

  deleteProvider(id: string): Observable<void> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`).pipe(map(res => res.data));
  }
}
