import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { TableColumn, TableAction } from '../../../shared/global-table/global-table.component';
import { AdminApiService, AuditLog, AuditLogSearch } from '../../../core/admin-api.service';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

@Component({
  selector: 'app-audit-log-management',
  standalone: true,
  imports: [ReactiveFormsModule, MatPaginatorModule],
  templateUrl: './audit-log-management.component.html',
  styleUrl: './audit-log-management.component.scss'
})
export class AuditLogManagementComponent implements OnInit {
  auditLogs: AuditLog[] = [];
  loading = false;
  error: string | null = null;
  showDetailsModal = false;
  selectedLog: any = null;
  filterForm!: FormGroup;
  page = 1;
  pageSize = 50;
  totalLogs = 0;
  auditColumns: TableColumn[] = [
    { key: 'createdAt', label: 'Date' },
    { key: 'userId', label: 'User' },
    { key: 'action', label: 'Action' },
    { key: 'entityType', label: 'Entity' },
    { key: 'entityId', label: 'Entity ID' },
    { key: 'description', label: 'Description' }
  ];
  auditActions: TableAction[] = [
    { label: 'Details', action: 'details' }
  ];
  selectedIds = new Set<string>();

  constructor(private adminApi: AdminApiService) {}

  ngOnInit() {
    this.filterForm = new FormGroup({
      userId: new FormControl(''),
      action: new FormControl(''),
      entityType: new FormControl(''),
      startDate: new FormControl(''),
      endDate: new FormControl(''),
      search: new FormControl('')
    });
    this.fetchAuditLogs();
    this.filterForm.valueChanges.subscribe(() => this.fetchAuditLogs());
  }

  fetchAuditLogs() {
    this.loading = true;
    this.error = null;
    const filters: AuditLogSearch = {
      ...this.filterForm.value,
      page: this.page,
      pageSize: this.pageSize,
      searchTerm: this.filterForm.value.search
    };
    this.adminApi.getAuditLogs(filters).subscribe({
      next: (logs: any) => {
        this.auditLogs = Array.isArray(logs.items) ? logs.items : logs;
        this.totalLogs = logs.totalCount || logs.length || 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load audit logs';
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.fetchAuditLogs();
  }

  get filteredAuditLogs(): AuditLog[] {
    return this.auditLogs;
  }

  onTableSelectionChange(ids: string[]) {
    this.selectedIds = new Set(ids);
  }

  onTableAction(event: { action: string, row: any }) {
    if (event.action === 'details') {
      this.selectedLog = event.row;
      this.showDetailsModal = true;
    }
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedLog = null;
  }
} 