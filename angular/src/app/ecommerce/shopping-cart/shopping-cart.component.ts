import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CartService } from '../../proxy/shopping-carts/cart.service';
import { ShoppingCartDto } from '../../proxy/products/models';

@Component({
  selector: 'app-shopping-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">Shopping Cart</h2>

      <div class="row" *ngIf="cart && !cart.isEmpty">
        <!-- Cart Items -->
        <div class="col-md-8">
          <div class="card">
            <div class="card-body">
              <div *ngFor="let item of cart.items" class="row mb-3 pb-3 border-bottom">
                <div class="col-md-2">
                  <img [src]="item.imageUrl || '/assets/images/no-image.png'"
                       class="img-fluid rounded"
                       [alt]="item.productName">
                </div>
                <div class="col-md-4">
                  <h5>{{ item.productName }}</h5>
                  <p class="text-muted mb-0">\${{ item.price }}</p>
                </div>
                <div class="col-md-3">
                  <div class="input-group">
                    <button class="btn btn-outline-secondary"
                            (click)="updateQuantity(item.productId, item.quantity - 1)"
                            [disabled]="item.quantity <= 1">
                      <i class="bi bi-dash"></i>
                    </button>
                    <input type="text"
                           class="form-control text-center"
                           [value]="item.quantity"
                           readonly>
                    <button class="btn btn-outline-secondary"
                            (click)="updateQuantity(item.productId, item.quantity + 1)">
                      <i class="bi bi-plus"></i>
                    </button>
                  </div>
                </div>
                <div class="col-md-2 text-end">
                  <strong>\${{ item.totalPrice }}</strong>
                </div>
                <div class="col-md-1 text-end">
                  <button class="btn btn-sm btn-danger"
                          (click)="removeItem(item.productId)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Order Summary -->
        <div class="col-md-4">
          <div class="card">
            <div class="card-header">
              <h5>Order Summary</h5>
            </div>
            <div class="card-body">
              <div class="d-flex justify-content-between mb-2">
                <span>Subtotal ({{ cart.totalItems }} items)</span>
                <strong>\${{ cart.subTotal }}</strong>
              </div>
              <div class="d-flex justify-content-between mb-2" *ngIf="cart.discountAmount > 0">
                <span class="text-success">
                  Discount
                  <span *ngIf="cart.couponCode" class="badge bg-success ms-2">
                    {{ cart.couponCode }}
                  </span>
                </span>
                <strong class="text-success">-\${{ cart.discountAmount }}</strong>
              </div>
              <hr>
              <div class="d-flex justify-content-between mb-3">
                <strong>Total</strong>
                <strong class="h4 text-primary">\${{ cart.total }}</strong>
              </div>

              <!-- Coupon Input -->
              <div class="input-group mb-3">
                <input type="text"
                       class="form-control"
                       placeholder="Coupon Code"
                       #couponInput>
                <button class="btn btn-outline-secondary"
                        (click)="applyCoupon(couponInput.value)">
                  Apply
                </button>
              </div>

              <div class="d-grid gap-2">
                <a routerLink="/ecommerce/checkout" class="btn btn-primary btn-lg">
                  <i class="bi bi-credit-card"></i> Proceed to Checkout
                </a>
                <a routerLink="/ecommerce/products" class="btn btn-outline-secondary">
                  <i class="bi bi-arrow-left"></i> Continue Shopping
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty Cart -->
      <div *ngIf="!cart || cart.isEmpty" class="text-center py-5">
        <i class="bi bi-cart-x" style="font-size: 5rem; color: #ccc;"></i>
        <h3 class="mt-3">Your cart is empty</h3>
        <p class="text-muted">Add some products to get started!</p>
        <a routerLink="/ecommerce/products" class="btn btn-primary">
          Browse Products
        </a>
      </div>
    </div>
  `
})
export class ShoppingCartComponent implements OnInit {
  cart: ShoppingCartDto | null = null;

  constructor(private cartService: CartService) {}

  ngOnInit() {
    this.loadCart();
  }

  loadCart() {
    this.cartService.cart$.subscribe(cart => {
      this.cart = cart;
    });
  }

  updateQuantity(productId: string, quantity: number) {
    if (quantity < 1) return;
    this.cartService.updateItemQuantity(productId, quantity).subscribe();
  }

  removeItem(productId: string) {
    if (confirm('Remove this item from cart?')) {
      this.cartService.removeItem(productId).subscribe();
    }
  }

  applyCoupon(code: string) {
    if (!code) return;
    this.cartService.applyCoupon(code).subscribe({
      next: () => alert('Coupon applied successfully!'),
      error: () => alert('Invalid coupon code')
    });
  }
}
