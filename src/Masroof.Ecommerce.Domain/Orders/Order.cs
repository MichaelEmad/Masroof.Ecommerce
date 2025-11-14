using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Orders;

public class Order : FullAuditedAggregateRoot<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }

    // Shipping Address
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;

    // Billing Address
    public string BillingFullName { get; set; } = string.Empty;
    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string BillingPhone { get; set; } = string.Empty;

    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingCarrier { get; set; }

    protected Order()
    {
        Items = new List<OrderItem>();
    }

    public Order(
        Guid id,
        string orderNumber,
        Guid customerId
    ) : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Items = new List<OrderItem>();
        Status = OrderStatus.Pending;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity, string? imageUrl = null)
    {
        Items.Add(new OrderItem(Guid.NewGuid(), productId, productName, unitPrice, quantity, imageUrl));
        RecalculateTotals();
    }

    public void SetShippingAddress(
        string fullName,
        string addressLine1,
        string city,
        string state,
        string postalCode,
        string country,
        string phone,
        string? addressLine2 = null)
    {
        ShippingFullName = fullName;
        ShippingAddressLine1 = addressLine1;
        ShippingAddressLine2 = addressLine2;
        ShippingCity = city;
        ShippingState = state;
        ShippingPostalCode = postalCode;
        ShippingCountry = country;
        ShippingPhone = phone;
    }

    public void SetBillingAddress(
        string fullName,
        string addressLine1,
        string city,
        string state,
        string postalCode,
        string country,
        string phone,
        string? addressLine2 = null)
    {
        BillingFullName = fullName;
        BillingAddressLine1 = addressLine1;
        BillingAddressLine2 = addressLine2;
        BillingCity = city;
        BillingState = state;
        BillingPostalCode = postalCode;
        BillingCountry = country;
        BillingPhone = phone;
    }

    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void SetShippingCost(decimal shippingCost)
    {
        ShippingCost = shippingCost;
        RecalculateTotals();
    }

    public void SetTax(decimal tax)
    {
        Tax = tax;
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        SubTotal = Items.Sum(x => x.GetTotalPrice());
        TotalAmount = SubTotal - DiscountAmount + ShippingCost + Tax;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot confirm order with status {Status}");
        }
        Status = OrderStatus.Confirmed;
    }

    public void Process()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot process order with status {Status}");
        }
        Status = OrderStatus.Processing;
    }

    public void Ship(string trackingNumber, string carrier)
    {
        if (Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot ship order with status {Status}");
        }
        Status = OrderStatus.Shipped;
        ShippedDate = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
        ShippingCarrier = carrier;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOperationException($"Cannot deliver order with status {Status}");
        }
        Status = OrderStatus.Delivered;
        DeliveredDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel order with status {Status}");
        }
        Status = OrderStatus.Cancelled;
    }

    public bool CanBeCancelled()
    {
        return Status != OrderStatus.Delivered && Status != OrderStatus.Cancelled;
    }
}

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
