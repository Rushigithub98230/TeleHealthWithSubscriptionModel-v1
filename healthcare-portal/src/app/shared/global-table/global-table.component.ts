import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface TableColumn {
  key: string;
  label: string;
  cellTemplate?: any; // For custom cell templates
}

export interface TableAction {
  label: string;
  icon?: string;
  action: string;
  color?: string;
}

@Component({
  selector: 'app-global-table',
  templateUrl: './global-table.component.html',
  styleUrls: ['./global-table.component.scss']
})
export class GlobalTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() loading = false;
  @Input() selectable = false;
  @Input() actions: TableAction[] = [];
  @Output() selectionChange = new EventEmitter<string[]>();
  @Output() action = new EventEmitter<{ action: string, row: any }>();

  selectedIds = new Set<string>();

  toggleSelection(id: string) {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
    this.selectionChange.emit(Array.from(this.selectedIds));
  }

  isSelected(id: string): boolean {
    return this.selectedIds.has(id);
  }

  selectAll() {
    this.data.forEach(row => this.selectedIds.add(row.id));
    this.selectionChange.emit(Array.from(this.selectedIds));
  }

  clearSelection() {
    this.selectedIds.clear();
    this.selectionChange.emit([]);
  }

  isAllSelected(): boolean {
    return this.data.length > 0 && this.data.every(row => this.selectedIds.has(row.id));
  }

  onAction(action: string, row: any) {
    this.action.emit({ action, row });
  }
} 