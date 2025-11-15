using System;

namespace Masroof.Ecommerce.Payments;

public class CreatePaymentIntentDto
{
    public Guid OrderId { get; set; }
}
