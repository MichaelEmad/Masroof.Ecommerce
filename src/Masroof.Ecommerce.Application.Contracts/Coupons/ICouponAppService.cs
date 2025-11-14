using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Coupons;

public interface ICouponAppService : ICrudAppService<CouponDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCouponDto>
{
    Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto input);
    Task<CouponDto> GetByCodeAsync(string code);
    Task<ListResultDto<CouponDto>> GetActiveCouponsAsync();
    Task DeactivateCouponAsync(Guid id);
    Task ActivateCouponAsync(Guid id);
}
