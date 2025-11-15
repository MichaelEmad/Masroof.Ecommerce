using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Payments;

public interface IPaymentAppService : IApplicationService
{
    Task<PaymentDto> GetAsync(Guid id);
    Task<PagedResultDto<PaymentDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<PaymentDto> CreateAsync(CreatePaymentDto input);
    Task<PaymentDto> ProcessPaymentAsync(Guid id, ProcessPaymentDto input);
    Task<PaymentDto> RefundAsync(Guid id);
    Task<PaymentDto> GetByOrderIdAsync(Guid orderId);

    // Stripe integration methods
    Task<PaymentIntentResultDto> CreatePaymentIntentAsync(CreatePaymentIntentDto input);
    Task<PaymentIntentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto input);
}
