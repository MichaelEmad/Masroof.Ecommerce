using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;
using Masroof.Ecommerce.Permissions;
using Masroof.Ecommerce.Products;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Dashboard;

[Authorize(EcommercePermissions.Dashboard.Host)]
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public DashboardAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Customer, Guid> customerRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var orders = await _orderRepository.GetListAsync();
        var products = await _productRepository.GetListAsync();
        var customers = await _customerRepository.GetListAsync();

        var stats = new DashboardStatsDto
        {
            OrderStats = GetOrderStats(orders),
            ProductStats = GetProductStats(products, orders),
            CustomerStats = GetCustomerStats(customers),
            RevenueStats = GetRevenueStats(orders)
        };

        return stats;
    }

    private OrderStatsDto GetOrderStats(System.Collections.Generic.List<Order> orders)
    {
        var totalOrders = orders.Count;
        var ordersByStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrdersByStatusDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalOrders > 0 ? (decimal)g.Count() / totalOrders * 100 : 0
            })
            .ToList();

        var recentOrders = orders
            .OrderByDescending(o => o.CreationTime)
            .Take(5)
            .Select(o => new RecentOrderDto
            {
                OrderNumber = o.OrderNumber,
                CustomerName = o.ShippingFullName,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                OrderDate = o.CreationTime
            })
            .ToList();

        return new OrderStatsDto
        {
            TotalOrders = totalOrders,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            OrdersByStatus = ordersByStatus,
            RecentOrders = recentOrders
        };
    }

    private ProductStatsDto GetProductStats(System.Collections.Generic.List<Product> products, System.Collections.Generic.List<Order> orders)
    {
        var topSellingProducts = products
            .Where(p => p.SoldCount > 0)
            .OrderByDescending(p => p.SoldCount)
            .Take(5)
            .Select(p => new TopSellingProductDto
            {
                Name = p.Name,
                SoldCount = p.SoldCount,
                Revenue = p.GetEffectivePrice() * p.SoldCount
            })
            .ToList();

        return new ProductStatsDto
        {
            TotalProducts = products.Count,
            ActiveProducts = products.Count(p => p.IsActive),
            OutOfStockProducts = products.Count(p => p.StockQuantity == 0),
            LowStockProducts = products.Count(p => p.StockQuantity > 0 && p.StockQuantity < 10),
            TopSellingProducts = topSellingProducts
        };
    }

    private CustomerStatsDto GetCustomerStats(System.Collections.Generic.List<Customer> customers)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        var topCustomers = customers
            .Where(c => c.TotalOrders > 0)
            .OrderByDescending(c => c.TotalSpent)
            .Take(5)
            .Select(c => new TopCustomerDto
            {
                Name = c.GetFullName(),
                TotalOrders = c.TotalOrders,
                TotalSpent = c.TotalSpent
            })
            .ToList();

        return new CustomerStatsDto
        {
            TotalCustomers = customers.Count,
            NewCustomersThisMonth = customers.Count(c => c.CreationTime >= firstDayOfMonth),
            VipCustomers = customers.Count(c => c.IsVipCustomer()),
            TopCustomers = topCustomers
        };
    }

    private RevenueStatsDto GetRevenueStats(System.Collections.Generic.List<Order> orders)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var revenueThisMonth = completedOrders
            .Where(o => o.CreationTime >= firstDayOfMonth)
            .Sum(o => o.TotalAmount);
        var revenueLastMonth = completedOrders
            .Where(o => o.CreationTime >= firstDayOfLastMonth && o.CreationTime < firstDayOfMonth)
            .Sum(o => o.TotalAmount);

        var growthPercentage = revenueLastMonth > 0
            ? ((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100
            : 0;

        return new RevenueStatsDto
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            RevenueLastMonth = revenueLastMonth,
            GrowthPercentage = growthPercentage
        };
    }
}
