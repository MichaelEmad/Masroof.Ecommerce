# ABP Payment Module Installation Guide

## Overview
This document provides instructions for completing the installation of ABP Payment Module (version 9.3.6) with Stripe integration for the Masroof E-Commerce platform.

## What Has Been Installed

### 1. Package References Added

The following ABP Payment module packages (version 9.3.6) have been added:

#### Domain.Shared Project
- `Volo.Payment.Domain.Shared`

#### Domain Project
- `Volo.Payment.Domain`

#### Application.Contracts Project
- `Volo.Payment.Application.Contracts`

#### Application Project
- `Volo.Payment.Application`

#### EntityFrameworkCore Project
- `Volo.Payment.EntityFrameworkCore`

#### HttpApi Project
- `Volo.Payment.HttpApi`

#### HttpApi.Host Project
- `Volo.Payment.Stripe` (Stripe gateway provider)

### 2. Module Dependencies Updated

All module classes have been updated to include payment module dependencies:

- `EcommerceDomainSharedModule` → Added `PaymentDomainSharedModule`
- `EcommerceDomainModule` → Added `PaymentDomainModule`
- `EcommerceApplicationContractsModule` → Added `PaymentApplicationContractsModule`
- `EcommerceApplicationModule` → Added `PaymentApplicationModule`
- `EcommerceEntityFrameworkCoreModule` → Added `PaymentEntityFrameworkCoreModule`
- `EcommerceHttpApiModule` → Added `PaymentHttpApiModule`
- `EcommerceHttpApiHostModule` → Added `PaymentStripeModule`

### 3. DbContext Configuration

The `EcommerceDbContext` has been configured with:
- Implemented `IPaymentDbContext` interface
- Added `ReplaceDbContext` attribute for `IPaymentDbContext`
- Added `builder.ConfigurePayment()` in `OnModelCreating` method

### 4. Stripe Configuration

Added Stripe configuration to `appsettings.json`:
```json
{
  "Payment": {
    "Stripe": {
      "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY",
      "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY",
      "WebhookSecret": "whsec_YOUR_STRIPE_WEBHOOK_SECRET",
      "PaymentMethodTypes": ["card"]
    }
  }
}
```

### 5. OrderPaymentService Integration

Created `/src/Masroof.Ecommerce.Application/Payments/OrderPaymentService.cs` with the following capabilities:

- `CreatePaymentRequestForOrderAsync()` - Creates payment requests for orders using ABP Payment Module
- `HandlePaymentCompletedAsync()` - Handles payment completion callbacks and updates order status
- `HandlePaymentFailedAsync()` - Handles payment failure callbacks
- `GetPaymentStatusForOrderAsync()` - Retrieves payment status for an order
- `CompletePaymentAsync()` - Completes payment and updates order

## Next Steps

### 1. Restore NuGet Packages

Run the following command to restore all packages:

```bash
dotnet restore
```

### 2. Create Database Migration

The ABP Payment Module includes database tables for payment requests. Create a new migration:

```bash
# Navigate to the DbMigrator or HttpApi.Host project
cd src/Masroof.Ecommerce.HttpApi.Host

# Add a new migration
dotnet ef migrations add "Added_Payment_Module" --project ../Masroof.Ecommerce.EntityFrameworkCore/Masroof.Ecommerce.EntityFrameworkCore.csproj

# Apply the migration
dotnet ef database update --project ../Masroof.Ecommerce.EntityFrameworkCore/Masroof.Ecommerce.EntityFrameworkCore.csproj
```

### 3. Configure Stripe API Keys

Replace the placeholder values in `appsettings.json` with your actual Stripe credentials:

1. **Get Stripe Keys:**
   - Log in to [Stripe Dashboard](https://dashboard.stripe.com/)
   - Navigate to Developers → API keys
   - Copy your Publishable key and Secret key

2. **Get Webhook Secret:**
   - Navigate to Developers → Webhooks
   - Click "Add endpoint"
   - Set the endpoint URL to: `https://your-domain.com/payment/stripe/webhook`
   - Select events to listen to (at minimum: `payment_intent.succeeded`, `payment_intent.payment_failed`)
   - Copy the webhook signing secret

3. **Update appsettings.json:**
   ```json
   {
     "Payment": {
       "Stripe": {
         "PublishableKey": "pk_test_51ABC...",
         "SecretKey": "sk_test_51ABC...",
         "WebhookSecret": "whsec_ABC...",
         "PaymentMethodTypes": ["card"]
       }
     }
   }
   ```

4. **For Production:**
   - Use environment-specific configuration files (`appsettings.Production.json`)
   - Store secrets in Azure Key Vault, AWS Secrets Manager, or environment variables
   - Replace `pk_test_` and `sk_test_` with live keys (`pk_live_` and `sk_live_`)

### 4. Register OrderPaymentService

The `OrderPaymentService` should be registered in the dependency injection container. Add this to `EcommerceApplicationModule.cs`:

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<AbpAutoMapperOptions>(options =>
    {
        options.AddMaps<EcommerceApplicationModule>();
    });

    // Register OrderPaymentService
    context.Services.AddTransient<OrderPaymentService>();

    // ... existing code
}
```

### 5. Build and Test

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src/Masroof.Ecommerce.HttpApi.Host/Masroof.Ecommerce.HttpApi.Host.csproj
```

### 6. Test Payment Flow

#### Example Usage in OrderAppService:

```csharp
public class OrderAppService : ApplicationService
{
    private readonly OrderPaymentService _orderPaymentService;

    public OrderAppService(OrderPaymentService orderPaymentService)
    {
        _orderPaymentService = orderPaymentService;
    }

    public async Task<PaymentRequestDto> CreateOrderPaymentAsync(Guid orderId)
    {
        // Create payment request for the order
        var paymentRequest = await _orderPaymentService.CreatePaymentRequestForOrderAsync(
            orderId,
            gateway: "Stripe"
        );

        // Return the payment request with checkout URL
        // The client will redirect to this URL for payment
        return paymentRequest;
    }

    // This method should be called from Stripe webhook
    public async Task HandlePaymentWebhookAsync(Guid paymentRequestId)
    {
        // Complete the payment and update order
        await _orderPaymentService.HandlePaymentCompletedAsync(paymentRequestId);
    }
}
```

### 7. Configure Webhook Endpoint

ABP Payment Module automatically provides webhook endpoints. Ensure your application is publicly accessible and configure the webhook URL in Stripe dashboard:

- Webhook URL: `https://your-domain.com/api/payment/stripe/webhook`
- Events to subscribe:
  - `payment_intent.succeeded`
  - `payment_intent.payment_failed`
  - `checkout.session.completed`
  - `checkout.session.expired`

### 8. Frontend Integration

For Angular frontend, generate the proxy:

```bash
# Navigate to angular folder
cd angular

# Generate payment service proxy
abp generate-proxy -t ng -m payment
```

This will create TypeScript service proxies for payment APIs.

## Database Schema Changes

The payment module will add the following tables to your database:

- `PayPaymentRequests` - Stores payment request information
- `PayPaymentRequestProducts` - Stores product items in payment requests
- `PayPlans` - For subscription plans (if using subscription features)
- `PayGatewayPlans` - Gateway-specific plan configurations

## Security Considerations

1. **Never expose Secret Keys** - Keep `SecretKey` and `WebhookSecret` secure
2. **Use HTTPS** - Always use HTTPS in production for webhook endpoints
3. **Validate Webhooks** - The module automatically validates webhook signatures
4. **PCI Compliance** - Since Stripe handles card data, you don't need PCI compliance, but follow Stripe's best practices

## Troubleshooting

### Issue: Migration fails with "table already exists"

**Solution:** If you have custom Payment tables, rename them or drop them before running migration.

### Issue: Webhook returns 400/401

**Solution:**
- Verify webhook secret is correct
- Ensure webhook URL is publicly accessible
- Check application logs for detailed error messages

### Issue: Payment creation fails

**Solution:**
- Verify Stripe API keys are correct
- Check that order is in "Pending" status
- Review application logs for API errors

## Additional Resources

- [ABP Payment Module Documentation](https://docs.abp.io/en/commercial/latest/modules/payment)
- [Stripe Documentation](https://stripe.com/docs)
- [ABP Framework Documentation](https://docs.abp.io/)

## Support

For issues specific to:
- **ABP Payment Module**: [ABP Support](https://support.abp.io/)
- **Stripe Integration**: [Stripe Support](https://support.stripe.com/)
- **This Implementation**: Contact your development team

---

**Installation Date:** 2025-11-15
**ABP Version:** 9.3.6
**Payment Module Version:** 9.3.6
**Stripe Gateway Version:** 9.3.6
