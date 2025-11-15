# ABP Payment Module - Database Migration Instructions

**Created:** 2025-11-15
**Purpose:** Add ABP Payment Module tables to the database
**Status:** Migration Required Before Deployment

---

## Overview

The ABP Payment Module has been integrated into the application. This module provides multi-gateway payment support (Stripe, PayPal, 2Checkout, Alipay, Iyzico, Payu). Before deploying or testing payment functionality, you must create and apply database migrations.

---

## Option 1: Using EF Core Migrations (Recommended)

### Prerequisites
- .NET 9 SDK installed
- Connection string configured in `appsettings.json`
- Database server running and accessible

### Steps

1. **Open a terminal** in the solution root directory

2. **Navigate to the EntityFrameworkCore project:**
   ```bash
   cd src/Masroof.Ecommerce.EntityFrameworkCore
   ```

3. **Create the migration:**
   ```bash
   dotnet ef migrations add "Added_Payment_Module" \
     --startup-project ../Masroof.Ecommerce.HttpApi.Host/Masroof.Ecommerce.HttpApi.Host.csproj
   ```

4. **Apply the migration to the database:**

   **Option A:** Using DbMigrator (Recommended - also seeds data)
   ```bash
   cd ../Masroof.Ecommerce.DbMigrator
   dotnet run
   ```

   **Option B:** Direct database update
   ```bash
   cd ../Masroof.Ecommerce.EntityFrameworkCore
   dotnet ef database update \
     --startup-project ../Masroof.Ecommerce.HttpApi.Host/Masroof.Ecommerce.HttpApi.Host.csproj
   ```

5. **Verify the migration:**
   Check that the following tables were created:
   - `PayPaymentRequests`
   - `PayPaymentRequestProducts`
   - `PayPlans`
   - `PayGatewayPlans`

---

## Option 2: Manual SQL Script (If dotnet CLI not available)

If you cannot run EF Core migrations, execute the following SQL script directly in your database management tool (SQL Server Management Studio, Azure Data Studio, etc.).

### SQL Script for ABP Payment Module Tables

```sql
-- =============================================
-- ABP Payment Module Tables
-- Based on ABP 9.3.6 Payment Module Schema
-- =============================================

-- Payment Requests Table
CREATE TABLE [dbo].[PayPaymentRequests] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NULL,
    [Gateway] NVARCHAR(64) NOT NULL,
    [Currency] NVARCHAR(3) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [State] INT NOT NULL DEFAULT 0,
    [FailReason] NVARCHAR(512) NULL,
    [ExternalSubscriptionId] NVARCHAR(256) NULL,
    [CustomerId] UNIQUEIDENTIFIER NULL,
    [Email] NVARCHAR(256) NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(40) NULL,
    [CreationTime] DATETIME2 NOT NULL,
    [CreatorId] UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2 NULL,
    [LastModifierId] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeleterId] UNIQUEIDENTIFIER NULL,
    [DeletionTime] DATETIME2 NULL,
    CONSTRAINT [PK_PayPaymentRequests] PRIMARY KEY ([Id])
);

-- Indexes for PayPaymentRequests
CREATE INDEX [IX_PayPaymentRequests_TenantId] ON [dbo].[PayPaymentRequests] ([TenantId]);
CREATE INDEX [IX_PayPaymentRequests_Gateway] ON [dbo].[PayPaymentRequests] ([Gateway]);
CREATE INDEX [IX_PayPaymentRequests_State] ON [dbo].[PayPaymentRequests] ([State]);
CREATE INDEX [IX_PayPaymentRequests_CustomerId] ON [dbo].[PayPaymentRequests] ([CustomerId]);

-- Payment Request Products Table
CREATE TABLE [dbo].[PayPaymentRequestProducts] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [PaymentRequestId] UNIQUEIDENTIFIER NOT NULL,
    [Code] NVARCHAR(256) NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [Count] INT NOT NULL,
    [TotalPrice] DECIMAL(18,2) NOT NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    [PlanId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_PayPaymentRequestProducts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PayPaymentRequestProducts_PayPaymentRequests_PaymentRequestId]
        FOREIGN KEY ([PaymentRequestId]) REFERENCES [dbo].[PayPaymentRequests] ([Id]) ON DELETE CASCADE
);

-- Indexes for PayPaymentRequestProducts
CREATE INDEX [IX_PayPaymentRequestProducts_PaymentRequestId] ON [dbo].[PayPaymentRequestProducts] ([PaymentRequestId]);
CREATE INDEX [IX_PayPaymentRequestProducts_PlanId] ON [dbo].[PayPaymentRequestProducts] ([PlanId]);

-- Plans Table (for subscription-based payments)
CREATE TABLE [dbo].[PayPlans] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [TenantId] UNIQUEIDENTIFIER NULL,
    [Name] NVARCHAR(128) NOT NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(40) NULL,
    [CreationTime] DATETIME2 NOT NULL,
    [CreatorId] UNIQUEIDENTIFIER NULL,
    [LastModificationTime] DATETIME2 NULL,
    [LastModifierId] UNIQUEIDENTIFIER NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeleterId] UNIQUEIDENTIFIER NULL,
    [DeletionTime] DATETIME2 NULL,
    CONSTRAINT [PK_PayPlans] PRIMARY KEY ([Id])
);

-- Indexes for PayPlans
CREATE INDEX [IX_PayPlans_TenantId] ON [dbo].[PayPlans] ([TenantId]);
CREATE INDEX [IX_PayPlans_Name] ON [dbo].[PayPlans] ([Name]);

-- Gateway Plans Table (linking plans to specific payment gateways)
CREATE TABLE [dbo].[PayGatewayPlans] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [PlanId] UNIQUEIDENTIFIER NOT NULL,
    [Gateway] NVARCHAR(64) NOT NULL,
    [ExternalId] NVARCHAR(256) NOT NULL,
    [ExtraProperties] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_PayGatewayPlans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PayGatewayPlans_PayPlans_PlanId]
        FOREIGN KEY ([PlanId]) REFERENCES [dbo].[PayPlans] ([Id]) ON DELETE CASCADE
);

-- Indexes for PayGatewayPlans
CREATE INDEX [IX_PayGatewayPlans_PlanId] ON [dbo].[PayGatewayPlans] ([PlanId]);
CREATE INDEX [IX_PayGatewayPlans_Gateway] ON [dbo].[PayGatewayPlans] ([Gateway]);
CREATE UNIQUE INDEX [IX_PayGatewayPlans_Gateway_ExternalId] ON [dbo].[PayGatewayPlans] ([Gateway], [ExternalId]);
```

### After Running SQL Script

1. **Verify tables exist:**
   ```sql
   SELECT TABLE_NAME
   FROM INFORMATION_SCHEMA.TABLES
   WHERE TABLE_NAME LIKE 'Pay%'
   ORDER BY TABLE_NAME;
   ```

   Expected output:
   - PayGatewayPlans
   - PayPaymentRequestProducts
   - PayPaymentRequests
   - PayPlans

2. **Verify table structures:**
   ```sql
   -- Check PayPaymentRequests columns
   SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'PayPaymentRequests'
   ORDER BY ORDINAL_POSITION;

   -- Check PayPaymentRequestProducts columns
   SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'PayPaymentRequestProducts'
   ORDER BY ORDINAL_POSITION;
   ```

---

## Verification Steps

After applying the migration (either method), verify the setup:

### 1. Check Database Tables
```sql
-- List all Payment Module tables
SELECT
    t.TABLE_NAME,
    COUNT(c.COLUMN_NAME) as ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_NAME LIKE 'Pay%'
GROUP BY t.TABLE_NAME
ORDER BY t.TABLE_NAME;
```

Expected results:
| TABLE_NAME | ColumnCount |
|------------|-------------|
| PayGatewayPlans | 5 |
| PayPaymentRequestProducts | 8 |
| PayPaymentRequests | 16 |
| PayPlans | 11 |

### 2. Test Payment Request Creation

You can test the Payment Module integration by running the application and attempting to create a payment request through the API.

---

## Integration with Orders

The Payment Module is integrated with the Order system via `OrderPaymentService.cs`. When an order is created:

1. Order is saved in `AppOrders` table with status = `Pending`
2. Payment request is created in `PayPaymentRequests` table
3. Payment products are created in `PayPaymentRequestProducts` table (one per order item)
4. User is redirected to payment gateway (e.g., Stripe Checkout)
5. After payment:
   - If successful: Order status → `Confirmed`, Payment record created
   - If failed: Order status remains `Pending`

### Order-Payment Data Flow

```
AppOrders.Id → PayPaymentRequests.ExtraProperties['OrderId']
AppOrders.OrderNumber → PayPaymentRequests.ExtraProperties['OrderNumber']
AppOrderItems → PayPaymentRequestProducts (mapped 1:1)
```

---

## Configuration Required

After migration, ensure these settings are configured:

### 1. Stripe Configuration (appsettings.json)

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

### 2. Get Stripe Keys

1. Go to https://dashboard.stripe.com/test/apikeys
2. Copy **Publishable key** (starts with `pk_test_`)
3. Copy **Secret key** (starts with `sk_test_`)
4. For webhooks:
   - Go to https://dashboard.stripe.com/test/webhooks
   - Create endpoint: `https://yourdomain.com/api/payment/stripe/webhook`
   - Select events: `checkout.session.completed`, `payment_intent.succeeded`
   - Copy **Webhook secret** (starts with `whsec_`)

### 3. Update Connection String

Ensure your database connection string is correct in:
- `src/Masroof.Ecommerce.HttpApi.Host/appsettings.json`
- `src/Masroof.Ecommerce.DbMigrator/appsettings.json`

---

## Troubleshooting

### Problem: "Table PayPaymentRequests already exists"
**Solution:** The migration has already been applied. Check if tables exist:
```sql
SELECT * FROM PayPaymentRequests;
```

### Problem: "Could not find migration"
**Solution:** Ensure you're running the command from the correct directory and using the correct startup project.

### Problem: "Login failed for user"
**Solution:** Check your connection string and ensure SQL Server is running.

### Problem: "Foreign key constraint error"
**Solution:** The payment tables must be created in the correct order (as shown in SQL script above).

---

## Migration Files Location

After running `dotnet ef migrations add`, the following files will be created:

```
src/Masroof.Ecommerce.EntityFrameworkCore/Migrations/
├── 20251114121833_Initial.cs
├── 20251114121833_Initial.Designer.cs
├── 20251115XXXXXX_Added_Payment_Module.cs         ← New
├── 20251115XXXXXX_Added_Payment_Module.Designer.cs ← New
└── EcommerceDbContextModelSnapshot.cs             ← Updated
```

---

## Next Steps After Migration

1. ✅ Test payment request creation via API
2. ✅ Configure Stripe test keys
3. ✅ Test complete checkout flow
4. ✅ Test payment webhooks
5. ✅ Test payment failure scenarios
6. ✅ Switch to live keys for production

---

## Support Resources

- **ABP Payment Module Docs:** https://docs.abp.io/en/commercial/latest/modules/payment
- **Stripe Integration:** https://docs.abp.io/en/commercial/latest/modules/payment-stripe
- **EF Core Migrations:** https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/

---

## Summary

The ABP Payment Module provides production-ready payment processing with support for 6 payment gateways:
- ✅ Stripe
- ✅ PayPal
- ✅ 2Checkout
- ✅ Alipay
- ✅ Iyzico
- ✅ Payu

This migration adds the necessary database tables to enable payment functionality. The module handles payment requests, subscriptions, webhooks, and payment state management automatically.
