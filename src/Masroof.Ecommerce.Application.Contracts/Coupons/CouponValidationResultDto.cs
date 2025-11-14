namespace Masroof.Ecommerce.Coupons;

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public CouponDto? Coupon { get; set; }
}
