using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Coupons;

public class CouponAppService :
    CrudAppService<
        Coupon,
        CouponDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCouponDto,
        CreateUpdateCouponDto>,
    ICouponAppService
{
    public CouponAppService(IRepository<Coupon, Guid> repository)
        : base(repository)
    {
        GetPolicyName = EcommercePermissions.Coupons.Default;
        GetListPolicyName = EcommercePermissions.Coupons.Default;
        CreatePolicyName = EcommercePermissions.Coupons.Create;
        UpdatePolicyName = EcommercePermissions.Coupons.Edit;
        DeletePolicyName = EcommercePermissions.Coupons.Delete;
    }

    public async Task<CouponDto> GetByCodeAsync(string code)
    {
        var coupon = await Repository.FirstOrDefaultAsync(x => x.Code == code);
        if (coupon == null)
        {
            throw new UserFriendlyException($"Coupon with code '{code}' not found");
        }

        return ObjectMapper.Map<Coupon, CouponDto>(coupon);
    }

    public async Task<CouponValidationResultDto> ValidateCouponAsync(ApplyCouponDto input)
    {
        var coupon = await Repository.FirstOrDefaultAsync(x => x.Code == input.CouponCode);

        if (coupon == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Invalid coupon code",
                DiscountAmount = 0
            };
        }

        if (!coupon.IsActive)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon is no longer active",
                DiscountAmount = 0
            };
        }

        if (!coupon.IsValidForDate(DateTime.UtcNow))
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon has expired or is not yet valid",
                DiscountAmount = 0
            };
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "This coupon has reached its usage limit",
                DiscountAmount = 0
            };
        }

        if (coupon.MinimumOrderAmount.HasValue && input.OrderAmount < coupon.MinimumOrderAmount.Value)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = $"Minimum order amount of {coupon.MinimumOrderAmount:C} required for this coupon",
                DiscountAmount = 0
            };
        }

        var discountAmount = coupon.CalculateDiscount(input.OrderAmount);

        return new CouponValidationResultDto
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            Coupon = ObjectMapper.Map<Coupon, CouponDto>(coupon)
        };
    }
}
