using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.Orders;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Payment;
using Volo.Payment.Requests;

namespace Masroof.Ecommerce.Payments;

/// <summary>
/// Service to integrate ABP Payment Module with Order processing
/// </summary>
public class OrderPaymentService : ApplicationService
{
    private readonly IPaymentRequestAppService _paymentRequestAppService;
    private readonly IRepository<Order, Guid> _orderRepository;

    public OrderPaymentService(
        IPaymentRequestAppService paymentRequestAppService,
        IRepository<Order, Guid> orderRepository)
    {
        _paymentRequestAppService = paymentRequestAppService;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Create a payment request for an order using ABP Payment Module
    /// </summary>
    /// <param name="orderId">The order ID to create payment for</param>
    /// <param name="gateway">Payment gateway (e.g., "Stripe")</param>
    /// <returns>Payment request DTO with redirect URL for payment</returns>
    public virtual async Task<PaymentRequestDto> CreatePaymentRequestForOrderAsync(
        Guid orderId,
        string gateway = "Stripe")
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot create payment for order with status {order.Status}. " +
                $"Order must be in Pending status.");
        }

        // Create payment request using ABP Payment Module
        var paymentRequest = await _paymentRequestAppService.CreateAsync(new PaymentRequestCreationDto
        {
            // Convert order total to cents/smallest currency unit for payment gateway
            // Most payment gateways work with integers (cents for USD, etc.)
            Currency = "USD",
            Gateway = gateway,

            // Store order information in extra properties for reference
            ExtraProperties =
            {
                { "OrderId", orderId.ToString() },
                { "OrderNumber", order.OrderNumber },
                { "CustomerId", order.CustomerId.ToString() }
            }
        });

        // Set the payment amount (ABP Payment module expects amount in decimal)
        paymentRequest.Products.Add(new PaymentRequestProductCreationDto
        {
            Code = order.OrderNumber,
            Name = $"Order {order.OrderNumber}",
            UnitPrice = order.TotalAmount,
            Count = 1,
            TotalPrice = order.TotalAmount
        });

        return paymentRequest;
    }

    /// <summary>
    /// Handle payment completion callback
    /// Updates order status based on payment result
    /// </summary>
    /// <param name="paymentRequestId">The payment request ID from ABP Payment</param>
    /// <returns>The updated order</returns>
    public virtual async Task<Order> HandlePaymentCompletedAsync(Guid paymentRequestId)
    {
        // Get payment request from ABP Payment Module
        var paymentRequest = await _paymentRequestAppService.GetAsync(paymentRequestId);

        // Extract order ID from extra properties
        if (!paymentRequest.ExtraProperties.TryGetValue("OrderId", out var orderIdObj))
        {
            throw new InvalidOperationException(
                "Payment request does not contain OrderId in extra properties.");
        }

        var orderId = Guid.Parse(orderIdObj.ToString()!);
        var order = await _orderRepository.GetAsync(orderId);

        // Update order status based on payment status
        if (paymentRequest.State == PaymentRequestState.Completed)
        {
            // Payment successful - confirm the order
            order.Confirm();
            await _orderRepository.UpdateAsync(order);
        }
        else if (paymentRequest.State == PaymentRequestState.Failed)
        {
            // Payment failed - you might want to cancel the order or mark it for retry
            // This depends on your business logic
            // For now, we'll leave it in Pending state
        }

        return order;
    }

    /// <summary>
    /// Handle payment failure callback
    /// </summary>
    /// <param name="paymentRequestId">The payment request ID from ABP Payment</param>
    /// <returns>The order</returns>
    public virtual async Task<Order> HandlePaymentFailedAsync(Guid paymentRequestId)
    {
        // Get payment request from ABP Payment Module
        var paymentRequest = await _paymentRequestAppService.GetAsync(paymentRequestId);

        // Extract order ID from extra properties
        if (!paymentRequest.ExtraProperties.TryGetValue("OrderId", out var orderIdObj))
        {
            throw new InvalidOperationException(
                "Payment request does not contain OrderId in extra properties.");
        }

        var orderId = Guid.Parse(orderIdObj.ToString()!);
        var order = await _orderRepository.GetAsync(orderId);

        // You can implement custom logic here
        // For example: send notification to customer, log the failure, etc.
        // The order remains in Pending status for retry

        return order;
    }

    /// <summary>
    /// Get payment status for an order
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>Payment request DTO if exists, null otherwise</returns>
    public virtual async Task<PaymentRequestDto?> GetPaymentStatusForOrderAsync(Guid orderId)
    {
        // Get all payment requests and filter by OrderId in extra properties
        // Note: This is a simplified implementation
        // In production, you might want to store the payment request ID with the order
        // or implement a more efficient lookup mechanism

        var paymentRequests = await _paymentRequestAppService.GetListAsync(
            new PaymentRequestGetListInput
            {
                MaxResultCount = 100
            });

        foreach (var request in paymentRequests.Items)
        {
            if (request.ExtraProperties.TryGetValue("OrderId", out var orderIdObj))
            {
                if (Guid.Parse(orderIdObj.ToString()!) == orderId)
                {
                    return request;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Complete a payment request and update order
    /// This method should be called from webhook or after payment gateway redirect
    /// </summary>
    /// <param name="paymentRequestId">The payment request ID</param>
    /// <param name="gateway">Payment gateway name</param>
    /// <returns>Updated order</returns>
    public virtual async Task<Order> CompletePaymentAsync(Guid paymentRequestId, string gateway = "Stripe")
    {
        // Complete the payment using ABP Payment Module
        await _paymentRequestAppService.CompleteAsync(gateway, new Dictionary<string, string>
        {
            { "paymentRequestId", paymentRequestId.ToString() }
        });

        // Handle the completion
        return await HandlePaymentCompletedAsync(paymentRequestId);
    }
}
