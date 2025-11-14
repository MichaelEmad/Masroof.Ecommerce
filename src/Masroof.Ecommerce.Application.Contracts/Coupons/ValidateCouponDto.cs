using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Coupons;

public class ValidateCouponDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OrderAmount { get; set; }
}
