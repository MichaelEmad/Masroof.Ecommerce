using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Coupons;

public class Coupon : FullAuditedAggregateRoot<Guid>
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

    protected Coupon()
    {
    }

    public Coupon(
        Guid id,
        string code,
        DiscountType discountType,
        decimal discountValue,
        DateTime validFrom,
        DateTime validTo
    ) : base(id)
    {
        Code = code;
        DiscountType = discountType;
        DiscountValue = discountValue;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = true;
        UsageCount = 0;
        IsOneTimeUse = false;
    }

    public bool IsValid(decimal orderAmount, out string? errorMessage)
    {
        errorMessage = null;

        if (!IsActive)
        {
            errorMessage = "Coupon is not active";
            return false;
        }

        var now = DateTime.UtcNow;
        if (now < ValidFrom)
        {
            errorMessage = "Coupon is not yet valid";
            return false;
        }

        if (now > ValidTo)
        {
            errorMessage = "Coupon has expired";
            return false;
        }

        if (MinimumOrderAmount.HasValue && orderAmount < MinimumOrderAmount.Value)
        {
            errorMessage = $"Minimum order amount of ${MinimumOrderAmount.Value} required";
            return false;
        }

        if (MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value)
        {
            errorMessage = "Coupon usage limit reached";
            return false;
        }

        return true;
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (!IsValid(orderAmount, out _))
        {
            return 0;
        }

        decimal discount = DiscountType switch
        {
            DiscountType.Percentage => orderAmount * (DiscountValue / 100),
            DiscountType.FixedAmount => DiscountValue,
            _ => 0
        };

        // Apply maximum discount cap if specified
        if (MaximumDiscountAmount.HasValue && discount > MaximumDiscountAmount.Value)
        {
            discount = MaximumDiscountAmount.Value;
        }

        // Ensure discount doesn't exceed order amount
        if (discount > orderAmount)
        {
            discount = orderAmount;
        }

        return discount;
    }

    public void IncrementUsageCount()
    {
        UsageCount++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
