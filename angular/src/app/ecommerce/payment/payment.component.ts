import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { loadStripe, Stripe, StripeElements, StripePaymentElement } from '@stripe/stripe-js';
import { environment } from '../../../environments/environment';
import { OrderService, PaymentService } from '../../proxy/controllers';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit, OnDestroy {
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  paymentElement: StripePaymentElement | null = null;

  orderId: string = '';
  clientSecret: string = '';
  paymentIntentId: string = '';

  loading: boolean = true;
  processing: boolean = false;
  errorMessage: string = '';

  orderDetails: any = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService
  ) {}

  async ngOnInit() {
    // Get order ID from route
    this.orderId = this.route.snapshot.paramMap.get('orderId') || '';

    if (!this.orderId) {
      this.errorMessage = 'Order ID not found';
      this.loading = false;
      return;
    }

    try {
      // Get order details
      this.orderDetails = await this.orderService.get(this.orderId).toPromise();

      if (!this.orderDetails) {
        this.errorMessage = 'Order not found';
        this.loading = false;
        return;
      }

      // Check if order already has payment client secret
      if (this.orderDetails.paymentClientSecret) {
        this.clientSecret = this.orderDetails.paymentClientSecret;
        this.paymentIntentId = this.orderDetails.paymentIntentId;
      } else {
        // Create payment intent if not exists
        const paymentIntent = await this.paymentService.createPaymentIntent({
          orderId: this.orderId
        }).toPromise();

        if (!paymentIntent || !paymentIntent.isSuccessful) {
          this.errorMessage = paymentIntent?.errorMessage || 'Failed to create payment';
          this.loading = false;
          return;
        }

        this.clientSecret = paymentIntent.clientSecret;
        this.paymentIntentId = paymentIntent.paymentIntentId;
      }

      // Initialize Stripe
      await this.initializeStripe();

    } catch (error: any) {
      console.error('Error loading payment:', error);
      this.errorMessage = error.error?.error?.message || 'Failed to load payment';
      this.loading = false;
    }
  }

  async initializeStripe() {
    try {
      // Load Stripe
      this.stripe = await loadStripe(environment.stripe.publishableKey);

      if (!this.stripe) {
        throw new Error('Failed to load Stripe');
      }

      // Create Elements instance
      this.elements = this.stripe.elements({
        clientSecret: this.clientSecret,
        appearance: {
          theme: 'stripe',
          variables: {
            colorPrimary: '#0570de',
          }
        }
      });

      // Create and mount Payment Element
      this.paymentElement = this.elements.create('payment');
      this.paymentElement.mount('#payment-element');

      this.loading = false;
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.errorMessage = 'Failed to initialize payment form';
      this.loading = false;
    }
  }

  async handleSubmit(event: Event) {
    event.preventDefault();

    if (!this.stripe || !this.elements) {
      return;
    }

    this.processing = true;
    this.errorMessage = '';

    try {
      // Confirm payment
      const { error, paymentIntent } = await this.stripe.confirmPayment({
        elements: this.elements,
        confirmParams: {
          return_url: `${window.location.origin}/ecommerce/payment-confirmation/${this.orderId}`,
        },
        redirect: 'if_required'
      });

      if (error) {
        // Show error to customer
        this.errorMessage = error.message || 'Payment failed';
        this.processing = false;
      } else if (paymentIntent && paymentIntent.status === 'succeeded') {
        // Payment succeeded, redirect to confirmation page
        this.router.navigate(['/ecommerce/payment-confirmation', this.orderId]);
      } else {
        this.errorMessage = 'Payment processing...';
        // Payment might require additional action
        // The return_url will handle this case
      }
    } catch (error: any) {
      console.error('Payment error:', error);
      this.errorMessage = error.message || 'An unexpected error occurred';
      this.processing = false;
    }
  }

  ngOnDestroy() {
    // Cleanup
    if (this.paymentElement) {
      this.paymentElement.destroy();
    }
  }

  cancelPayment() {
    this.router.navigate(['/ecommerce/my-orders']);
  }
}
