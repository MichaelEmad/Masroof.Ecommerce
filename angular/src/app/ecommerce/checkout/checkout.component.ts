import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../proxy/shopping-carts/cart.service';
import { OrderService } from '../../proxy/orders/order.service';
import { AddressService } from '../../proxy/addresses/address.service';
import { ShoppingCartDto } from '../../proxy/products/models';
import { AddressDto, AddressType } from '../../proxy/addresses/models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {
  cart: ShoppingCartDto | null = null;
  addresses: AddressDto[] = [];
  shippingAddresses: AddressDto[] = [];
  billingAddresses: AddressDto[] = [];

  selectedShippingAddressId: string = '';
  selectedBillingAddressId: string = '';
  customerNotes: string = '';

  shippingCost: number = 15.00; // Fixed shipping cost
  taxRate: number = 0.08; // 8% tax

  loading: boolean = true;
  processingOrder: boolean = false;
  errorMessage: string = '';

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private addressService: AddressService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;
    this.errorMessage = '';

    // Load cart
    this.cartService.cart$.subscribe({
      next: (cart) => {
        this.cart = cart;
        if (!cart || cart.isEmpty) {
          this.router.navigate(['/ecommerce/cart']);
        }
      },
      error: (err) => {
        console.error('Error loading cart:', err);
        this.errorMessage = 'Failed to load cart. Please try again.';
        this.loading = false;
      }
    });

    // Load addresses
    this.addressService.getMyAddresses().subscribe({
      next: (addresses) => {
        this.addresses = addresses;
        this.filterAddresses();
        this.selectDefaultAddresses();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading addresses:', err);
        this.errorMessage = 'Failed to load addresses. Please add an address first.';
        this.loading = false;
      }
    });
  }

  filterAddresses() {
    this.shippingAddresses = this.addresses.filter(
      addr => addr.addressType === AddressType.Shipping || addr.addressType === AddressType.Both
    );
    this.billingAddresses = this.addresses.filter(
      addr => addr.addressType === AddressType.Billing || addr.addressType === AddressType.Both
    );
  }

  selectDefaultAddresses() {
    const defaultShipping = this.shippingAddresses.find(addr => addr.isDefault);
    if (defaultShipping) {
      this.selectedShippingAddressId = defaultShipping.id;
    } else if (this.shippingAddresses.length > 0) {
      this.selectedShippingAddressId = this.shippingAddresses[0].id;
    }

    const defaultBilling = this.billingAddresses.find(addr => addr.isDefault);
    if (defaultBilling) {
      this.selectedBillingAddressId = defaultBilling.id;
    } else if (this.billingAddresses.length > 0) {
      this.selectedBillingAddressId = this.billingAddresses[0].id;
    }
  }

  get tax(): number {
    if (!this.cart) return 0;
    return (this.cart.total + this.shippingCost) * this.taxRate;
  }

  get totalAmount(): number {
    if (!this.cart) return 0;
    return this.cart.total + this.shippingCost + this.tax;
  }

  useSameAddress(event: any) {
    if (event.target.checked) {
      this.selectedBillingAddressId = this.selectedShippingAddressId;
    }
  }

  placeOrder() {
    if (!this.selectedShippingAddressId || !this.selectedBillingAddressId) {
      this.errorMessage = 'Please select both shipping and billing addresses.';
      return;
    }

    if (!this.cart || this.cart.isEmpty) {
      this.errorMessage = 'Your cart is empty.';
      return;
    }

    this.processingOrder = true;
    this.errorMessage = '';

    const orderData = {
      shippingAddressId: this.selectedShippingAddressId,
      billingAddressId: this.selectedBillingAddressId,
      customerNotes: this.customerNotes || undefined
    };

    this.orderService.create(orderData).subscribe({
      next: (order) => {
        this.processingOrder = false;
        // Clear the cart
        this.cartService.loadCart();
        // Redirect to order confirmation
        this.router.navigate(['/ecommerce/my-orders']);
        alert('Order placed successfully! Order #' + order.orderNumber);
      },
      error: (err) => {
        console.error('Error placing order:', err);
        this.errorMessage = err.error?.error?.message || 'Failed to place order. Please try again.';
        this.processingOrder = false;
      }
    });
  }

  getAddressById(addressId: string): AddressDto | undefined {
    return this.addresses.find(addr => addr.id === addressId);
  }
}
