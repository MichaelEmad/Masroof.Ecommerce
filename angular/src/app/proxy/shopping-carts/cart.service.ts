import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { ShoppingCartDto, AddToCartDto } from '../products/models';
import { BehaviorSubject, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CartService {
  apiName = 'Default';
  private cartSubject = new BehaviorSubject<ShoppingCartDto | null>(null);
  cart$ = this.cartSubject.asObservable();

  constructor(private restService: RestService) {
    this.loadCart();
  }

  loadCart() {
    this.getMyCart().subscribe(cart => this.cartSubject.next(cart));
  }

  getMyCart = () =>
    this.restService.request<any, ShoppingCartDto>({
      method: 'GET',
      url: '/api/app/shopping-cart/my-cart',
    });

  addItem = (input: AddToCartDto) =>
    this.restService.request<any, ShoppingCartDto>({
      method: 'POST',
      url: '/api/app/shopping-cart/add-item',
      body: input,
    }).pipe(tap(cart => this.cartSubject.next(cart)));

  updateItemQuantity = (productId: string, quantity: number) =>
    this.restService.request<any, ShoppingCartDto>({
      method: 'PUT',
      url: `/api/app/shopping-cart/update-item-quantity/${productId}`,
      body: { quantity },
    }).pipe(tap(cart => this.cartSubject.next(cart)));

  removeItem = (productId: string) =>
    this.restService.request<any, ShoppingCartDto>({
      method: 'DELETE',
      url: `/api/app/shopping-cart/remove-item/${productId}`,
    }).pipe(tap(cart => this.cartSubject.next(cart)));

  clearCart = () =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/shopping-cart/clear-cart',
    }).pipe(tap(() => this.cartSubject.next(null)));

  applyCoupon = (couponCode: string) =>
    this.restService.request<any, ShoppingCartDto>({
      method: 'POST',
      url: '/api/app/shopping-cart/apply-coupon',
      body: { couponCode },
    }).pipe(tap(cart => this.cartSubject.next(cart)));
}
