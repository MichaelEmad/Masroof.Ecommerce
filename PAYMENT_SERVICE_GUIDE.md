# Payment Service Implementation Guide

## Overview
This document explains the OrderPaymentService integration with ABP Payment Module and how to test it.

---

## Implementation Details

### OrderPaymentService Methods

#### 1. CreatePaymentRequestForOrderAsync
Creates a payment request for an order using ABP Payment Module.

**Parameters:**
- `orderId` (Guid): The order ID to create payment for
- `gateway` (string): Payment gateway name (default: "Stripe")

**Returns:** `PaymentRequestDto` with payment URL for redirect

**Usage:**
```csharp
var paymentRequest = await _orderPaymentService.CreatePaymentRequestForOrderAsync(orderId, "Stripe");
// Redirect user to: /Payment/GatewaySelection?paymentRequestId={paymentRequest.Id}
```

**Important Notes:**
- Order must be in `Pending` status
- Products are created from order items
- Order information is stored in ExtraProperties

---

#### 2. HandlePaymentCompletedAsync
Handles successful payment completion.

**Parameters:**
- `paymentRequestId` (Guid): The payment request ID

**Returns:** Updated `Order` entity

**When Called:**
- Automatically by ABP Payment Module webhook
- Or manually after payment gateway callback

**What It Does:**
- Verifies payment is completed
- Updates order status to `Confirmed`
- Returns updated order

---

#### 3. HandlePaymentFailedAsync
Handles failed payment.

**Parameters:**
- `paymentRequestId` (Guid): The payment request ID

**Returns:** `Order` entity (unchanged)

**What It Does:**
- Extracts order information
- Leaves order in `Pending` status
- Can be extended to send notifications, log failures, etc.

---

## ABP Payment Module Integration

### Correct API Usage

Based on ABP Payment Module 9.3.6 documentation:

#### PaymentRequestCreateDto Structure
```csharp
public class PaymentRequestCreateDto
{
    public string Currency { get; set; } // Must be 3-letter ISO code (e.g., "USD")
    public List<PaymentRequestProductCreateDto> Products { get; set; }
    public Dictionary<string, object> ExtraProperties { get; set; }
}
```

#### PaymentRequestProductCreateDto Structure
```csharp
public class PaymentRequestProductCreateDto
{
    public string Code { get; set; }      // Product code (e.g., order number)
    public string Name { get; set; }      // Product name
    public decimal UnitPrice { get; set; } // Price per unit
    public int Count { get; set; }         // Quantity
    public decimal TotalPrice { get; set; } // Total price (UnitPrice * Count)
}
```

#### PaymentRequestDto Response
```csharp
public class PaymentRequestDto
{
    public Guid Id { get; set; }
    public string Currency { get; set; }
    public PaymentRequestState State { get; set; }
    public Dictionary<string, object> ExtraProperties { get; set; }
    // ... other properties
}
```

#### PaymentRequestState Enum
```csharp
public enum PaymentRequestState
{
    Waiting = 0,
    Completed = 1,
    Failed = 2
}
```

---

## Testing Steps

### 1. Build the Project
```bash
cd src/Masroof.Ecommerce.Application
dotnet build
```

**Expected Result:** No compilation errors

---

### 2. Run Database Migration
```bash
cd src/Masroof.Ecommerce.DbMigrator
dotnet run
```

**Expected Result:** Payment tables created (PayPaymentRequests, PayPaymentRequestProducts, PayPlans, PayGatewayPlans)

---

### 3. Configure Stripe Test Keys

Edit `src/Masroof.Ecommerce.HttpApi.Host/appsettings.json`:

```json
{
  "Payment": {
    "Stripe": {
      "PublishableKey": "pk_test_51XxxxYYY",
      "SecretKey": "sk_test_51XxxxYYY",
      "WebhookSecret": "whsec_xxx",
      "PaymentMethodTypes": ["card"]
    }
  }
}
```

Get test keys from: https://dashboard.stripe.com/test/apikeys

---

### 4. Start the Application
```bash
cd src/Masroof.Ecommerce.HttpApi.Host
dotnet run
```

**Expected Output:**
```
Now listening on: https://localhost:44378
Application started. Press Ctrl+C to shut down.
```

---

### 5. Test Payment Flow

#### Step 1: Create an Order
```http
POST /api/app/order
Content-Type: application/json

{
  "customerId": "guid-here",
  "shippingAddressId": "guid-here",
  "billingAddressId": "guid-here",
  "items": [
    {
      "productId": "guid-here",
      "quantity": 2
    }
  ]
}
```

#### Step 2: Create Payment Request
```csharp
// In your OrderAppService or controller
var paymentRequest = await _orderPaymentService.CreatePaymentRequestForOrderAsync(
    orderId,
    "Stripe"
);

// Return payment request ID to frontend
return new
{
    orderId = order.Id,
    paymentRequestId = paymentRequest.Id,
    // Frontend should redirect to this URL (POST method)
    redirectUrl = $"/Payment/GatewaySelection?paymentRequestId={paymentRequest.Id}"
};
```

#### Step 3: User Pays via Stripe
- User is redirected to Stripe Checkout
- User enters test card: `4242 4242 4242 4242`
- Expiry: Any future date
- CVC: Any 3 digits
- Stripe processes payment

#### Step 4: Webhook Callback
Stripe calls: `POST /api/payment/stripe/webhook`

ABP Payment Module automatically:
- Receives webhook
- Updates PaymentRequest.State to `Completed`
- You can listen to this event to update your order

#### Step 5: Update Order Status
```csharp
// Option A: Subscribe to ABP Payment Module events
// Option B: Call manually after webhook
await _orderPaymentService.HandlePaymentCompletedAsync(paymentRequestId);
```

---

## Webhook Configuration

### Stripe Webhook Setup

1. Go to: https://dashboard.stripe.com/test/webhooks
2. Click "Add endpoint"
3. Endpoint URL: `https://yourdomain.com/api/payment/stripe/webhook`
4. Select events:
   - `checkout.session.completed`
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
5. Copy webhook secret
6. Update `appsettings.json` with webhook secret

### Webhook Events

ABP Payment Module listens for:
- **checkout.session.completed**: Payment succeeded
- **payment_intent.succeeded**: Payment intent completed
- **payment_intent.payment_failed**: Payment failed

---

## Error Handling

### Common Errors and Solutions

#### 1. "Cannot create payment for order with status X"
**Cause:** Order is not in Pending status
**Solution:** Only create payments for Pending orders

#### 2. "Payment request does not contain OrderId"
**Cause:** PaymentRequest.ExtraProperties missing OrderId
**Solution:** Ensure ExtraProperties are set during creation

#### 3. Type not found errors (PaymentRequestCreateDto)
**Cause:** Wrong DTO name or missing package reference
**Solution:** Verify package `Volo.Payment.Application.Contracts` version 9.3.6 is installed

---

## Production Checklist

Before deploying to production:

- [ ] Replace Stripe test keys with live keys
- [ ] Configure webhook with production URL
- [ ] Test complete payment flow end-to-end
- [ ] Test payment failure scenarios
- [ ] Verify order status updates correctly
- [ ] Test refund scenarios (if applicable)
- [ ] Configure payment confirmation emails
- [ ] Set up monitoring for failed payments
- [ ] Implement retry logic for failed webhooks
- [ ] Add logging for all payment operations

---

## Security Considerations

1. **API Keys**: Never commit API keys to source control
2. **Webhook Signature**: Always verify webhook signatures
3. **HTTPS Only**: Use HTTPS for all payment endpoints
4. **PCI Compliance**: Never store card details
5. **Amount Validation**: Always verify amounts match
6. **Idempotency**: Ensure webhook handlers are idempotent

---

## Troubleshooting

### Debug Payment Issues

1. **Check Payment Request:**
   ```sql
   SELECT * FROM PayPaymentRequests WHERE Id = 'payment-request-id'
   ```

2. **Check Payment Products:**
   ```sql
   SELECT * FROM PayPaymentRequestProducts WHERE PaymentRequestId = 'payment-request-id'
   ```

3. **Check Order Status:**
   ```sql
   SELECT Id, OrderNumber, Status FROM AppOrders WHERE Id = 'order-id'
   ```

4. **Check Logs:**
   ```bash
   tail -f Logs/logs.txt
   ```

### Stripe Dashboard

Monitor payments in Stripe Dashboard:
- Events: https://dashboard.stripe.com/test/events
- Payments: https://dashboard.stripe.com/test/payments
- Webhooks: https://dashboard.stripe.com/test/webhooks

---

## Additional Resources

- ABP Payment Module Docs: https://docs.abp.io/en/commercial/latest/modules/payment
- Stripe Testing: https://stripe.com/docs/testing
- ABP Support: https://support.abp.io

---

## Future Enhancements

Consider adding:

1. **Store Payment Request ID in Order:**
   ```csharp
   public class Order : FullAuditedAggregateRoot<Guid>
   {
       public Guid? PaymentRequestId { get; set; } // Add this
       // ... other properties
   }
   ```

2. **Event Handlers:**
   Subscribe to ABP Payment events for automatic order updates

3. **Multiple Currencies:**
   Support different currencies based on customer location

4. **Refund Support:**
   Implement refund processing

5. **Payment History:**
   Track all payment attempts per order

---

**Last Updated:** 2025-11-15
**ABP Version:** 9.3.6
**Payment Module Version:** 9.3.6
