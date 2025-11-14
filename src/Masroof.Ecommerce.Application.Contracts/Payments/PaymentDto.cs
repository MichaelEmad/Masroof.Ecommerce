using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Payments;

public class PaymentDto : FullAuditedEntityDto<Guid>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? FailureReason { get; set; }
    public string? CardLast4Digits { get; set; }
    public string? CardBrand { get; set; }
}
