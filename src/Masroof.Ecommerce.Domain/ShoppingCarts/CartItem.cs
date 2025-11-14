using System;
using Volo.Abp.Domain.Entities;

namespace Masroof.Ecommerce.ShoppingCarts;

public class CartItem : Entity<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }

    protected CartItem()
    {
    }

    public CartItem(
        Guid id,
        Guid productId,
        string productName,
        decimal price,
        int quantity,
        string? imageUrl = null
    ) : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
        ImageUrl = imageUrl;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        }

        Quantity = quantity;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative", nameof(price));
        }

        Price = price;
    }

    public decimal GetTotalPrice()
    {
        return Price * Quantity;
    }
}
