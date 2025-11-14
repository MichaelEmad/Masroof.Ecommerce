using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Addresses;

public interface IAddressAppService : ICrudAppService<AddressDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateAddressDto>
{
    Task<ListResultDto<AddressDto>> GetMyAddressesAsync();
    Task<AddressDto> GetDefaultAddressAsync(Guid customerId);
    Task SetAsDefaultAsync(Guid id);
}
