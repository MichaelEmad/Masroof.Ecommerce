using System;
using Volo.Abp.Domain.Entities;

namespace Masroof.Ecommerce.Orders;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    protected OrderItem()
    {
    }

    public OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        string productName,
        decimal price,
        int quantity)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    public decimal GetTotalPrice()
    {
        return Price * Quantity;
    }
}
