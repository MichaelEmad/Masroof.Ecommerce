using System;
using Volo.Abp.Domain.Entities;

namespace Masroof.Ecommerce.Orders;

public class OrderItem : Entity<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }

    protected OrderItem()
    {
    }

    public OrderItem(
        Guid id,
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity,
        string? imageUrl = null
    ) : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }

    public decimal GetTotalPrice()
    {
        return UnitPrice * Quantity;
    }
}
