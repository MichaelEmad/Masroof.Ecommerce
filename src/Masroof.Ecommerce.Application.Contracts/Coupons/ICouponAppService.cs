using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Coupons;

public interface ICouponAppService :
    ICrudAppService<
        CouponDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCouponDto,
        CreateUpdateCouponDto>
{
    Task<CouponValidationResultDto> ValidateCouponAsync(ApplyCouponDto input);
    Task<CouponDto> GetByCodeAsync(string code);
}
