using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Payments;

public class Payment : FullAuditedAggregateRoot<Guid>
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentGatewayResponse { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? FailureReason { get; set; }
    public string? CardLast4Digits { get; set; }
    public string? CardBrand { get; set; }

    protected Payment()
    {
    }

    public Payment(
        Guid id,
        Guid orderId,
        Guid customerId,
        decimal amount,
        PaymentMethod paymentMethod
    ) : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot process payment with status {Status}");
        }
        Status = PaymentStatus.Processing;
    }

    public void MarkAsSucceeded(string transactionId, string? cardLast4 = null, string? cardBrand = null)
    {
        if (Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot mark payment as succeeded with status {Status}");
        }
        Status = PaymentStatus.Succeeded;
        TransactionId = transactionId;
        PaidDate = DateTime.UtcNow;
        CardLast4Digits = cardLast4;
        CardBrand = cardBrand;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Cannot mark succeeded payment as failed");
        }
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Can only refund succeeded payments");
        }
        Status = PaymentStatus.Refunded;
    }

    public bool IsSuccessful()
    {
        return Status == PaymentStatus.Succeeded;
    }

    public bool CanBeRefunded()
    {
        return Status == PaymentStatus.Succeeded;
    }
}
