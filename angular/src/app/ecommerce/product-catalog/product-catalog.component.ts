import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../proxy/products/product.service';
import { CategoryService } from '../../proxy/categories/category.service';
import { CartService } from '../../proxy/shopping-carts/cart.service';
import { ProductDto, CategoryDto } from '../../proxy/products/models';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-product-catalog',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mt-4">
      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3">
          <div class="card">
            <div class="card-header">
              <h5>Categories</h5>
            </div>
            <div class="list-group list-group-flush">
              <button
                class="list-group-item list-group-item-action"
                [class.active]="!selectedCategoryId"
                (click)="selectCategory(null)">
                All Products
              </button>
              <button
                *ngFor="let category of categories"
                class="list-group-item list-group-item-action"
                [class.active]="selectedCategoryId === category.id"
                (click)="selectCategory(category.id)">
                {{ category.name }}
              </button>
            </div>
          </div>
        </div>

        <!-- Products Grid -->
        <div class="col-md-9">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h2>{{ selectedCategoryId ? 'Category Products' : 'All Products' }}</h2>
            <div class="btn-group">
              <button class="btn btn-outline-primary active">
                <i class="bi bi-grid"></i> Grid
              </button>
              <button class="btn btn-outline-primary">
                <i class="bi bi-list"></i> List
              </button>
            </div>
          </div>

          <div class="row row-cols-1 row-cols-md-3 g-4">
            <div class="col" *ngFor="let product of products">
              <div class="card h-100 shadow-sm">
                <div class="position-relative">
                  <img
                    [src]="product.imageUrl || '/assets/images/no-image.png'"
                    class="card-img-top"
                    [alt]="product.name"
                    style="height: 200px; object-fit: cover;">
                  <span *ngIf="product.discountPrice" class="badge bg-danger position-absolute top-0 end-0 m-2">
                    {{ getDiscountPercentage(product) }}% OFF
                  </span>
                  <span *ngIf="!product.inStock" class="badge bg-secondary position-absolute top-0 start-0 m-2">
                    Out of Stock
                  </span>
                  <span *ngIf="product.isFeatured" class="badge bg-warning position-absolute top-0 start-0 m-2">
                    <i class="bi bi-star-fill"></i> Featured
                  </span>
                </div>
                <div class="card-body">
                  <h5 class="card-title">{{ product.name }}</h5>
                  <p class="card-text text-muted small">{{ product.shortDescription }}</p>
                  <div class="d-flex justify-content-between align-items-center">
                    <div>
                      <span *ngIf="product.discountPrice" class="text-muted text-decoration-line-through me-2">
                        \${{ product.price }}
                      </span>
                      <span class="h5 text-primary mb-0">\${{ product.effectivePrice }}</span>
                    </div>
                    <small class="text-muted">
                      <i class="bi bi-eye"></i> {{ product.viewCount }}
                    </small>
                  </div>
                </div>
                <div class="card-footer bg-white">
                  <div class="d-grid gap-2">
                    <button
                      class="btn btn-primary"
                      [disabled]="!product.inStock"
                      (click)="addToCart(product)">
                      <i class="bi bi-cart-plus"></i>
                      {{ product.inStock ? 'Add to Cart' : 'Out of Stock' }}
                    </button>
                    <a [routerLink]="['/ecommerce/products', product.id]" class="btn btn-outline-secondary btn-sm">
                      View Details
                    </a>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div *ngIf="products.length === 0" class="text-center py-5">
            <i class="bi bi-inbox" style="font-size: 3rem;"></i>
            <p class="text-muted mt-3">No products found</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ProductCatalogComponent implements OnInit {
  products: ProductDto[] = [];
  categories: CategoryDto[] = [];
  selectedCategoryId: string | null = null;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private cartService: CartService
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories() {
    this.categoryService.getActiveCategories().subscribe(data => {
      this.categories = data;
    });
  }

  loadProducts() {
    if (this.selectedCategoryId) {
      this.productService.getProductsByCategory(this.selectedCategoryId).subscribe(data => {
        this.products = data;
      });
    } else {
      this.productService.getPublicProducts().subscribe(data => {
        this.products = data.items || [];
      });
    }
  }

  selectCategory(categoryId: string | null) {
    this.selectedCategoryId = categoryId;
    this.loadProducts();
  }

  addToCart(product: ProductDto) {
    this.cartService.addItem({ productId: product.id, quantity: 1 }).subscribe(() => {
      alert(`${product.name} added to cart!`);
    });
  }

  getDiscountPercentage(product: ProductDto): number {
    if (!product.discountPrice) return 0;
    return Math.round((1 - product.discountPrice / product.price) * 100);
  }
}
