import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../proxy/products/product.service';
import { ProductImageService } from '../../proxy/products/product-image.service';
import { CategoryService } from '../../proxy/categories/category.service';
import { ProductDto, CreateUpdateProductDto, CategoryDto, ProductImageDto, UploadProductImageDto } from '../../proxy/products/models';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-products.component.html',
  styleUrls: ['./admin-products.component.scss']
})
export class AdminProductsComponent implements OnInit {
  products: ProductDto[] = [];
  filteredProducts: ProductDto[] = [];
  categories: CategoryDto[] = [];
  loading = true;
  showModal = false;
  editingProduct: ProductDto | null = null;

  productForm: CreateUpdateProductDto = this.getEmptyForm();

  // Image management
  productImages: ProductImageDto[] = [];
  uploadingImages = false;
  imagePreview: string | null = null;

  // Filters
  searchTerm = '';
  selectedCategoryFilter = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;

  constructor(
    private productService: ProductService,
    private productImageService: ProductImageService,
    private categoryService: CategoryService
  ) {}

  ngOnInit() {
    this.loadProducts();
    this.loadCategories();
  }

  getEmptyForm(): CreateUpdateProductDto {
    return {
      name: '',
      description: '',
      shortDescription: '',
      price: 0,
      discountPrice: undefined,
      sku: '',
      stockQuantity: 0,
      imageUrl: '',
      isActive: true,
      isFeatured: false,
      categoryId: '',
      weight: 0,
      brand: ''
    };
  }

  loadProducts() {
    this.loading = true;
    this.productService.getList().subscribe({
      next: (data) => {
        this.products = data.items || [];
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.loading = false;
      }
    });
  }

  loadCategories() {
    this.categoryService.getList().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Error loading categories:', err);
      }
    });
  }

  applyFilters() {
    let filtered = [...this.products];

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(term) ||
        p.sku?.toLowerCase().includes(term) ||
        p.description.toLowerCase().includes(term)
      );
    }

    // Apply category filter
    if (this.selectedCategoryFilter) {
      filtered = filtered.filter(p => p.categoryId === this.selectedCategoryFilter);
    }

    this.filteredProducts = filtered;
    this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize);
    this.currentPage = 1;
  }

  onFilterChange() {
    this.applyFilters();
  }

  get paginatedProducts(): ProductDto[] {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    return this.filteredProducts.slice(startIndex, endIndex);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  openCreateModal() {
    this.editingProduct = null;
    this.productForm = this.getEmptyForm();
    this.productImages = [];
    this.imagePreview = null;
    this.showModal = true;
  }

  openEditModal(product: ProductDto) {
    this.editingProduct = product;
    this.productForm = {
      name: product.name,
      description: product.description,
      shortDescription: product.shortDescription,
      price: product.price,
      discountPrice: product.discountPrice,
      sku: product.sku,
      stockQuantity: product.stockQuantity,
      imageUrl: product.imageUrl,
      isActive: product.isActive,
      isFeatured: product.isFeatured,
      categoryId: product.categoryId,
      weight: product.weight,
      brand: product.brand
    };
    this.imagePreview = null;
    this.loadProductImages(product.id);
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.editingProduct = null;
    this.productImages = [];
    this.imagePreview = null;
  }

  saveProduct() {
    if (!this.isFormValid()) {
      alert('Please fill in all required fields correctly');
      return;
    }

    if (this.editingProduct) {
      this.productService.update(this.editingProduct.id, this.productForm).subscribe({
        next: () => {
          this.loadProducts();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error updating product:', err);
          alert('Error updating product: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    } else {
      this.productService.create(this.productForm).subscribe({
        next: () => {
          this.loadProducts();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error creating product:', err);
          alert('Error creating product: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  isFormValid(): boolean {
    return !!(
      this.productForm.name &&
      this.productForm.description &&
      this.productForm.price > 0 &&
      this.productForm.stockQuantity >= 0
    );
  }

  deleteProduct(product: ProductDto) {
    if (confirm(`Are you sure you want to delete product "${product.name}"?`)) {
      this.productService.delete(product.id).subscribe({
        next: () => {
          this.loadProducts();
        },
        error: (err) => {
          console.error('Error deleting product:', err);
          alert('Error deleting product: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  toggleActive(product: ProductDto) {
    const updatedProduct: CreateUpdateProductDto = {
      name: product.name,
      description: product.description,
      shortDescription: product.shortDescription,
      price: product.price,
      discountPrice: product.discountPrice,
      sku: product.sku,
      stockQuantity: product.stockQuantity,
      imageUrl: product.imageUrl,
      isActive: !product.isActive,
      isFeatured: product.isFeatured,
      categoryId: product.categoryId,
      weight: product.weight,
      brand: product.brand
    };

    this.productService.update(product.id, updatedProduct).subscribe({
      next: () => this.loadProducts(),
      error: (err) => {
        console.error('Error toggling active status:', err);
        alert('Error toggling active status');
      }
    });
  }

  toggleFeatured(product: ProductDto) {
    const updatedProduct: CreateUpdateProductDto = {
      name: product.name,
      description: product.description,
      shortDescription: product.shortDescription,
      price: product.price,
      discountPrice: product.discountPrice,
      sku: product.sku,
      stockQuantity: product.stockQuantity,
      imageUrl: product.imageUrl,
      isActive: product.isActive,
      isFeatured: !product.isFeatured,
      categoryId: product.categoryId,
      weight: product.weight,
      brand: product.brand
    };

    this.productService.update(product.id, updatedProduct).subscribe({
      next: () => this.loadProducts(),
      error: (err) => {
        console.error('Error toggling featured status:', err);
        alert('Error toggling featured status');
      }
    });
  }

  onImageChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      // For now, just store the filename. In production, you would upload to a server
      this.productForm.imageUrl = '/assets/images/products/' + file.name;
    }
  }

  getCategoryName(categoryId?: string): string {
    if (!categoryId) return 'No Category';
    const category = this.categories.find(c => c.id === categoryId);
    return category?.name || 'Unknown';
  }

  getStatusBadgeClass(product: ProductDto): string {
    return product.isActive ? 'bg-success' : 'bg-danger';
  }

  getStockBadgeClass(product: ProductDto): string {
    if (product.stockQuantity === 0) return 'bg-danger';
    if (product.stockQuantity < 10) return 'bg-warning';
    return 'bg-success';
  }

  // Image Management Methods
  loadProductImages(productId: string) {
    this.productImageService.getProductImages(productId).subscribe({
      next: (images) => {
        this.productImages = images;
      },
      error: (err) => {
        console.error('Error loading product images:', err);
      }
    });
  }

  onImageFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    if (!this.editingProduct) {
      alert('Please save the product first before uploading images');
      return;
    }

    const files = Array.from(input.files);

    // Validate file types and sizes
    const validFiles = files.filter(file => {
      const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif'];
      const maxSize = 5 * 1024 * 1024; // 5MB

      if (!validTypes.includes(file.type)) {
        alert(`File ${file.name} has invalid type. Allowed: JPEG, PNG, WEBP, GIF`);
        return false;
      }

      if (file.size > maxSize) {
        alert(`File ${file.name} exceeds maximum size of 5MB`);
        return false;
      }

      return true;
    });

    if (validFiles.length === 0) {
      return;
    }

    this.uploadingImages = true;

    // Upload each file
    validFiles.forEach((file, index) => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64String = (reader.result as string).split(',')[1]; // Remove data:image/xxx;base64, prefix

        const uploadDto: UploadProductImageDto = {
          productId: this.editingProduct!.id,
          fileName: file.name,
          contentType: file.type,
          content: base64String
        };

        this.productImageService.uploadImage(uploadDto).subscribe({
          next: (image) => {
            this.productImages.push(image);
            this.productImages.sort((a, b) => a.displayOrder - b.displayOrder);

            // If this is the last file, reset the loading state
            if (index === validFiles.length - 1) {
              this.uploadingImages = false;
              input.value = ''; // Reset file input
            }
          },
          error: (err) => {
            console.error('Error uploading image:', err);
            alert('Error uploading image: ' + (err.error?.error?.message || 'Unknown error'));
            this.uploadingImages = false;
          }
        });
      };

      reader.readAsDataURL(file);
    });
  }

  deleteImage(image: ProductImageDto) {
    if (!confirm(`Are you sure you want to delete "${image.fileName}"?`)) {
      return;
    }

    this.productImageService.deleteImage(image.id).subscribe({
      next: () => {
        this.productImages = this.productImages.filter(i => i.id !== image.id);
      },
      error: (err) => {
        console.error('Error deleting image:', err);
        alert('Error deleting image: ' + (err.error?.error?.message || 'Unknown error'));
      }
    });
  }

  setPrimaryImage(image: ProductImageDto) {
    this.productImageService.setPrimaryImage(image.id).subscribe({
      next: () => {
        // Update local state
        this.productImages.forEach(i => i.isPrimary = false);
        image.isPrimary = true;
      },
      error: (err) => {
        console.error('Error setting primary image:', err);
        alert('Error setting primary image: ' + (err.error?.error?.message || 'Unknown error'));
      }
    });
  }

  getImageUrl(image: ProductImageDto): string {
    // For now, we'll construct the URL. In production, you might use a CDN or blob storage URL
    return `/api/app/product-image/blob/${image.blobName}`;
  }

  get sortedImages(): ProductImageDto[] {
    return [...this.productImages].sort((a, b) => {
      // Primary image first
      if (a.isPrimary && !b.isPrimary) return -1;
      if (!a.isPrimary && b.isPrimary) return 1;
      // Then by display order
      return a.displayOrder - b.displayOrder;
    });
  }
}
