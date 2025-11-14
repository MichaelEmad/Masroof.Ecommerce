namespace Masroof.Ecommerce.Coupons;

public enum DiscountType
{
    /// <summary>
    /// Discount is a percentage off the total (e.g., 10% off)
    /// </summary>
    Percentage = 0,

    /// <summary>
    /// Discount is a fixed amount off the total (e.g., $10 off)
    /// </summary>
    FixedAmount = 1
}
