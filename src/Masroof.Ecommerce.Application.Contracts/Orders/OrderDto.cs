using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Orders;

public class OrderDto : FullAuditedEntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderDto
{
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class GetOrderListDto : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
}
