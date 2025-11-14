using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Carts;

public class CartItemDto : EntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddToCartDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}
