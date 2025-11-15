import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../proxy/products/product.service';
import { CategoryService } from '../../proxy/categories/category.service';
import { CartService } from '../../proxy/shopping-carts/cart.service';
import { ProductDto, CategoryDto, ProductFilterDto } from '../../proxy/products/models';
import { RouterModule } from '@angular/router';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-product-catalog',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-catalog.component.html',
  styleUrls: ['./product-catalog.component.scss']
})
export class ProductCatalogComponent implements OnInit {
  products: ProductDto[] = [];
  categories: CategoryDto[] = [];
  totalCount = 0;

  // Filter properties
  searchTerm = '';
  selectedCategoryId: string | null = null;
  selectedPriceRange: string = '';
  selectedSortBy: string = 'newest';
  inStockOnly = false;

  // Pagination
  currentPage = 1;
  pageSize = 12;
  totalPages = 1;

  // Active filters
  activeFilters: { type: string; label: string; value: any }[] = [];

  // Search debounce
  private searchSubject = new Subject<string>();

  // Price range options
  priceRanges = [
    { label: 'All Prices', value: '', min: null, max: null },
    { label: 'Under $50', value: 'under50', min: 0, max: 50 },
    { label: '$50 - $100', value: '50-100', min: 50, max: 100 },
    { label: '$100 - $200', value: '100-200', min: 100, max: 200 },
    { label: 'Over $200', value: 'over200', min: 200, max: null }
  ];

  // Sort options
  sortOptions = [
    { label: 'Name A-Z', value: 'name_asc' },
    { label: 'Name Z-A', value: 'name_desc' },
    { label: 'Price Low-High', value: 'price_asc' },
    { label: 'Price High-Low', value: 'price_desc' },
    { label: 'Newest', value: 'newest' },
    { label: 'Most Popular', value: 'popular' }
  ];

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private cartService: CartService
  ) {}

  ngOnInit() {
    this.loadCategories();
    this.setupSearchDebounce();
    this.loadProducts();
  }

  setupSearchDebounce() {
    this.searchSubject.pipe(debounceTime(500)).subscribe(() => {
      this.currentPage = 1;
      this.loadProducts();
    });
  }

  loadCategories() {
    this.categoryService.getActiveCategories().subscribe(data => {
      this.categories = data;
    });
  }

  loadProducts() {
    const priceRange = this.priceRanges.find(pr => pr.value === this.selectedPriceRange);

    const filter: ProductFilterDto = {
      searchTerm: this.searchTerm || undefined,
      categoryId: this.selectedCategoryId || undefined,
      minPrice: priceRange?.min || undefined,
      maxPrice: priceRange?.max || undefined,
      sortBy: this.selectedSortBy,
      inStockOnly: this.inStockOnly || undefined,
      skipCount: (this.currentPage - 1) * this.pageSize,
      maxResultCount: this.pageSize
    };

    this.productService.getFilteredProducts(filter).subscribe(data => {
      this.products = data.items || [];
      this.totalCount = data.totalCount || 0;
      this.totalPages = Math.ceil(this.totalCount / this.pageSize);
      this.updateActiveFilters();
    });
  }

  onSearchChange() {
    this.searchSubject.next(this.searchTerm);
  }

  selectCategory(categoryId: string | null) {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPriceRangeChange() {
    this.currentPage = 1;
    this.loadProducts();
  }

  onSortChange() {
    this.currentPage = 1;
    this.loadProducts();
  }

  onInStockChange() {
    this.currentPage = 1;
    this.loadProducts();
  }

  updateActiveFilters() {
    this.activeFilters = [];

    if (this.searchTerm) {
      this.activeFilters.push({
        type: 'search',
        label: `Search: "${this.searchTerm}"`,
        value: this.searchTerm
      });
    }

    if (this.selectedCategoryId) {
      const category = this.categories.find(c => c.id === this.selectedCategoryId);
      if (category) {
        this.activeFilters.push({
          type: 'category',
          label: `Category: ${category.name}`,
          value: this.selectedCategoryId
        });
      }
    }

    if (this.selectedPriceRange) {
      const priceRange = this.priceRanges.find(pr => pr.value === this.selectedPriceRange);
      if (priceRange) {
        this.activeFilters.push({
          type: 'price',
          label: `Price: ${priceRange.label}`,
          value: this.selectedPriceRange
        });
      }
    }

    if (this.inStockOnly) {
      this.activeFilters.push({
        type: 'stock',
        label: 'In Stock Only',
        value: true
      });
    }
  }

  removeFilter(filter: { type: string; label: string; value: any }) {
    switch (filter.type) {
      case 'search':
        this.searchTerm = '';
        break;
      case 'category':
        this.selectedCategoryId = null;
        break;
      case 'price':
        this.selectedPriceRange = '';
        break;
      case 'stock':
        this.inStockOnly = false;
        break;
    }
    this.currentPage = 1;
    this.loadProducts();
  }

  clearAllFilters() {
    this.searchTerm = '';
    this.selectedCategoryId = null;
    this.selectedPriceRange = '';
    this.inStockOnly = false;
    this.currentPage = 1;
    this.loadProducts();
  }

  // Pagination methods
  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProducts();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
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
