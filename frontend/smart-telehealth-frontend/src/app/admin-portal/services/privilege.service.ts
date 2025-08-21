import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService } from '../../core/services/common.service';
import { JsonModel } from '../../core/models/index';

export interface Privilege {
  id: number;
  name: string;
  description: string;
  category: string;
  isActive: boolean;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreatePrivilegeRequest {
  name: string;
  description: string;
  category: string;
  isActive: boolean;
  sortOrder: number;
}

export interface UpdatePrivilegeRequest extends Partial<CreatePrivilegeRequest> {
  id: number;
}

export interface PrivilegeListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  status?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PrivilegeService {
  private readonly API_PATHS = {
    PRIVILEGES: '/api/Privileges',
    PRIVILEGES_EXPORT: '/api/Privileges/export'
  } as const;

  constructor(private commonService: CommonService) {}

  // Get all privileges with pagination and filters
  getPrivileges(params: PrivilegeListParams = {}): Observable<JsonModel<Privilege[]>> {
    return this.commonService.get<Privilege[]>(this.API_PATHS.PRIVILEGES, params);
  }

  // Get privilege by ID
  getPrivilegeById(id: number): Observable<JsonModel<Privilege>> {
    const path = `${this.API_PATHS.PRIVILEGES}/${id}`;
    return this.commonService.get<Privilege>(path);
  }

  // Create new privilege
  createPrivilege(privilegeData: CreatePrivilegeRequest): Observable<JsonModel<Privilege>> {
    return this.commonService.post<Privilege>(this.API_PATHS.PRIVILEGES, privilegeData);
  }

  // Update existing privilege
  updatePrivilege(id: number, privilegeData: UpdatePrivilegeRequest): Observable<JsonModel<Privilege>> {
    const path = `${this.API_PATHS.PRIVILEGES}/${id}`;
    return this.commonService.put<Privilege>(path, privilegeData);
  }

  // Delete privilege
  deletePrivilege(id: number): Observable<JsonModel<any>> {
    const path = `${this.API_PATHS.PRIVILEGES}/${id}`;
    return this.commonService.delete<any>(path);
  }

  // Export privileges
  exportPrivileges(params: Omit<PrivilegeListParams, 'page' | 'pageSize'> = {}): Observable<Blob> {
    return this.commonService.getBlob(this.API_PATHS.PRIVILEGES_EXPORT, params);
  }

  // Get privilege categories
  getPrivilegeCategories(): string[] {
    return [
      'Telehealth Services',
      'Medical Services',
      'Administrative',
      'Communication',
      'Reporting'
    ];
  }

  // Get status options
  getStatusOptions(): Array<{ value: string; label: string; color: string }> {
    return [
      { value: 'active', label: 'Active', color: '#10b981' },
      { value: 'inactive', label: 'Inactive', color: '#6b7280' }
    ];
  }

  // Get category colors
  getCategoryColor(category: string): string {
    const colors: { [key: string]: string } = {
      'Telehealth Services': '#3b82f6',
      'Medical Services': '#10b981',
      'Administrative': '#f59e0b',
      'Communication': '#8b5cf6',
      'Reporting': '#ef4444'
    };
    return colors[category] || '#6b7280';
  }

  // Get status color
  getStatusColor(isActive: boolean): string {
    return isActive ? '#10b981' : '#6b7280';
  }

  // Get status label
  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }
}
