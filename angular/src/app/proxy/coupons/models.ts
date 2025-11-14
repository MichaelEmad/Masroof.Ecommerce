export interface CouponDto {
  id: string;
  code: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  maxUsageCount?: number;
  usageCount: number;
  validFrom: string;
  validTo: string;
  isActive: boolean;
  isOneTimeUse: boolean;
  isExpired: boolean;
  remainingUses: number;
  creationTime: string;
}

export interface CreateUpdateCouponDto {
  code: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  minimumOrderAmount?: number;
  maximumDiscountAmount?: number;
  maxUsageCount?: number;
  validFrom: string;
  validTo: string;
  isActive: boolean;
  isOneTimeUse: boolean;
}

export enum DiscountType {
  Percentage = 0,
  FixedAmount = 1,
}
