using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.ShoppingCarts;

public class ShoppingCart : FullAuditedAggregateRoot<Guid>
{
    public Guid CustomerId { get; set; }
    public List<CartItem> Items { get; set; }
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }

    protected ShoppingCart()
    {
        Items = new List<CartItem>();
    }

    public ShoppingCart(Guid id, Guid customerId) : base(id)
    {
        CustomerId = customerId;
        Items = new List<CartItem>();
        ExpiresAt = DateTime.UtcNow.AddDays(30);
    }

    public void AddItem(Guid productId, string productName, decimal price, int quantity, string? imageUrl = null)
    {
        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(new CartItem(Guid.NewGuid(), productId, productName, price, quantity, imageUrl));
        }

        ExtendExpiration();
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.UpdateQuantity(quantity);
            }
        }

        ExtendExpiration();
    }

    public void Clear()
    {
        Items.Clear();
        CouponCode = null;
        DiscountAmount = 0;
    }

    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
    }

    public void RemoveCoupon()
    {
        CouponCode = null;
        DiscountAmount = 0;
    }

    public decimal GetSubTotal()
    {
        return Items.Sum(x => x.GetTotalPrice());
    }

    public decimal GetTotal()
    {
        return Math.Max(0, GetSubTotal() - DiscountAmount);
    }

    public int GetTotalItems()
    {
        return Items.Sum(x => x.Quantity);
    }

    public bool IsEmpty()
    {
        return !Items.Any();
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    private void ExtendExpiration()
    {
        ExpiresAt = DateTime.UtcNow.AddDays(30);
    }
}
