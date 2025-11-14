using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Orders;

public class Order : FullAuditedAggregateRoot<Guid>
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    protected Order()
    {
    }

    public Order(
        Guid id,
        Guid customerId,
        string customerName,
        string customerEmail)
        : base(id)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        Status = OrderStatus.Pending;
    }

    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        var item = new OrderItem(
            Guid.NewGuid(),
            Id,
            productId,
            productName,
            price,
            quantity);

        OrderItems.Add(item);
        CalculateTotalAmount();
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark order as paid. Current status: {Status}");
        }
        Status = OrderStatus.Paid;
    }

    public void MarkAsCancelled()
    {
        if (Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Cannot cancel a paid order");
        }
        Status = OrderStatus.Cancelled;
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = OrderItems.Sum(x => x.GetTotalPrice());
    }
}
