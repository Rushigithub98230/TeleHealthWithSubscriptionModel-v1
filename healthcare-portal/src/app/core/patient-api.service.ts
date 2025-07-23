import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Patient {
  id: string;
  name: string;
  email: string;
  // Add more fields as needed
}

@Injectable({
  providedIn: 'root'
})
export class PatientApiService {
  private baseUrl = '/api/Users';

  constructor(private http: HttpClient) { }

  getAllPatients(): Observable<Patient[]> {
    return this.http.get<any>(`${this.baseUrl}?userType=Patient`).pipe(map(res => res.data));
  }

  getPatient(id: string): Observable<Patient> {
    return this.http.get<any>(`${this.baseUrl}/${id}`).pipe(map(res => res.data));
  }

  createPatient(patient: Partial<Patient>): Observable<Patient> {
    return this.http.post<any>(`${this.baseUrl}`, patient).pipe(map(res => res.data));
  }

  updatePatient(id: string, patient: Partial<Patient>): Observable<Patient> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, patient).pipe(map(res => res.data));
  }

  deletePatient(id: string): Observable<void> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`).pipe(map(res => res.data));
  }
}
