import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CouponService } from '../../proxy/coupons/coupon.service';
import { CouponDto, CreateUpdateCouponDto, DiscountType } from '../../proxy/coupons/models';

@Component({
  selector: 'app-coupon-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './coupon-management.component.html',
  styleUrls: ['./coupon-management.component.scss']
})
export class CouponManagementComponent implements OnInit {
  coupons: CouponDto[] = [];
  filteredCoupons: CouponDto[] = [];
  loading = true;
  showModal = false;
  editingCoupon: CouponDto | null = null;

  couponForm: CreateUpdateCouponDto = {
    code: '',
    description: '',
    discountType: DiscountType.Percentage,
    discountValue: 0,
    minimumOrderAmount: undefined,
    maximumDiscountAmount: undefined,
    maxUsageCount: undefined,
    validFrom: '',
    validTo: '',
    isActive: true,
    isOneTimeUse: false
  };

  filterStatus: 'all' | 'active' | 'expired' | 'inactive' = 'all';
  searchTerm = '';

  DiscountType = DiscountType;

  constructor(private couponService: CouponService) {}

  ngOnInit() {
    this.loadCoupons();
    this.setDefaultDates();
  }

  setDefaultDates() {
    const now = new Date();
    const threeMonthsLater = new Date();
    threeMonthsLater.setMonth(now.getMonth() + 3);

    this.couponForm.validFrom = now.toISOString().split('T')[0];
    this.couponForm.validTo = threeMonthsLater.toISOString().split('T')[0];
  }

  loadCoupons() {
    this.loading = true;
    this.couponService.getList().subscribe({
      next: (data) => {
        this.coupons = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading coupons:', err);
        this.loading = false;
      }
    });
  }

  applyFilters() {
    let filtered = [...this.coupons];

    // Apply status filter
    if (this.filterStatus === 'active') {
      filtered = filtered.filter(c => c.isActive && !c.isExpired);
    } else if (this.filterStatus === 'expired') {
      filtered = filtered.filter(c => c.isExpired);
    } else if (this.filterStatus === 'inactive') {
      filtered = filtered.filter(c => !c.isActive);
    }

    // Apply search filter
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(c =>
        c.code.toLowerCase().includes(term) ||
        c.description?.toLowerCase().includes(term)
      );
    }

    this.filteredCoupons = filtered;
  }

  onFilterChange() {
    this.applyFilters();
  }

  openCreateModal() {
    this.editingCoupon = null;
    this.couponForm = {
      code: '',
      description: '',
      discountType: DiscountType.Percentage,
      discountValue: 0,
      minimumOrderAmount: undefined,
      maximumDiscountAmount: undefined,
      maxUsageCount: undefined,
      validFrom: '',
      validTo: '',
      isActive: true,
      isOneTimeUse: false
    };
    this.setDefaultDates();
    this.showModal = true;
  }

  openEditModal(coupon: CouponDto) {
    this.editingCoupon = coupon;
    this.couponForm = {
      code: coupon.code,
      description: coupon.description,
      discountType: coupon.discountType,
      discountValue: coupon.discountValue,
      minimumOrderAmount: coupon.minimumOrderAmount,
      maximumDiscountAmount: coupon.maximumDiscountAmount,
      maxUsageCount: coupon.maxUsageCount,
      validFrom: coupon.validFrom.split('T')[0],
      validTo: coupon.validTo.split('T')[0],
      isActive: coupon.isActive,
      isOneTimeUse: coupon.isOneTimeUse
    };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.editingCoupon = null;
  }

  saveCoupon() {
    if (!this.isFormValid()) {
      alert('Please fill in all required fields correctly');
      return;
    }

    if (this.editingCoupon) {
      this.couponService.update(this.editingCoupon.id, this.couponForm).subscribe({
        next: () => {
          this.loadCoupons();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error updating coupon:', err);
          alert('Error updating coupon: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    } else {
      this.couponService.create(this.couponForm).subscribe({
        next: () => {
          this.loadCoupons();
          this.closeModal();
        },
        error: (err) => {
          console.error('Error creating coupon:', err);
          alert('Error creating coupon: ' + (err.error?.error?.message || 'Unknown error'));
        }
      });
    }
  }

  isFormValid(): boolean {
    return !!(
      this.couponForm.code &&
      this.couponForm.discountValue > 0 &&
      this.couponForm.validFrom &&
      this.couponForm.validTo &&
      new Date(this.couponForm.validFrom) < new Date(this.couponForm.validTo)
    );
  }

  deleteCoupon(coupon: CouponDto) {
    if (confirm(`Are you sure you want to delete coupon "${coupon.code}"?`)) {
      this.couponService.delete(coupon.id).subscribe({
        next: () => {
          this.loadCoupons();
        },
        error: (err) => {
          console.error('Error deleting coupon:', err);
          alert('Error deleting coupon');
        }
      });
    }
  }

  toggleActive(coupon: CouponDto) {
    if (coupon.isActive) {
      this.couponService.deactivate(coupon.id).subscribe({
        next: () => this.loadCoupons(),
        error: (err) => console.error('Error deactivating coupon:', err)
      });
    } else {
      this.couponService.activate(coupon.id).subscribe({
        next: () => this.loadCoupons(),
        error: (err) => console.error('Error activating coupon:', err)
      });
    }
  }

  getDiscountTypeText(type: DiscountType): string {
    return type === DiscountType.Percentage ? 'Percentage' : 'Fixed Amount';
  }

  getDiscountDisplay(coupon: CouponDto): string {
    if (coupon.discountType === DiscountType.Percentage) {
      return `${coupon.discountValue}%`;
    } else {
      return `$${coupon.discountValue}`;
    }
  }

  getStatusBadgeClass(coupon: CouponDto): string {
    if (coupon.isExpired) return 'bg-secondary';
    if (!coupon.isActive) return 'bg-danger';
    return 'bg-success';
  }

  getStatusText(coupon: CouponDto): string {
    if (coupon.isExpired) return 'Expired';
    if (!coupon.isActive) return 'Inactive';
    return 'Active';
  }
}
