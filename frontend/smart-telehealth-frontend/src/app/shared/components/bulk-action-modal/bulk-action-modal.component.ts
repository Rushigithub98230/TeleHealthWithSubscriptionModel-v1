import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BulkAction {
  id: string;
  label: string;
  description: string;
  icon: string;
  color: string;
  action: string;
}

@Component({
  selector: 'app-bulk-action-modal',
  templateUrl: './bulk-action-modal.component.html',
  styleUrls: ['./bulk-action-modal.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class BulkActionModalComponent {
  @Input() selectedIds: number[] = [];
  @Input() actionType: 'subscription' | 'user' | 'plan' = 'subscription';
  @Input() title = 'Bulk Actions';
  @Output() success = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  selectedAction: string | null = null;
  isLoading = false;
  additionalData: any = {};

  // Available bulk actions based on type
  get availableActions(): BulkAction[] {
    switch (this.actionType) {
      case 'subscription':
        return [
          { id: 'activate', label: 'Activate', description: 'Activate all selected subscriptions', icon: '✅', color: '#10b981', action: 'activate' },
          { id: 'suspend', label: 'Suspend', description: 'Suspend all selected subscriptions', icon: '⏸️', color: '#f59e0b', action: 'suspend' },
          { id: 'cancel', label: 'Cancel', description: 'Cancel all selected subscriptions', icon: '❌', color: '#ef4444', action: 'cancel' },
          { id: 'change-plan', label: 'Change Plan', description: 'Change plan for all selected subscriptions', icon: '🔄', color: '#3b82f6', action: 'change-plan' },
          { id: 'extend', label: 'Extend', description: 'Extend all selected subscriptions', icon: '📅', color: '#8b5cf6', action: 'extend' }
        ];
      case 'user':
        return [
          { id: 'activate', label: 'Activate', description: 'Activate all selected users', icon: '✅', color: '#10b981', action: 'activate' },
          { id: 'deactivate', label: 'Deactivate', description: 'Deactivate all selected users', icon: '⏸️', color: '#f59e0b', action: 'deactivate' },
          { id: 'delete', label: 'Delete', description: 'Delete all selected users', icon: '🗑️', color: '#ef4444', action: 'delete' },
          { id: 'change-role', label: 'Change Role', description: 'Change role for all selected users', icon: '👤', color: '#3b82f6', action: 'change-role' }
        ];
      case 'plan':
        return [
          { id: 'activate', label: 'Activate', description: 'Activate all selected plans', icon: '✅', color: '#10b981', action: 'activate' },
          { id: 'deactivate', label: 'Deactivate', description: 'Deactivate all selected plans', icon: '⏸️', color: '#f59e0b', action: 'deactivate' },
          { id: 'delete', label: 'Delete', description: 'Delete all selected plans', icon: '🗑️', color: '#ef4444', action: 'delete' }
        ];
      default:
        return [];
    }
  }

  selectAction(action: BulkAction): void {
    this.selectedAction = action.action;
    this.additionalData = {};
  }

  onCancel(): void {
    this.cancel.emit();
  }

  onConfirm(): void {
    if (!this.selectedAction) {
      return;
    }

    this.isLoading = true;
    setTimeout(() => {
      this.isLoading = false;
      this.success.emit();
    }, 2000);
  }

  getSelectedCount(): number {
    return this.selectedIds.length;
  }

  getActionTypeLabel(): string {
    switch (this.actionType) {
      case 'subscription':
        return 'subscriptions';
      case 'user':
        return 'users';
      case 'plan':
        return 'plans';
      default:
        return 'items';
    }
  }
}
