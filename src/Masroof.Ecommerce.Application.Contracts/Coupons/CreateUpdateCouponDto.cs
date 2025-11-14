using System;
using System.ComponentModel.DataAnnotations;
using Masroof.Ecommerce.Coupons;

namespace Masroof.Ecommerce.Coupons;

public class CreateUpdateCouponDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaximumDiscountAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxUsageCount { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    [Required]
    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOneTimeUse { get; set; }
}
