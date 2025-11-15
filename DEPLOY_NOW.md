# 🚀 DEPLOY NOW - 3 Simple Steps

## Step 1: Run Migration (2 minutes)

**Option A - Automatic:**
```bash
./QUICK_START.sh
```

**Option B - Manual SQL:**
```sql
-- Open MIGRATION.sql in SQL Server Management Studio
-- Update line 6: USE [YourDatabaseName];
-- Execute the script
```

## Step 2: Add Stripe Keys (5 minutes)

1. Go to: https://dashboard.stripe.com/test/apikeys
2. Copy keys
3. Edit `src/Masroof.Ecommerce.HttpApi.Host/appsettings.json`:

```json
"Stripe": {
  "PublishableKey": "pk_test_PASTE_HERE",
  "SecretKey": "sk_test_PASTE_HERE"
}
```

## Step 3: Run & Test (2 minutes)

```bash
cd src/Masroof.Ecommerce.HttpApi.Host
dotnet run
```

**Test checkout:**
- Card: `4242 4242 4242 4242`
- Expiry: Any future date
- CVC: Any 3 digits

## ✅ DONE!

All features working:
- ✅ Payment (Stripe + 5 more gateways)
- ✅ Auto customer creation
- ✅ Exception handling
- ✅ English + Arabic
- ✅ Email notifications
- ✅ Full e-commerce

**Need help?** Check `FINAL_PRODUCTION_CHECKLIST.md`
