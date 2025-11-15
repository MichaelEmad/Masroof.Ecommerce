import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { CartService } from '../../proxy/shopping-carts/cart.service';
import { ShoppingCartDto } from '../../proxy/products/models';

@Component({
  selector: 'app-shopping-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './shopping-cart.component.html',
  styleUrls: ['./shopping-cart.component.scss']
})
export class ShoppingCartComponent implements OnInit {
  cart: ShoppingCartDto | null = null;
  loading = false;

  constructor(
    private cartService: CartService,
    private toasterService: ToasterService
  ) {}

  ngOnInit() {
    this.loadCart();
  }

  loadCart() {
    this.loading = true;
    this.cartService.cart$.subscribe({
      next: (cart) => {
        this.cart = cart;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toasterService.error('Failed to load cart', 'Error');
      }
    });
  }

  updateQuantity(productId: string, quantity: number) {
    if (quantity < 1) return;

    this.cartService.updateItemQuantity(productId, quantity).subscribe({
      next: () => {
        this.toasterService.success('Cart updated', 'Success');
      },
      error: () => {
        this.toasterService.error('Failed to update quantity', 'Error');
      }
    });
  }

  removeItem(productId: string) {
    if (confirm('Are you sure you want to remove this item from your cart?')) {
      this.cartService.removeItem(productId).subscribe({
        next: () => {
          this.toasterService.success('Item removed from cart', 'Success');
        },
        error: () => {
          this.toasterService.error('Failed to remove item', 'Error');
        }
      });
    }
  }

  applyCoupon(code: string) {
    if (!code || !code.trim()) {
      this.toasterService.warn('Please enter a coupon code', 'Warning');
      return;
    }

    this.cartService.applyCoupon(code.trim()).subscribe({
      next: () => {
        this.toasterService.success('Coupon applied successfully!', 'Success');
      },
      error: (error) => {
        const message = error?.error?.error?.message || 'Invalid coupon code';
        this.toasterService.error(message, 'Error');
      }
    });
  }

  removeCoupon() {
    this.cartService.removeCoupon().subscribe({
      next: () => {
        this.toasterService.info('Coupon removed', 'Info');
      },
      error: () => {
        this.toasterService.error('Failed to remove coupon', 'Error');
      }
    });
  }

  clearCart() {
    if (confirm('Are you sure you want to clear your entire cart?')) {
      this.cartService.clearCart().subscribe({
        next: () => {
          this.toasterService.success('Cart cleared', 'Success');
        },
        error: () => {
          this.toasterService.error('Failed to clear cart', 'Error');
        }
      });
    }
  }
}
