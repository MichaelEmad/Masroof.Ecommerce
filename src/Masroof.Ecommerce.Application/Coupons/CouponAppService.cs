using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Coupons;

[Authorize(EcommercePermissions.Coupons.Default)]
public class CouponAppService : CrudAppService<Coupon, CouponDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCouponDto>, ICouponAppService
{
    public CouponAppService(IRepository<Coupon, Guid> repository) : base(repository)
    {
        GetPolicyName = EcommercePermissions.Coupons.Default;
        GetListPolicyName = EcommercePermissions.Coupons.Default;
        CreatePolicyName = EcommercePermissions.Coupons.Create;
        UpdatePolicyName = EcommercePermissions.Coupons.Edit;
        DeletePolicyName = EcommercePermissions.Coupons.Delete;
    }

    [AllowAnonymous]
    public async Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto input)
    {
        var coupon = await Repository.FirstOrDefaultAsync(c => c.Code == input.Code);

        if (coupon == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon not found",
                DiscountAmount = 0
            };
        }

        var isValid = coupon.IsValid(input.OrderAmount, out var errorMessage);

        if (!isValid)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                DiscountAmount = 0,
                Coupon = ObjectMapper.Map<Coupon, CouponDto>(coupon)
            };
        }

        var discountAmount = coupon.CalculateDiscount(input.OrderAmount);

        return new CouponValidationResultDto
        {
            IsValid = true,
            ErrorMessage = null,
            DiscountAmount = discountAmount,
            Coupon = ObjectMapper.Map<Coupon, CouponDto>(coupon)
        };
    }

    [AllowAnonymous]
    public async Task<CouponDto> GetByCodeAsync(string code)
    {
        var coupon = await Repository.FirstOrDefaultAsync(c => c.Code == code);
        if (coupon == null)
        {
            throw new UserFriendlyException("Coupon not found");
        }

        return ObjectMapper.Map<Coupon, CouponDto>(coupon);
    }

    public async Task<ListResultDto<CouponDto>> GetActiveCouponsAsync()
    {
        var coupons = await Repository.GetListAsync();
        var activeCoupons = coupons
            .Where(c => c.IsActive && c.ValidTo >= DateTime.UtcNow)
            .OrderBy(c => c.Code)
            .ToList();

        return new ListResultDto<CouponDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Coupon>, System.Collections.Generic.List<CouponDto>>(activeCoupons)
        );
    }

    [Authorize(EcommercePermissions.Coupons.Edit)]
    public async Task DeactivateCouponAsync(Guid id)
    {
        var coupon = await Repository.GetAsync(id);
        coupon.Deactivate();
        await Repository.UpdateAsync(coupon);
    }

    [Authorize(EcommercePermissions.Coupons.Edit)]
    public async Task ActivateCouponAsync(Guid id)
    {
        var coupon = await Repository.GetAsync(id);
        coupon.Activate();
        await Repository.UpdateAsync(coupon);
    }

    public override async Task<CouponDto> CreateAsync(CreateUpdateCouponDto input)
    {
        // Check if coupon code already exists
        var existingCoupon = await Repository.FirstOrDefaultAsync(c => c.Code == input.Code);
        if (existingCoupon != null)
        {
            throw new UserFriendlyException("A coupon with this code already exists");
        }

        // Validate dates
        if (input.ValidFrom >= input.ValidTo)
        {
            throw new UserFriendlyException("Valid From date must be before Valid To date");
        }

        return await base.CreateAsync(input);
    }

    protected override async Task<IQueryable<Coupon>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await Repository.GetQueryableAsync())
            .OrderByDescending(c => c.CreationTime);
    }
}
