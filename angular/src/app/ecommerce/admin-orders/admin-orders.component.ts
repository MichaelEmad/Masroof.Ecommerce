import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../proxy/orders/order.service';
import { OrderDto, OrderStatus, UpdateOrderStatusDto, UpdateTrackingInfoDto } from '../../proxy/products/models';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.scss']
})
export class AdminOrdersComponent implements OnInit {
  orders: OrderDto[] = [];
  filteredOrders: OrderDto[] = [];
  loading = true;
  showDetailsModal = false;
  showTrackingModal = false;
  showNotesModal = false;
  selectedOrder: OrderDto | null = null;

  // Filters
  statusFilter: OrderStatus | 'all' = 'all';
  dateFromFilter = '';
  dateToFilter = '';
  searchTerm = '';

  // Forms
  trackingForm = {
    trackingNumber: '',
    carrier: ''
  };
  adminNotes = '';

  OrderStatus = OrderStatus;

  constructor(private orderService: OrderService) {}

  ngOnInit() {
    this.loadOrders();
    this.setDefaultDateFilters();
  }

  setDefaultDateFilters() {
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);

    this.dateFromFilter = thirtyDaysAgo.toISOString().split('T')[0];
    this.dateToFilter = today.toISOString().split('T')[0];
  }

  loadOrders() {
    this.loading = true;
    this.orderService.getList().subscribe({
      next: (data) => {
        this.orders = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading orders:', err);
        this.loading = false;
      }
    });
  }

  applyFilters() {
    let filtered = [...this.orders];

    // Apply status filter
    if (this.statusFilter !== 'all') {
      filtered = filtered.filter(o => o.status === this.statusFilter);
    }

    // Apply date range filter
    if (this.dateFromFilter) {
      const fromDate = new Date(this.dateFromFilter);
      filtered = filtered.filter(o => new Date(o.creationTime) >= fromDate);
    }
    if (this.dateToFilter) {
      const toDate = new Date(this.dateToFilter);
      toDate.setHours(23, 59, 59, 999);
      filtered = filtered.filter(o => new Date(o.creationTime) <= toDate);
    }

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(o =>
        o.orderNumber.toLowerCase().includes(term) ||
        o.customerEmail?.toLowerCase().includes(term) ||
        o.shippingFullName.toLowerCase().includes(term)
      );
    }

    // Sort by creation time (newest first)
    filtered.sort((a, b) => new Date(b.creationTime).getTime() - new Date(a.creationTime).getTime());

    this.filteredOrders = filtered;
  }

  onFilterChange() {
    this.applyFilters();
  }

  openDetailsModal(order: OrderDto) {
    this.selectedOrder = order;
    this.showDetailsModal = true;
  }

  closeDetailsModal() {
    this.showDetailsModal = false;
    this.selectedOrder = null;
  }

  openTrackingModal(order: OrderDto) {
    this.selectedOrder = order;
    this.trackingForm = {
      trackingNumber: order.trackingNumber || '',
      carrier: order.carrier || ''
    };
    this.showTrackingModal = true;
  }

  closeTrackingModal() {
    this.showTrackingModal = false;
    this.selectedOrder = null;
  }

  openNotesModal(order: OrderDto) {
    this.selectedOrder = order;
    this.adminNotes = order.adminNotes || '';
    this.showNotesModal = true;
  }

  closeNotesModal() {
    this.showNotesModal = false;
    this.selectedOrder = null;
  }

  updateOrderStatus(order: OrderDto, newStatus: OrderStatus) {
    if (confirm(`Are you sure you want to change the status to ${this.getStatusText(newStatus)}?`)) {
      const updateDto: UpdateOrderStatusDto = { status: newStatus };
      this.orderService.updateStatus(order.id, updateDto).subscribe({
        next: () => {
          this.loadOrders();
        },
        error: (err) => {
          console.error('Error updating order status:', err);
          alert('Error updating order status: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  saveTrackingInfo() {
    if (!this.selectedOrder) return;

    if (!this.trackingForm.trackingNumber || !this.trackingForm.carrier) {
      alert('Please fill in both tracking number and carrier');
      return;
    }

    const updateDto: UpdateTrackingInfoDto = {
      trackingNumber: this.trackingForm.trackingNumber,
      carrier: this.trackingForm.carrier
    };

    this.orderService.updateTrackingInfo(this.selectedOrder.id, updateDto).subscribe({
      next: () => {
        // Also update status to Shipped if not already
        if (this.selectedOrder!.status !== OrderStatus.Shipped &&
            this.selectedOrder!.status !== OrderStatus.Delivered) {
          const statusDto: UpdateOrderStatusDto = { status: OrderStatus.Shipped };
          this.orderService.updateStatus(this.selectedOrder!.id, statusDto).subscribe({
            next: () => {
              this.loadOrders();
              this.closeTrackingModal();
            },
            error: (err) => console.error('Error updating status:', err)
          });
        } else {
          this.loadOrders();
          this.closeTrackingModal();
        }
      },
      error: (err) => {
        console.error('Error updating tracking info:', err);
        alert('Error updating tracking info: ' + (err.error?.error?.message || 'Unknown error'));
      }
    });
  }

  saveAdminNotes() {
    if (!this.selectedOrder) return;

    this.orderService.addAdminNotes(this.selectedOrder.id, this.adminNotes).subscribe({
      next: () => {
        this.loadOrders();
        this.closeNotesModal();
      },
      error: (err) => {
        console.error('Error saving admin notes:', err);
        alert('Error saving admin notes: ' + (err.error?.error?.message || 'Unknown error'));
      }
    });
  }

  cancelOrder(order: OrderDto) {
    if (confirm(`Are you sure you want to cancel order ${order.orderNumber}?`)) {
      this.orderService.cancel(order.id).subscribe({
        next: () => {
          this.loadOrders();
        },
        error: (err) => {
          console.error('Error cancelling order:', err);
          alert('Error cancelling order: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  getStatusText(status: OrderStatus): string {
    const statusMap: { [key: number]: string } = {
      [OrderStatus.Pending]: 'Pending',
      [OrderStatus.Confirmed]: 'Confirmed',
      [OrderStatus.Processing]: 'Processing',
      [OrderStatus.Shipped]: 'Shipped',
      [OrderStatus.Delivered]: 'Delivered',
      [OrderStatus.Cancelled]: 'Cancelled'
    };
    return statusMap[status] || 'Unknown';
  }

  getStatusBadgeClass(status: OrderStatus): string {
    const classMap: { [key: number]: string } = {
      [OrderStatus.Pending]: 'bg-warning',
      [OrderStatus.Confirmed]: 'bg-info',
      [OrderStatus.Processing]: 'bg-primary',
      [OrderStatus.Shipped]: 'bg-success',
      [OrderStatus.Delivered]: 'bg-success',
      [OrderStatus.Cancelled]: 'bg-danger'
    };
    return classMap[status] || 'bg-secondary';
  }

  getNextStatuses(currentStatus: OrderStatus): OrderStatus[] {
    const statusFlow: { [key: number]: OrderStatus[] } = {
      [OrderStatus.Pending]: [OrderStatus.Confirmed, OrderStatus.Cancelled],
      [OrderStatus.Confirmed]: [OrderStatus.Processing, OrderStatus.Cancelled],
      [OrderStatus.Processing]: [OrderStatus.Shipped, OrderStatus.Cancelled],
      [OrderStatus.Shipped]: [OrderStatus.Delivered],
      [OrderStatus.Delivered]: [],
      [OrderStatus.Cancelled]: []
    };
    return statusFlow[currentStatus] || [];
  }

  canUpdateStatus(order: OrderDto): boolean {
    return this.getNextStatuses(order.status).length > 0;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
  }

  getOrderTotal(order: OrderDto): number {
    return order.totalAmount;
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
