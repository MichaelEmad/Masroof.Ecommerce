using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Orders;

public class CreateOrderDto
{
    public Guid ShippingAddressId { get; set; }
    public Guid BillingAddressId { get; set; }
    public string? CustomerNotes { get; set; }
    public string? CouponCode { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }

    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }
    public string? AdminNotes { get; set; }
}
