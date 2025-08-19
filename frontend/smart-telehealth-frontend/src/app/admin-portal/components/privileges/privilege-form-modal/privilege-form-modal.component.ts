import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PrivilegeService, Privilege } from '../../../services/privilege.service';
import { ToastService } from '../../../../core/services/toast.service';


@Component({
  selector: 'app-privilege-form-modal',
  templateUrl: './privilege-form-modal.component.html',
  styleUrls: ['./privilege-form-modal.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class PrivilegeFormModalComponent implements OnInit {
  @Input() isCreateMode: boolean = true;
  @Input() privilege: Privilege | null = null;
  
  @Output() success = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  privilegeForm: FormGroup;
  isSubmitting = false;
  categories: string[] = [];

  constructor(
    private fb: FormBuilder,
    private privilegeService: PrivilegeService,
    private toastService: ToastService
  ) {
    this.privilegeForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      category: ['', Validators.required],
      isActive: [true],
      sortOrder: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.categories = this.privilegeService.getPrivilegeCategories();
    
    if (!this.isCreateMode && this.privilege) {
      this.privilegeForm.patchValue({
        name: this.privilege.name,
        description: this.privilege.description,
        category: this.privilege.category,
        isActive: this.privilege.isActive,
        sortOrder: this.privilege.sortOrder
      });
    }
  }

  onSubmit(): void {
    if (this.privilegeForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;
    const formData = this.privilegeForm.value;

    if (this.isCreateMode) {
      this.createPrivilege(formData);
    } else {
      this.updatePrivilege(formData);
    }
  }

  private createPrivilege(data: any): void {
    this.privilegeService.createPrivilege(data)
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.toastService.showSuccess('Privilege created successfully');
            this.success.emit();
          } else {
            this.toastService.showError(response.message || 'Failed to create privilege');
          }
        },
        error: (error) => {
          this.toastService.showError('Failed to create privilege');
          console.error('Error creating privilege:', error);
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }

  private updatePrivilege(data: any): void {
    if (!this.privilege) return;

    this.privilegeService.updatePrivilege(this.privilege.id, data)
      .subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.toastService.showSuccess('Privilege updated successfully');
            this.success.emit();
          } else {
            this.toastService.showError(response.message || 'Failed to update privilege');
          }
        },
        error: (error) => {
          this.toastService.showError('Failed to update privilege');
          console.error('Error updating privilege:', error);
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }

  onCancel(): void {
    this.cancel.emit();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.privilegeForm.controls).forEach(key => {
      const control = this.privilegeForm.get(key);
      control?.markAsTouched();
    });
  }

  getModalTitle(): string {
    return this.isCreateMode ? 'Create New Privilege' : 'Edit Privilege';
  }

  getSubmitButtonText(): string {
    return this.isSubmitting 
      ? (this.isCreateMode ? 'Creating...' : 'Updating...') 
      : (this.isCreateMode ? 'Create Privilege' : 'Update Privilege');
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.privilegeForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.privilegeForm.get(fieldName);
    if (field && field.errors && field.touched) {
      if (field.errors['required']) return 'This field is required';
      if (field.errors['minlength']) return `Minimum length is ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['maxlength']) return `Maximum length is ${field.errors['maxlength'].requiredLength} characters`;
      if (field.errors['min']) return 'Value must be 0 or greater';
    }
    return '';
  }
}
