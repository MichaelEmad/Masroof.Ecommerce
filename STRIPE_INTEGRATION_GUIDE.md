# Stripe Payment Gateway Integration Guide

This guide provides comprehensive instructions for setting up and using the Stripe payment gateway integration in your e-commerce platform.

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Backend Setup](#backend-setup)
4. [Frontend Setup](#frontend-setup)
5. [Configuration](#configuration)
6. [Testing](#testing)
7. [Webhook Configuration](#webhook-configuration)
8. [Going Live](#going-live)
9. [Troubleshooting](#troubleshooting)

## Overview

The Stripe integration includes:
- **Backend**: ASP.NET Core services for creating payment intents, processing payments, and handling webhooks
- **Frontend**: Angular components for displaying payment forms and confirmation pages
- **Security**: Webhook signature verification, secure API key management
- **Features**: 3D Secure support, automatic payment methods, refunds, payment status tracking

## Prerequisites

1. **Stripe Account**: Sign up at [https://stripe.com](https://stripe.com)
2. **.NET 9.0 SDK**: Already installed in your project
3. **Node.js**: For Angular development
4. **Stripe Dashboard Access**: To get API keys and configure webhooks

## Backend Setup

### 1. Install NuGet Package

The Stripe.NET package has already been added to the Application project:

```xml
<PackageReference Include="Stripe.net" Version="47.4.0" />
```

To restore packages, run:
```bash
dotnet restore
```

### 2. Configuration Files

Update `src/Masroof.Ecommerce.HttpApi.Host/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

**To get your API keys:**
1. Log in to [Stripe Dashboard](https://dashboard.stripe.com)
2. Go to Developers > API keys
3. Copy your **Secret key** (starts with `sk_test_` for test mode)
4. Copy your **Publishable key** (starts with `pk_test_` for test mode)

### 3. Implemented Backend Components

The following files have been created:

#### Payment Service Interface
- **File**: `src/Masroof.Ecommerce.Application/Payments/IPaymentService.cs`
- **Purpose**: Defines payment operations contract

#### Stripe Payment Service
- **File**: `src/Masroof.Ecommerce.Application/Payments/StripePaymentService.cs`
- **Purpose**: Implements Stripe payment operations
- **Features**:
  - Create PaymentIntent
  - Confirm payments
  - Process refunds
  - Handle webhook events

#### Payment DTOs
- **CreatePaymentIntentDto.cs**: Request to create payment
- **PaymentIntentResultDto.cs**: Response with client secret
- **ConfirmPaymentDto.cs**: Payment confirmation request

#### Webhook Controller
- **File**: `src/Masroof.Ecommerce.HttpApi/Controllers/StripeWebhookController.cs`
- **Endpoint**: `POST /api/stripe/webhook`
- **Purpose**: Receives and processes Stripe webhook events
- **Events Handled**:
  - `payment_intent.succeeded`: Payment successful
  - `payment_intent.payment_failed`: Payment failed
  - `charge.refunded`: Refund processed

#### Updated Services
- **PaymentAppService**: Added Stripe integration methods
- **OrderAppService**: Creates PaymentIntent when order is created

## Frontend Setup

### 1. Install Stripe.js Package

Navigate to the Angular project and install:

```bash
cd angular
npm install @stripe/stripe-js
```

### 2. Environment Configuration

Update environment files with your Stripe publishable key:

**Development** (`angular/src/environments/environment.ts`):
```typescript
export const environment = {
  // ... existing config
  stripe: {
    publishableKey: 'pk_test_YOUR_PUBLISHABLE_KEY_HERE',
  },
};
```

**Production** (`angular/src/environments/environment.prod.ts`):
```typescript
export const environment = {
  // ... existing config
  stripe: {
    publishableKey: 'pk_live_YOUR_LIVE_PUBLISHABLE_KEY_HERE',
  },
};
```

### 3. Implemented Frontend Components

#### Payment Component
- **Location**: `angular/src/app/ecommerce/payment/`
- **Route**: `/ecommerce/payment/:orderId`
- **Features**:
  - Loads order details
  - Displays Stripe payment form
  - Handles payment confirmation
  - Shows loading and error states
  - Supports 3D Secure authentication

#### Payment Confirmation Component
- **Location**: `angular/src/app/ecommerce/payment-confirmation/`
- **Route**: `/ecommerce/payment-confirmation/:orderId`
- **Features**:
  - Shows payment success/failure status
  - Displays order details and items
  - Shows shipping address
  - Provides order invoice download
  - Navigation to order history

#### Updated Checkout Flow
The checkout component now redirects to the payment page after order creation instead of showing a simple confirmation.

## Configuration

### 1. Get Stripe API Keys

**Test Mode Keys** (for development):
1. Go to [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)
2. Copy **Publishable key** → Add to Angular environment files
3. Copy **Secret key** → Add to backend appsettings.json

**Live Mode Keys** (for production):
1. Complete Stripe account verification
2. Go to [Live API Keys](https://dashboard.stripe.com/apikeys)
3. Copy **Publishable key** → Add to Angular production environment
4. Copy **Secret key** → Add to backend production configuration

### 2. Update Configuration Files

**Backend** (`appsettings.json`):
```json
{
  "Stripe": {
    "SecretKey": "sk_test_51...",
    "PublishableKey": "pk_test_51...",
    "WebhookSecret": "whsec_..." // Add after webhook setup
  }
}
```

**Frontend** (`environment.ts`):
```typescript
stripe: {
  publishableKey: 'pk_test_51...',
}
```

## Testing

### 1. Test Cards

Stripe provides test cards for different scenarios:

| Card Number | Scenario |
|------------|----------|
| `4242 4242 4242 4242` | Successful payment |
| `4000 0025 0000 3155` | Requires 3D Secure authentication |
| `4000 0000 0000 9995` | Declined payment |
| `4000 0000 0000 0341` | Attaching fails |

**For all test cards:**
- Use any future expiration date (e.g., 12/34)
- Use any 3-digit CVC
- Use any postal code

### 2. Testing Payment Flow

1. **Create an Order**:
   - Add items to cart
   - Proceed to checkout
   - Fill in shipping/billing addresses
   - Click "Place Order"

2. **Complete Payment**:
   - You'll be redirected to the payment page
   - Use a test card number
   - Fill in card details
   - Click "Pay"

3. **View Confirmation**:
   - After successful payment, you'll see the confirmation page
   - Order status will change to "Confirmed"

### 3. Testing Webhooks Locally

To test webhooks during development:

1. **Install Stripe CLI**:
   ```bash
   # macOS
   brew install stripe/stripe-cli/stripe

   # Windows (using Scoop)
   scoop install stripe
   ```

2. **Login to Stripe CLI**:
   ```bash
   stripe login
   ```

3. **Forward webhooks to local server**:
   ```bash
   stripe listen --forward-to https://localhost:44302/api/stripe/webhook
   ```

4. **Copy the webhook secret** from the output and add to `appsettings.json`:
   ```
   Your webhook signing secret is whsec_...
   ```

5. **Test webhook events**:
   ```bash
   stripe trigger payment_intent.succeeded
   ```

## Webhook Configuration

### 1. Create Webhook Endpoint

1. Go to [Stripe Dashboard > Webhooks](https://dashboard.stripe.com/test/webhooks)
2. Click "Add endpoint"
3. Enter your endpoint URL:
   - **Development**: Use Stripe CLI (see Testing section)
   - **Production**: `https://yourdomain.com/api/stripe/webhook`

### 2. Select Events

Select the following events:
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `charge.refunded`

### 3. Get Webhook Secret

1. After creating the endpoint, click on it
2. Click "Reveal" under "Signing secret"
3. Copy the secret (starts with `whsec_`)
4. Add to `appsettings.json`:
   ```json
   {
     "Stripe": {
       "WebhookSecret": "whsec_..."
     }
   }
   ```

## Going Live

### 1. Prerequisites for Live Mode

Before going live, ensure:
- [ ] Stripe account is fully activated
- [ ] Business information is complete
- [ ] Bank account is connected
- [ ] Identity verification is complete

### 2. Switch to Live Mode

1. **Get Live API Keys**:
   - Go to Stripe Dashboard
   - Toggle to "Live mode" (top right)
   - Go to Developers > API keys
   - Copy live keys (start with `pk_live_` and `sk_live_`)

2. **Update Backend Configuration**:
   ```json
   {
     "Stripe": {
       "SecretKey": "sk_live_...",
       "PublishableKey": "pk_live_...",
       "WebhookSecret": "whsec_..." // Live webhook secret
     }
   }
   ```

3. **Update Frontend Configuration**:
   ```typescript
   // environment.prod.ts
   stripe: {
     publishableKey: 'pk_live_...',
   }
   ```

4. **Create Live Webhook**:
   - Go to Developers > Webhooks
   - Add endpoint with your production URL
   - Select same events as test mode
   - Copy webhook secret to production config

### 3. Security Checklist

- [ ] Store API keys in secure environment variables (not in code)
- [ ] Enable HTTPS on production server
- [ ] Implement rate limiting on webhook endpoint
- [ ] Monitor Stripe Dashboard for suspicious activity
- [ ] Set up alerts for failed payments
- [ ] Regularly rotate API keys
- [ ] Use restricted API keys with minimal permissions

### 4. Deployment

1. **Build Backend**:
   ```bash
   dotnet publish -c Release
   ```

2. **Build Frontend**:
   ```bash
   cd angular
   npm run build -- --configuration production
   ```

3. **Deploy and Test**:
   - Deploy to production server
   - Test with live test cards first
   - Make a small real transaction to verify
   - Monitor webhook events in Stripe Dashboard

## Troubleshooting

### Common Issues

#### 1. "Stripe secret key is not configured"
- **Cause**: Missing or incorrect API key in appsettings.json
- **Solution**: Verify API key is correctly copied from Stripe Dashboard

#### 2. Payment form not loading
- **Cause**: Incorrect publishable key in Angular environment
- **Solution**: Check environment.ts has correct publishable key

#### 3. Webhook signature verification failed
- **Cause**: Incorrect webhook secret or request modified
- **Solution**:
  - Verify webhook secret matches Stripe Dashboard
  - Ensure webhook endpoint is publicly accessible
  - Check firewall/proxy settings

#### 4. Payment succeeds but order status not updated
- **Cause**: Webhook not configured or failing
- **Solution**:
  - Check webhook endpoint in Stripe Dashboard
  - Verify webhook events are being sent
  - Check backend logs for webhook processing errors

#### 5. 3D Secure authentication fails
- **Cause**: Browser blocking popup or iframe
- **Solution**: Ensure pop-up blocker is disabled for your domain

### Debug Mode

Enable detailed logging for troubleshooting:

**Backend** (appsettings.json):
```json
{
  "Logging": {
    "LogLevel": {
      "Masroof.Ecommerce.Payments": "Debug"
    }
  }
}
```

**Frontend** (payment.component.ts):
```typescript
// Add console.log statements in catch blocks
console.error('Payment error:', error);
```

### Support Resources

- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Support](https://support.stripe.com/)
- [Stripe Status](https://status.stripe.com/)
- [Stripe Testing Guide](https://stripe.com/docs/testing)

## Additional Features

### Refund Processing

Refunds can be processed through the admin panel:

```typescript
// In PaymentAppService
await paymentService.RefundAsync(paymentId);
```

### Custom Metadata

Add custom data to payment intents:

```csharp
// In StripePaymentService
Metadata = new Dictionary<string, string>
{
    { "order_id", order.Id.ToString() },
    { "customer_email", customerEmail },
    { "custom_field", "value" }
}
```

### Payment Methods

The integration automatically supports:
- Credit/Debit Cards
- Apple Pay (if configured)
- Google Pay (if configured)
- Link (Stripe's one-click checkout)

To add more payment methods, update the PaymentElement configuration in the payment component.

## Best Practices

1. **Always Use HTTPS**: Both in development and production
2. **Validate Webhooks**: Always verify webhook signatures
3. **Handle Errors Gracefully**: Show user-friendly error messages
4. **Test Thoroughly**: Use test cards before going live
5. **Monitor Payments**: Regularly check Stripe Dashboard
6. **Keep Logs**: Log all payment events for debugging
7. **Update Regularly**: Keep Stripe.NET and Stripe.js up to date
8. **Secure API Keys**: Never commit API keys to version control

## Need Help?

If you encounter issues:
1. Check this guide's troubleshooting section
2. Review Stripe Dashboard for errors
3. Check backend and frontend logs
4. Contact Stripe support
5. Review Stripe API documentation

---

**Last Updated**: November 2025
**Stripe.NET Version**: 47.4.0
**@stripe/stripe-js Version**: Latest
