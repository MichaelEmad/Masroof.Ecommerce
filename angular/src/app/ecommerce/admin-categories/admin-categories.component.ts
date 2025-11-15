import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../proxy/categories/category.service';
import { CategoryDto, CreateUpdateCategoryDto } from '../../proxy/products/models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-categories.component.html',
  styleUrls: ['./admin-categories.component.scss']
})
export class AdminCategoriesComponent implements OnInit {
  categories: CategoryDto[] = [];
  filteredCategories: CategoryDto[] = [];
  loading = true;
  showModal = false;
  editingCategory: CategoryDto | null = null;

  categoryForm: CreateUpdateCategoryDto = this.getEmptyForm();

  searchTerm = '';

  constructor(private categoryService: CategoryService) {}

  ngOnInit() {
    this.loadCategories();
  }

  getEmptyForm(): CreateUpdateCategoryDto {
    return {
      name: '',
      description: '',
      imageUrl: '',
      isActive: true,
      displayOrder: 0,
      parentCategoryId: undefined
    };
  }

  loadCategories() {
    this.loading = true;
    this.categoryService.getList().subscribe({
      next: (data) => {
        this.categories = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading categories:', err);
        this.loading = false;
      }
    });
  }

  applyFilters() {
    let filtered = [...this.categories];

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(c =>
        c.name.toLowerCase().includes(term) ||
        c.description?.toLowerCase().includes(term)
      );
    }

    // Sort by display order
    filtered.sort((a, b) => a.displayOrder - b.displayOrder);

    this.filteredCategories = filtered;
  }

  onFilterChange() {
    this.applyFilters();
  }

  openCreateModal() {
    this.editingCategory = null;
    this.categoryForm = this.getEmptyForm();
    this.showModal = true;
  }

  openEditModal(category: CategoryDto) {
    this.editingCategory = category;
    this.categoryForm = {
      name: category.name,
      description: category.description,
      imageUrl: category.imageUrl,
      isActive: category.isActive,
      displayOrder: category.displayOrder,
      parentCategoryId: category.parentCategoryId
    };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.editingCategory = null;
  }

  saveCategory() {
    if (!this.isFormValid()) {
      alert('Please fill in all required fields correctly');
      return;
    }

    if (this.editingCategory) {
      this.categoryService.update(this.editingCategory.id, this.categoryForm).subscribe({
        next: () => {
          this.loadCategories();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error updating category:', err);
          alert('Error updating category: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    } else {
      this.categoryService.create(this.categoryForm).subscribe({
        next: () => {
          this.loadCategories();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error creating category:', err);
          alert('Error creating category: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  isFormValid(): boolean {
    return !!(
      this.categoryForm.name &&
      this.categoryForm.description
    );
  }

  deleteCategory(category: CategoryDto) {
    if (confirm(`Are you sure you want to delete category "${category.name}"?`)) {
      this.categoryService.delete(category.id).subscribe({
        next: () => {
          this.loadCategories();
        },
        error: (err) => {
          console.error('Error deleting category:', err);
          alert('Error deleting category: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  toggleActive(category: CategoryDto) {
    const updatedCategory: CreateUpdateCategoryDto = {
      name: category.name,
      description: category.description,
      imageUrl: category.imageUrl,
      isActive: !category.isActive,
      displayOrder: category.displayOrder,
      parentCategoryId: category.parentCategoryId
    };

    this.categoryService.update(category.id, updatedCategory).subscribe({
      next: () => this.loadCategories(),
      error: (err) => {
        console.error('Error toggling active status:', err);
        alert('Error toggling active status');
      }
    });
  }

  getParentCategoryName(parentId?: string): string {
    if (!parentId) return 'Root Category';
    const parent = this.categories.find(c => c.id === parentId);
    return parent?.name || 'Unknown';
  }

  getAvailableParentCategories(): CategoryDto[] {
    if (this.editingCategory) {
      // Exclude the current category from the parent list to prevent circular reference
      return this.categories.filter(c => c.id !== this.editingCategory!.id);
    }
    return this.categories;
  }

  getStatusBadgeClass(category: CategoryDto): string {
    return category.isActive ? 'bg-success' : 'bg-danger';
  }

  getCategoryTypeText(category: CategoryDto): string {
    return category.isRootCategory ? 'Root' : 'Sub-category';
  }
}
