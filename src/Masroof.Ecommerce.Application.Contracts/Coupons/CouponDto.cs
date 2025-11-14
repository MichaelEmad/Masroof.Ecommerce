using System;
using Masroof.Ecommerce.Coupons;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Coupons;

public class CouponDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsOneTimeUse { get; set; }
    public bool IsExpired { get; set; }
    public int RemainingUses { get; set; }
}
