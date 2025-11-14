using System;
using Volo.Abp;
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
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }

    protected Coupon()
    {
    }

    public Coupon(
        Guid id,
        string code,
        DiscountType discountType,
        decimal discountValue,
        decimal? minimumOrderAmount = null,
        decimal? maximumDiscountAmount = null,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        int? usageLimit = null,
        string? description = null)
        : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), maxLength: 50);
        DiscountType = discountType;
        DiscountValue = Check.Positive(discountValue, nameof(discountValue));
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UsageLimit = usageLimit;
        Description = description;
        UsageCount = 0;
        IsActive = true;
    }

    public bool IsValidForDate(DateTime date)
    {
        if (ValidFrom.HasValue && date < ValidFrom.Value)
        {
            return false;
        }

        if (ValidTo.HasValue && date > ValidTo.Value)
        {
            return false;
        }

        return true;
    }

    public bool CanBeUsed()
    {
        if (!IsActive)
        {
            return false;
        }

        if (!IsValidForDate(DateTime.UtcNow))
        {
            return false;
        }

        if (UsageLimit.HasValue && UsageCount >= UsageLimit.Value)
        {
            return false;
        }

        return true;
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (!CanBeUsed())
        {
            return 0;
        }

        if (MinimumOrderAmount.HasValue && orderAmount < MinimumOrderAmount.Value)
        {
            return 0;
        }

        decimal discount = DiscountType switch
        {
            DiscountType.Percentage => orderAmount * (DiscountValue / 100),
            DiscountType.FixedAmount => DiscountValue,
            _ => 0
        };

        if (MaximumDiscountAmount.HasValue && discount > MaximumDiscountAmount.Value)
        {
            discount = MaximumDiscountAmount.Value;
        }

        return discount;
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }
}
