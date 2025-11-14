using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Payments;

public class CreatePaymentDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public string? PaymentGateway { get; set; }
}

public class ProcessPaymentDto
{
    public string? TransactionId { get; set; }
    public string? CardLast4Digits { get; set; }
    public string? CardBrand { get; set; }
}
