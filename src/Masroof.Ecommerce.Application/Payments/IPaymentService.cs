using System.Threading.Tasks;
using Masroof.Ecommerce.Orders;

namespace Masroof.Ecommerce.Payments;

public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe PaymentIntent for the given order
    /// </summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Order order, string customerEmail);

    /// <summary>
    /// Confirms a payment with the given PaymentIntent ID
    /// </summary>
    Task<PaymentIntentResult> ConfirmPaymentAsync(string paymentIntentId);

    /// <summary>
    /// Handles Stripe webhook events
    /// </summary>
    Task HandleWebhookAsync(string json, string signature);

    /// <summary>
    /// Refunds a payment
    /// </summary>
    Task<RefundResult> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
}

public class PaymentIntentResult
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
}

public class RefundResult
{
    public string RefundId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}
