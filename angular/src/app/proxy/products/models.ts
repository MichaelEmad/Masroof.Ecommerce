export interface ProductDto {
  id: string;
  name: string;
  description: string;
  shortDescription?: string;
  price: number;
  discountPrice?: number;
  sku?: string;
  stockQuantity: number;
  imageUrl?: string;
  isActive: boolean;
  isFeatured: boolean;
  categoryId?: string;
  categoryName?: string;
  weight: number;
  brand?: string;
  viewCount: number;
  soldCount: number;
  effectivePrice: number;
  inStock: boolean;
}

export interface CreateUpdateProductDto {
  name: string;
  description: string;
  shortDescription?: string;
  price: number;
  discountPrice?: number;
  sku?: string;
  stockQuantity: number;
  imageUrl?: string;
  isActive: boolean;
  isFeatured: boolean;
  categoryId?: string;
  weight: number;
  brand?: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string;
  imageUrl?: string;
  isActive: boolean;
  displayOrder: number;
  parentCategoryId?: string;
  slug?: string;
  isRootCategory: boolean;
}

export interface ShoppingCartDto {
  id: string;
  customerId: string;
  items: CartItemDto[];
  couponCode?: string;
  discountAmount: number;
  subTotal: number;
  total: number;
  totalItems: number;
  isEmpty: boolean;
}

export interface CartItemDto {
  id: string;
  productId: string;
  productName: string;
  price: number;
  quantity: number;
  imageUrl?: string;
  totalPrice: number;
}

export interface AddToCartDto {
  productId: string;
  quantity: number;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  customerId: string;
  items: OrderItemDto[];
  status: OrderStatus;
  subTotal: number;
  discountAmount: number;
  shippingCost: number;
  tax: number;
  totalAmount: number;
  creationTime: string;
  shippingFullName: string;
  shippingCity: string;
  shippingState: string;
  trackingNumber?: string;
}

export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  imageUrl?: string;
  totalPrice: number;
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5
}

export interface CreateOrderDto {
  shippingAddressId: string;
  billingAddressId: string;
  customerNotes?: string;
}
