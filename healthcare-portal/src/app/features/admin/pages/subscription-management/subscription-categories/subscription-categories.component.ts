import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { AdminSubscriptionManagementService, AdminCategory, ApiResponse } from '../../../../../core/services/admin-subscription-management.service';

export interface SubscriptionCategory {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  basePrice: number;
  consultationFee: number;
  oneTimeConsultationFee: number;
  requiresHealthAssessment: boolean;
  allowsMedicationDelivery: boolean;
  allowsFollowUpMessaging: boolean;
  displayOrder: number;
  createdAt: Date;
  updatedAt: Date;
}

@Component({
  selector: 'app-subscription-categories',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatCheckboxModule,
    MatSlideToggleModule
  ],
  templateUrl: './subscription-categories.component.html',
  styleUrls: ['./subscription-categories.component.scss']
})
export class SubscriptionCategoriesComponent implements OnInit {
  categories: AdminCategory[] = [];
  filteredCategories: AdminCategory[] = [];
  loading = false;
  error = '';
  successMessage = '';
  showCreateModal = false;
  showEditModal = false;
  selectedCategory: AdminCategory | null = null;
  searchTerm = '';
  selectedStatus = '';

  statuses = ['active', 'inactive'];

  // Form data for create/edit
  newCategory = {
    name: '',
    description: '',
    isActive: true,
    basePrice: 0,
    consultationFee: 0,
    oneTimeConsultationFee: 0,
    requiresHealthAssessment: false,
    allowsMedicationDelivery: false,
    allowsFollowUpMessaging: false,
    displayOrder: 0
  };

  constructor(
    private subscriptionService: AdminSubscriptionManagementService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.error = '';

    this.subscriptionService.getAllCategories(
      this.searchTerm
    ).subscribe({
      next: (response: ApiResponse<AdminCategory[]>) => {
        if (response.success && response.data) {
          this.categories = response.data;
          this.filteredCategories = this.categories;
        } else {
          this.error = response.message || 'Failed to load categories';
        }
        this.loading = false;
      },
      error: (error: any) => {
        this.error = 'Error loading categories. Please try again.';
        this.loading = false;
        console.error('Error loading categories:', error);
      }
    });
  }

  onSearchChange(): void {
    this.loadCategories();
  }

  onFilterChange(): void {
    this.loadCategories();
  }

  openCreateModal(): void {
    this.resetNewCategory();
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.resetNewCategory();
  }

  openEditModal(category: AdminCategory): void {
    this.selectedCategory = category;
    this.newCategory = {
      name: category.name,
      description: category.description || '',
      isActive: category.isActive,
      basePrice: category.basePrice,
      consultationFee: category.consultationFee,
      oneTimeConsultationFee: category.oneTimeConsultationFee,
      requiresHealthAssessment: category.requiresHealthAssessment,
      allowsMedicationDelivery: category.allowsMedicationDelivery,
      allowsFollowUpMessaging: category.allowsFollowUpMessaging,
      displayOrder: category.displayOrder
    };
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedCategory = null;
    this.resetNewCategory();
  }

  resetNewCategory(): void {
    this.newCategory = {
      name: '',
      description: '',
      isActive: true,
      basePrice: 0,
      consultationFee: 0,
      oneTimeConsultationFee: 0,
      requiresHealthAssessment: false,
      allowsMedicationDelivery: false,
      allowsFollowUpMessaging: false,
      displayOrder: 0
    };
  }

  createCategory(): void {
    if (!this.validateCategory()) return;

    this.loading = true;
    
    this.subscriptionService.createCategory(this.newCategory)
      .subscribe({
        next: (response: ApiResponse<AdminCategory>) => {
          if (response.success) {
            this.successMessage = 'Category created successfully';
            this.closeCreateModal();
            this.loadCategories();
          } else {
            this.error = response.message || 'Failed to create category';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'Error creating category. Please try again.';
          this.loading = false;
          console.error('Error creating category:', error);
        }
      });
  }

  updateCategory(): void {
    if (!this.selectedCategory || !this.validateCategory()) return;

    this.loading = true;
    
    this.subscriptionService.updateCategory(this.selectedCategory.id, this.newCategory)
      .subscribe({
        next: (response: ApiResponse<AdminCategory>) => {
          if (response.success) {
            this.successMessage = 'Category updated successfully';
            this.closeEditModal();
            this.loadCategories();
          } else {
            this.error = response.message || 'Failed to update category';
          }
          this.loading = false;
        },
        error: (error: any) => {
          this.error = 'Error updating category. Please try again.';
          this.loading = false;
          console.error('Error updating category:', error);
        }
      });
  }

  deleteCategory(category: AdminCategory): void {
    if (confirm(`Are you sure you want to delete the category "${category.name}"?`)) {
      this.loading = true;
      
      this.subscriptionService.deleteCategory(category.id)
        .subscribe({
          next: (response: ApiResponse<any>) => {
            if (response.success) {
              this.successMessage = 'Category deleted successfully';
              this.loadCategories();
            } else {
              this.error = response.message || 'Failed to delete category';
            }
            this.loading = false;
          },
          error: (error: any) => {
            this.error = 'Error deleting category. Please try again.';
            this.loading = false;
            console.error('Error deleting category:', error);
          }
        });
    }
  }

  toggleCategoryStatus(category: AdminCategory): void {
    const action = category.isActive ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} the category "${category.name}"?`)) {
      this.loading = true;
      
      const updateData = { ...category, isActive: !category.isActive };
      
      this.subscriptionService.updateCategory(category.id, updateData)
        .subscribe({
          next: (response: ApiResponse<AdminCategory>) => {
            if (response.success) {
              this.successMessage = `Category ${action}d successfully`;
              this.loadCategories();
            } else {
              this.error = response.message || `Failed to ${action} category`;
            }
            this.loading = false;
          },
          error: (error: any) => {
            this.error = `Error ${action}ing category. Please try again.`;
            this.loading = false;
            console.error(`Error ${action}ing category:`, error);
          }
        });
    }
  }

  validateCategory(): boolean {
    if (!this.newCategory.name.trim()) {
      this.error = 'Category name is required';
      return false;
    }
    if (this.newCategory.basePrice < 0) {
      this.error = 'Base price cannot be negative';
      return false;
    }
    if (this.newCategory.consultationFee < 0) {
      this.error = 'Consultation fee cannot be negative';
      return false;
    }
    if (this.newCategory.oneTimeConsultationFee < 0) {
      this.error = 'One-time consultation fee cannot be negative';
      return false;
    }
    return true;
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge-success' : 'badge-secondary';
  }

  getFeatureIcon(feature: string): string {
    switch (feature) {
      case 'requiresHealthAssessment': return 'fas fa-clipboard-check';
      case 'allowsMedicationDelivery': return 'fas fa-pills';
      case 'allowsFollowUpMessaging': return 'fas fa-comments';
      default: return 'fas fa-check';
    }
  }

  getFeatureLabel(feature: string): string {
    switch (feature) {
      case 'requiresHealthAssessment': return 'Health Assessment';
      case 'allowsMedicationDelivery': return 'Medication Delivery';
      case 'allowsFollowUpMessaging': return 'Follow-up Messaging';
      default: return feature;
    }
  }

  clearError(): void {
    this.error = '';
  }

  clearSuccess(): void {
    this.successMessage = '';
  }
} 