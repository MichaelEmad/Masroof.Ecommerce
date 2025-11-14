export interface DashboardStatsDto {
  orderStats: OrderStatsDto;
  productStats: ProductStatsDto;
  customerStats: CustomerStatsDto;
  revenueStats: RevenueStatsDto;
}

export interface OrderStatsDto {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  cancelledOrders: number;
  ordersByStatus: OrdersByStatusDto[];
  recentOrders: RecentOrderDto[];
}

export interface OrdersByStatusDto {
  status: string;
  count: number;
  percentage: number;
}

export interface RecentOrderDto {
  orderNumber: string;
  customerName: string;
  totalAmount: number;
  status: string;
  orderDate: string;
}

export interface ProductStatsDto {
  totalProducts: number;
  activeProducts: number;
  outOfStockProducts: number;
  lowStockProducts: number;
  topSellingProducts: TopSellingProductDto[];
}

export interface TopSellingProductDto {
  name: string;
  soldCount: number;
  revenue: number;
}

export interface CustomerStatsDto {
  totalCustomers: number;
  newCustomersThisMonth: number;
  vipCustomers: number;
  topCustomers: TopCustomerDto[];
}

export interface TopCustomerDto {
  name: string;
  totalOrders: number;
  totalSpent: number;
}

export interface RevenueStatsDto {
  totalRevenue: number;
  revenueThisMonth: number;
  revenueLastMonth: number;
  growthPercentage: number;
}
