using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Orders;

public interface IOrderAppService : IApplicationService
{
    Task<OrderDto> GetAsync(Guid id);
    Task<PagedResultDto<OrderDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<OrderDto> CreateAsync(CreateOrderDto input);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto input);
    Task<ListResultDto<OrderDto>> GetMyOrdersAsync();
    Task<OrderDto> GetMyOrderAsync(Guid id);
    Task CancelAsync(Guid id);
}
