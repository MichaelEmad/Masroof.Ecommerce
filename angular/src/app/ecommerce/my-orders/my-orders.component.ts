import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { OrderService } from '../../proxy/orders/order.service';
import { OrderDto, OrderStatus } from '../../proxy/products/models';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-orders.component.html',
  styleUrls: ['./my-orders.component.scss']
})
export class MyOrdersComponent implements OnInit {
  orders: OrderDto[] = [];
  loading = false;

  constructor(
    private orderService: OrderService,
    private toasterService: ToasterService
  ) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading = true;
    this.orderService.getMyOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toasterService.error('Failed to load orders', 'Error');
      }
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
      this.orderService.cancel(orderId).subscribe({
        next: () => {
          this.toasterService.success('Order cancelled successfully!', 'Success');
          this.loadOrders();
        },
        error: () => {
          this.toasterService.error('Failed to cancel order', 'Error');
        }
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
        this.toasterService.success('Invoice downloaded successfully', 'Success');
      },
      error: (err) => {
        const errorMessage = err.error?.error?.message || 'Failed to download invoice';
        this.toasterService.error(errorMessage, 'Error');
      }
    });
  }
}
