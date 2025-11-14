using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Carts;

public class CartItem : CreationAuditedAggregateRoot<Guid>
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public string? ProductImageUrl { get; set; }

    protected CartItem()
    {
    }

    public CartItem(
        Guid id,
        Guid customerId,
        Guid productId,
        string productName,
        decimal productPrice,
        int quantity,
        string? productImageUrl = null)
        : base(id)
    {
        CustomerId = customerId;
        ProductId = productId;
        ProductName = productName;
        ProductPrice = productPrice;
        Quantity = quantity;
        ProductImageUrl = productImageUrl;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }
        Quantity = quantity;
    }

    public decimal GetTotalPrice()
    {
        return ProductPrice * Quantity;
    }
}
