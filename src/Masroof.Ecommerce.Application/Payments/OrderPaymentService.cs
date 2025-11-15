using System;
using System.Collections.Generic;
using System.Linq;
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
    public virtual async Task<PaymentRequestWithDetailsDto> CreatePaymentRequestForOrderAsync(
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
        // Products must be added during creation, not after
        var createDto = new PaymentRequestCreateDto
        {
            Currency = "USD", // Must be 3-letter ISO code
            Products = new List<PaymentRequestProductCreateDto>
            {
                new PaymentRequestProductCreateDto
                {
                    Code = order.OrderNumber,
                    Name = $"Order {order.OrderNumber}",
                    UnitPrice = (float)order.TotalAmount,
                    Count = 1,
                    TotalPrice = (float)order.TotalAmount
                }
            }
        };

        // Store order information in extra properties for reference
        createDto.ExtraProperties.Add("OrderId", orderId.ToString());
        createDto.ExtraProperties.Add("OrderNumber", order.OrderNumber);
        createDto.ExtraProperties.Add("CustomerId", order.CustomerId.ToString());

        var paymentRequest = await _paymentRequestAppService.CreateAsync(createDto);

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
    /// <remarks>
    /// Note: This is a simplified implementation.
    /// In production, you should store the payment request ID with the order
    /// for more efficient lookup.
    /// </remarks>
    public virtual async Task<PaymentRequestWithDetailsDto?> GetPaymentStatusForOrderAsync(Guid orderId)
    {
        // This is a simplified implementation
        // ABP Payment module doesn't provide query by extra properties
        // In production, you should:
        // 1. Add a PaymentRequestId property to Order entity
        // 2. Store the payment request ID when creating payment
        // 3. Use GetAsync with the stored ID

        // For now, this method returns null
        // You should enhance this based on your requirements
        return await Task.FromResult<PaymentRequestWithDetailsDto?>(null);
    }

    /// <summary>
    /// Process payment webhook callback from payment gateway
    /// This is typically called by ABP Payment module's webhook endpoint
    /// </summary>
    /// <param name="paymentRequestId">The payment request ID</param>
    /// <returns>Updated order</returns>
    /// <remarks>
    /// ABP Payment module handles webhook processing automatically.
    /// This method is for manual processing if needed.
    /// You should configure payment gateway webhooks to call ABP's built-in endpoints:
    /// - Stripe: POST /api/payment/stripe/webhook
    /// - PayPal: POST /api/payment/paypal/webhook
    /// </remarks>
    public virtual async Task<Order> ProcessPaymentWebhookAsync(Guid paymentRequestId)
    {
        // Handle the payment completion
        // ABP Payment module will have already updated the payment request state
        return await HandlePaymentCompletedAsync(paymentRequestId);
    }
}
