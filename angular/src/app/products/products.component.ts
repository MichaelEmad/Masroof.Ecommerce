import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageModule } from '@abp/ng.components/page';
import { ListService } from '@abp/ng.core';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, PageModule],
  providers: [ListService],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  products = { items: [], totalCount: 0 };
  categories = [];
  loading = false;

  constructor(
    public readonly list: ListService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    // TODO: Implement product loading using generated proxy service
    // this.productService.getPublicList({...this.list.query}).subscribe(
    //   result => {
    //     this.products = result;
    //   }
    // );
  }

  addToCart(productId: string): void {
    // TODO: Implement add to cart using generated proxy service
    // this.cartService.addToCart({ productId, quantity: 1 }).subscribe(
    //   () => {
    //     // Show success message
    //   }
    // );
  }
}
