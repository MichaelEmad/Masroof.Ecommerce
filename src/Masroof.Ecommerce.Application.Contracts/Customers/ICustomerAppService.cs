using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Customers;

public interface ICustomerAppService : ICrudAppService<CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>
{
    Task<CustomerDto> GetMyProfileAsync();
    Task<CustomerDto> UpdateMyProfileAsync(CreateUpdateCustomerDto input);
    Task<ListResultDto<CustomerDto>> GetVipCustomersAsync();
}
