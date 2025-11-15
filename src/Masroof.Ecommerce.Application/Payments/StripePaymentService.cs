using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Payments;

public class StripePaymentService : IPaymentService, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Order, Guid> _orderRepository;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Order, Guid> orderRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;

        // Set Stripe API Key
        var secretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new UserFriendlyException("Stripe secret key is not configured");
        }
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Order order, string customerEmail)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100), // Convert to cents
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Description = $"Order {order.OrderNumber}",
                ReceiptEmail = customerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "order_number", order.OrderNumber },
                    { "customer_id", order.CustomerId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe PaymentIntent {PaymentIntentId} for order {OrderNumber}",
                paymentIntent.Id, order.OrderNumber);

            return new PaymentIntentResult
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                IsSuccessful = true
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe PaymentIntent for order {OrderNumber}", order.OrderNumber);
            return new PaymentIntentResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentIntentResult> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return new PaymentIntentResult
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                IsSuccessful = paymentIntent.Status == "succeeded",
                CardLast4 = paymentIntent.Charges?.Data?.FirstOrDefault()?.PaymentMethodDetails?.Card?.Last4,
                CardBrand = paymentIntent.Charges?.Data?.FirstOrDefault()?.PaymentMethodDetails?.Card?.Brand
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error confirming Stripe PaymentIntent {PaymentIntentId}", paymentIntentId);
            return new PaymentIntentResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret is not configured");
            return;
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                webhookSecret
            );

            _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

            // Handle the event
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentSucceededAsync(paymentIntent!);
                    break;

                case Events.PaymentIntentPaymentFailed:
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentFailedAsync(failedPaymentIntent!);
                    break;

                case Events.ChargeRefunded:
                    var charge = stripeEvent.Data.Object as Charge;
                    await HandleChargeRefundedAsync(charge!);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            throw;
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            };

            if (amount.HasValue)
            {
                options.Amount = (long)(amount.Value * 100); // Convert to cents
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe refund {RefundId} for PaymentIntent {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return new RefundResult
            {
                RefundId = refund.Id,
                Status = refund.Status,
                Amount = refund.Amount / 100m, // Convert from cents
                IsSuccessful = refund.Status == "succeeded" || refund.Status == "pending"
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe refund for PaymentIntent {PaymentIntentId}", paymentIntentId);
            return new RefundResult
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task HandlePaymentIntentSucceededAsync(PaymentIntent paymentIntent)
    {
        var orderId = Guid.Parse(paymentIntent.Metadata["order_id"]);

        // Get the payment record
        var payments = await _paymentRepository.GetListAsync();
        var payment = payments.FirstOrDefault(p => p.TransactionId == paymentIntent.Id);

        if (payment != null)
        {
            // Update payment status
            payment.MarkAsSucceeded(
                paymentIntent.Id,
                paymentIntent.Charges?.Data?.FirstOrDefault()?.PaymentMethodDetails?.Card?.Last4,
                paymentIntent.Charges?.Data?.FirstOrDefault()?.PaymentMethodDetails?.Card?.Brand
            );

            await _paymentRepository.UpdateAsync(payment, autoSave: true);

            // Update order status
            var order = await _orderRepository.GetAsync(orderId);
            if (order.Status == OrderStatus.Pending)
            {
                order.Confirm();
                await _orderRepository.UpdateAsync(order, autoSave: true);
            }

            _logger.LogInformation("Payment succeeded for order {OrderId}, payment {PaymentId}",
                orderId, payment.Id);
        }
        else
        {
            _logger.LogWarning("Payment not found for PaymentIntent {PaymentIntentId}", paymentIntent.Id);
        }
    }

    private async Task HandlePaymentIntentFailedAsync(PaymentIntent paymentIntent)
    {
        var orderId = Guid.Parse(paymentIntent.Metadata["order_id"]);

        // Get the payment record
        var payments = await _paymentRepository.GetListAsync();
        var payment = payments.FirstOrDefault(p => p.TransactionId == paymentIntent.Id);

        if (payment != null)
        {
            // Update payment status
            var errorMessage = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
            payment.MarkAsFailed(errorMessage);

            await _paymentRepository.UpdateAsync(payment, autoSave: true);

            _logger.LogWarning("Payment failed for order {OrderId}: {ErrorMessage}",
                orderId, errorMessage);
        }
    }

    private async Task HandleChargeRefundedAsync(Charge charge)
    {
        if (charge.PaymentIntent == null)
        {
            return;
        }

        // Get the payment record
        var payments = await _paymentRepository.GetListAsync();
        var payment = payments.FirstOrDefault(p => p.TransactionId == charge.PaymentIntentId);

        if (payment != null)
        {
            // Update payment status
            payment.Refund();
            await _paymentRepository.UpdateAsync(payment, autoSave: true);

            // Update order status
            var order = await _orderRepository.GetAsync(payment.OrderId);
            if (order.CanBeCancelled())
            {
                order.Cancel();
                await _orderRepository.UpdateAsync(order, autoSave: true);
            }

            _logger.LogInformation("Charge refunded for payment {PaymentId}", payment.Id);
        }
    }
}
