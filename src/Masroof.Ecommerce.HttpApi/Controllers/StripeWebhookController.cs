using System;
using System.IO;
using System.Threading.Tasks;
using Masroof.Ecommerce.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace Masroof.Ecommerce.Controllers;

[Area("app")]
[Route("api/stripe/webhook")]
[ApiController]
[AllowAnonymous] // Webhooks come from Stripe, not authenticated users
public class StripeWebhookController : AbpControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IPaymentService paymentService,
        ILogger<StripeWebhookController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Read the request body
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            // Get the Stripe signature header
            var signature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Webhook received without Stripe-Signature header");
                return BadRequest("Missing Stripe-Signature header");
            }

            // Handle the webhook
            await _paymentService.HandleWebhookAsync(json, signature);

            _logger.LogInformation("Successfully processed Stripe webhook");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return BadRequest($"Webhook processing failed: {ex.Message}");
        }
    }
}
