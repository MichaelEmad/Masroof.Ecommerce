using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Orders;

public interface IOrderAppService : IApplicationService
{
    Task<OrderDto> CreateAsync(CreateOrderDto input);
    Task<OrderDto> GetAsync(Guid id);
    Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input);
    Task<PagedResultDto<OrderDto>> GetMyOrdersAsync(GetOrderListDto input);
    Task<OrderDto> MarkAsPaidAsync(Guid id);
    Task<OrderDto> CancelAsync(Guid id);
}
