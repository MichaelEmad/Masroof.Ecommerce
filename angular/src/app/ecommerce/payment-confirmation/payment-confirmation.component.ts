import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../../proxy/controllers';

@Component({
  selector: 'app-payment-confirmation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment-confirmation.component.html',
  styleUrls: ['./payment-confirmation.component.scss']
})
export class PaymentConfirmationComponent implements OnInit {
  orderId: string = '';
  orderDetails: any = null;
  loading: boolean = true;
  errorMessage: string = '';
  paymentStatus: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {}

  async ngOnInit() {
    // Get order ID from route
    this.orderId = this.route.snapshot.paramMap.get('orderId') || '';

    // Check for payment_intent in URL (from Stripe redirect)
    const paymentIntentId = this.route.snapshot.queryParamMap.get('payment_intent');
    const redirectStatus = this.route.snapshot.queryParamMap.get('redirect_status');

    if (redirectStatus === 'succeeded') {
      this.paymentStatus = 'succeeded';
    } else if (redirectStatus === 'failed') {
      this.paymentStatus = 'failed';
      this.errorMessage = 'Payment failed. Please try again.';
    }

    if (!this.orderId) {
      this.errorMessage = 'Order ID not found';
      this.loading = false;
      return;
    }

    try {
      // Get order details
      this.orderDetails = await this.orderService.getMyOrder(this.orderId).toPromise();

      if (!this.orderDetails) {
        this.errorMessage = 'Order not found';
        this.loading = false;
        return;
      }

      this.loading = false;
    } catch (error: any) {
      console.error('Error loading order:', error);
      this.errorMessage = error.error?.error?.message || 'Failed to load order details';
      this.loading = false;
    }
  }

  viewOrderDetails() {
    this.router.navigate(['/ecommerce/my-orders']);
  }

  continueShopping() {
    this.router.navigate(['/ecommerce/products']);
  }

  downloadInvoice() {
    // Download invoice
    window.open(`/api/app/order/download-invoice/${this.orderId}`, '_blank');
  }

  getStatusClass(): string {
    if (this.paymentStatus === 'succeeded' || this.orderDetails?.status === 'Confirmed') {
      return 'text-success';
    } else if (this.paymentStatus === 'failed') {
      return 'text-danger';
    }
    return 'text-warning';
  }

  getStatusIcon(): string {
    if (this.paymentStatus === 'succeeded' || this.orderDetails?.status === 'Confirmed') {
      return 'fa-check-circle';
    } else if (this.paymentStatus === 'failed') {
      return 'fa-times-circle';
    }
    return 'fa-clock';
  }

  getStatusText(): string {
    if (this.paymentStatus === 'succeeded' || this.orderDetails?.status === 'Confirmed') {
      return 'Payment Successful';
    } else if (this.paymentStatus === 'failed') {
      return 'Payment Failed';
    }
    return 'Payment Processing';
  }
}
