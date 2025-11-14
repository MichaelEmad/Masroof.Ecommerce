using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.ShoppingCarts;

public class ShoppingCartDto : EntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public int TotalItems { get; set; }
    public bool IsEmpty { get; set; }
}

public class CartItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public decimal TotalPrice { get; set; }
}
