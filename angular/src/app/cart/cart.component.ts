import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageModule } from '@abp/ng.components/page';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, PageModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit {
  cartItems = [];
  cartTotal = 0;
  loading = false;

  constructor(
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    // TODO: Implement cart loading using generated proxy service
    // this.cartService.getMyCart().subscribe(
    //   result => {
    //     this.cartItems = result;
    //     this.calculateTotal();
    //   }
    // );
  }

  updateQuantity(itemId: string, quantity: number): void {
    // TODO: Implement quantity update using generated proxy service
    // this.cartService.updateCartItem(itemId, { quantity }).subscribe(
    //   () => {
    //     this.loadCart();
    //   }
    // );
  }

  removeItem(itemId: string): void {
    // TODO: Implement item removal using generated proxy service
    // this.cartService.removeFromCart(itemId).subscribe(
    //   () => {
    //     this.loadCart();
    //   }
    // );
  }

  calculateTotal(): void {
    this.cartTotal = this.cartItems.reduce((sum, item) => sum + item.totalPrice, 0);
  }

  checkout(): void {
    this.router.navigate(['/checkout']);
  }
}
