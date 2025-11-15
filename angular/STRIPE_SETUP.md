# Stripe Frontend Setup - Quick Guide

## Install Stripe.js Package

Run this command in the `angular` directory:

```bash
npm install @stripe/stripe-js
```

## Update Environment Files

### Development (environment.ts)
```typescript
stripe: {
  publishableKey: 'pk_test_YOUR_PUBLISHABLE_KEY_HERE',
}
```

### Production (environment.prod.ts)
```typescript
stripe: {
  publishableKey: 'pk_live_YOUR_LIVE_PUBLISHABLE_KEY_HERE',
}
```

## Get Your Stripe Keys

1. Go to [https://dashboard.stripe.com/test/apikeys](https://dashboard.stripe.com/test/apikeys)
2. Copy your **Publishable key** (starts with `pk_test_`)
3. Replace `pk_test_YOUR_PUBLISHABLE_KEY_HERE` in environment files

## Test the Integration

1. Build the application:
   ```bash
   npm run build
   ```

2. Run the development server:
   ```bash
   npm start
   ```

3. Navigate to the payment page and test with Stripe test cards:
   - `4242 4242 4242 4242` - Successful payment
   - Any future expiry date (e.g., 12/34)
   - Any 3-digit CVC

## Need More Help?

See the main documentation: `../STRIPE_INTEGRATION_GUIDE.md`
