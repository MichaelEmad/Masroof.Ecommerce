using System.Collections.Generic;

namespace Masroof.Ecommerce.Dashboard;

public class DashboardStatsDto
{
    public OrderStatsDto OrderStats { get; set; }
    public ProductStatsDto ProductStats { get; set; }
    public CustomerStatsDto CustomerStats { get; set; }
    public RevenueStatsDto RevenueStats { get; set; }
}

public class OrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public List<OrdersByStatusDto> OrdersByStatus { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class OrdersByStatusDto
{
    public string Status { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class RecentOrderDto
{
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public System.DateTime OrderDate { get; set; }
}

public class ProductStatsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
}

public class TopSellingProductDto
{
    public string Name { get; set; }
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
}

public class CustomerStatsDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int VipCustomers { get; set; }
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

public class TopCustomerDto
{
    public string Name { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class RevenueStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal GrowthPercentage { get; set; }
}
