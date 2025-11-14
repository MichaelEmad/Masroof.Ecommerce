using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.ShoppingCarts;

public class AddToCartDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}

public class ApplyCouponDto
{
    [Required]
    [StringLength(50)]
    public string CouponCode { get; set; } = string.Empty;
}
