import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../proxy/orders/order.service';
import { OrderDto, OrderStatus } from '../../proxy/products/models';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">My Orders</h2>

      <div class="row">
        <div class="col-12">
          <div *ngFor="let order of orders" class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center">
              <div>
                <strong>Order #{{ order.orderNumber }}</strong>
                <small class="text-muted ms-3">
                  {{ order.creationTime | date:'medium' }}
                </small>
              </div>
              <span [class]="'badge ' + getStatusBadgeClass(order.status)">
                {{ getStatusText(order.status) }}
              </span>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-8">
                  <h6>Items:</h6>
                  <ul class="list-unstyled">
                    <li *ngFor="let item of order.items" class="mb-2">
                      <div class="d-flex">
                        <img *ngIf="item.imageUrl"
                             [src]="item.imageUrl"
                             class="me-3 rounded"
                             style="width: 50px; height: 50px; object-fit: cover;">
                        <div>
                          <strong>{{ item.productName }}</strong>
                          <br>
                          <small class="text-muted">
                            Qty: {{ item.quantity }} × \${{ item.unitPrice }} = \${{ item.totalPrice }}
                          </small>
                        </div>
                      </div>
                    </li>
                  </ul>
                  <hr>
                  <div>
                    <strong>Shipping Address:</strong><br>
                    {{ order.shippingFullName }}<br>
                    {{ order.shippingCity }}, {{ order.shippingState }}
                  </div>
                  <div *ngIf="order.trackingNumber" class="mt-2">
                    <strong>Tracking:</strong> {{ order.trackingNumber }}
                  </div>
                </div>
                <div class="col-md-4">
                  <div class="card bg-light">
                    <div class="card-body">
                      <h6>Order Summary</h6>
                      <div class="d-flex justify-content-between mb-1">
                        <span>Subtotal:</span>
                        <span>\${{ order.subTotal }}</span>
                      </div>
                      <div class="d-flex justify-content-between mb-1" *ngIf="order.discountAmount > 0">
                        <span class="text-success">Discount:</span>
                        <span class="text-success">-\${{ order.discountAmount }}</span>
                      </div>
                      <div class="d-flex justify-content-between mb-1">
                        <span>Shipping:</span>
                        <span>\${{ order.shippingCost }}</span>
                      </div>
                      <div class="d-flex justify-content-between mb-1">
                        <span>Tax:</span>
                        <span>\${{ order.tax }}</span>
                      </div>
                      <hr>
                      <div class="d-flex justify-content-between">
                        <strong>Total:</strong>
                        <strong class="text-primary">\${{ order.totalAmount }}</strong>
                      </div>
                    </div>
                  </div>
                  <button
                          class="btn btn-success btn-sm mt-2 w-100"
                          (click)="downloadInvoice(order)">
                    <i class="bi bi-file-pdf"></i> Download Invoice
                  </button>
                  <button *ngIf="canCancel(order)"
                          class="btn btn-danger btn-sm mt-2 w-100"
                          (click)="cancelOrder(order.id)">
                    Cancel Order
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div *ngIf="orders.length === 0" class="text-center py-5">
            <i class="bi bi-inbox" style="font-size: 3rem; color: #ccc;"></i>
            <h4 class="mt-3">No orders yet</h4>
            <p class="text-muted">Start shopping to see your orders here!</p>
            <a routerLink="/ecommerce/products" class="btn btn-primary">
              Browse Products
            </a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class MyOrdersComponent implements OnInit {
  orders: OrderDto[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.orderService.getMyOrders().subscribe(data => {
      this.orders = data;
    });
  }

  getStatusText(status: OrderStatus): string {
    const statusMap = {
      [OrderStatus.Pending]: 'Pending',
      [OrderStatus.Confirmed]: 'Confirmed',
      [OrderStatus.Processing]: 'Processing',
      [OrderStatus.Shipped]: 'Shipped',
      [OrderStatus.Delivered]: 'Delivered',
      [OrderStatus.Cancelled]: 'Cancelled'
    };
    return statusMap[status];
  }

  getStatusBadgeClass(status: OrderStatus): string {
    const classMap = {
      [OrderStatus.Pending]: 'bg-warning',
      [OrderStatus.Confirmed]: 'bg-info',
      [OrderStatus.Processing]: 'bg-primary',
      [OrderStatus.Shipped]: 'bg-success',
      [OrderStatus.Delivered]: 'bg-success',
      [OrderStatus.Cancelled]: 'bg-danger'
    };
    return classMap[status];
  }

  canCancel(order: OrderDto): boolean {
    return order.status !== OrderStatus.Delivered && order.status !== OrderStatus.Cancelled;
  }

  cancelOrder(orderId: string) {
    if (confirm('Are you sure you want to cancel this order?')) {
      this.orderService.cancel(orderId).subscribe(() => {
        alert('Order cancelled successfully!');
        this.loadOrders();
      });
    }
  }

  downloadInvoice(order: OrderDto) {
    this.orderService.downloadInvoice(order.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Invoice-${order.orderNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Error downloading invoice:', err);
        alert('Error downloading invoice: ' + (err.error?.error?.message || 'Unknown error'));
      }
    });
  }
}
