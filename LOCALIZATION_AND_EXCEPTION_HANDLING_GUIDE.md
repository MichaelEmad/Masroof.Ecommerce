# Localization and Exception Handling Implementation Guide

This document provides guidance on using the comprehensive exception handling and bilingual localization (English + Arabic) features implemented in the Masroof E-Commerce platform.

## Table of Contents
1. [Exception Handling](#exception-handling)
2. [Localization](#localization)
3. [Usage Examples](#usage-examples)

---

## Exception Handling

### Global Exception Filter

The platform now includes a centralized `GlobalExceptionFilter` that automatically handles all unhandled exceptions across the application.

#### File Location
- **Filter**: `/src/Masroof.Ecommerce.HttpApi.Host/Filters/GlobalExceptionFilter.cs`
- **DTO**: `/src/Masroof.Ecommerce.Application.Contracts/Dtos/ErrorResponseDto.cs`

#### Exception Types Handled

1. **UserFriendlyException** → 400 Bad Request
   - Returns the message as-is to the user

2. **EntityNotFoundException** → 404 Not Found
   - Returns a friendly "not found" message

3. **AbpValidationException** → 400 Bad Request
   - Returns validation errors in a user-friendly format

4. **AbpAuthorizationException** → 403 Forbidden
   - Returns "permission denied" message

5. **UnauthorizedAccessException** → 401 Unauthorized
   - Returns "authentication required" message

6. **BusinessException** → 400 Bad Request
   - Returns business logic error messages

7. **All Other Exceptions** → 500 Internal Server Error
   - Returns generic error message (technical details are hidden from users)
   - Full exception details are logged for debugging

#### Error Response Format

```json
{
  "statusCode": 404,
  "errorCode": "ENTITY_NOT_FOUND",
  "message": "Product not found",
  "details": "Entity Type: Product",
  "timestamp": "2025-11-15T10:30:00.000Z"
}
```

---

## Localization

### Supported Languages

- **English (en)** - Default language
- **Arabic (ar)** - RTL (Right-to-Left) support enabled

### Localization Files Location

- **English**: `/src/Masroof.Ecommerce.Domain.Shared/Localization/Ecommerce/en.json`
- **Arabic**: `/src/Masroof.Ecommerce.Domain.Shared/Localization/Ecommerce/ar.json`

### Translation Categories

The localization files include 200+ translations across the following categories:

1. **Common UI Elements** (Common:*)
   - Buttons, actions, labels

2. **Product Catalog** (Product:*)
   - Product details, stock status, actions

3. **Categories** (Category:*)
   - Product categories

4. **Shopping Cart** (Cart:*)
   - Cart operations, messages

5. **Orders** (Order:*)
   - Order status, details, actions

6. **Payment** (Payment:*)
   - Payment methods, transaction details

7. **Checkout** (Checkout:*)
   - Checkout process, coupons

8. **User Account** (User:*)
   - Profile, addresses, authentication

9. **Validation Messages** (Validation:*)
   - Form validation errors

10. **Error Messages** (Error:*)
    - System and user errors

11. **Success Messages** (Success:*)
    - Operation confirmations

12. **General Messages** (Message:*)
    - Confirmations, loading states

---

## Usage Examples

### 1. Using Localization in Application Services

```csharp
using Volo.Abp.Application.Services;
using Microsoft.Extensions.Localization;
using Masroof.Ecommerce.Localization;

public class ProductAppService : ApplicationService
{
    public ProductAppService()
    {
        // This automatically injects the localization service
        LocalizationResource = typeof(EcommerceResource);
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var product = await _productRepository.FindAsync(id);

        if (product == null)
        {
            // Throw user-friendly exception with localized message
            throw new UserFriendlyException(L["Product:ProductNotFound"]);
        }

        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task AddToCartAsync(Guid productId, int quantity)
    {
        var product = await _productRepository.GetAsync(productId);

        if (product.StockQuantity < quantity)
        {
            // Throw business exception with localized message
            throw new BusinessException(
                code: "INSUFFICIENT_STOCK",
                message: L["Cart:InsufficientStock"],
                details: $"Available: {product.StockQuantity}, Requested: {quantity}"
            );
        }

        // Add to cart logic...

        // Return success message
        return L["Cart:ItemAddedToCart"];
    }
}
```

### 2. Using Localization in Controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Masroof.Ecommerce.Localization;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IStringLocalizer<EcommerceResource> _localizer;
    private readonly IProductAppService _productAppService;

    public ProductController(
        IStringLocalizer<EcommerceResource> localizer,
        IProductAppService productAppService)
    {
        _localizer = localizer;
        _productAppService = productAppService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _productAppService.GetAsync(id);
        return Ok(product);
    }

    [HttpPost("{id}/add-to-cart")]
    public async Task<IActionResult> AddToCart(Guid id, [FromBody] AddToCartDto input)
    {
        await _productAppService.AddToCartAsync(id, input.Quantity);

        return Ok(new
        {
            message = _localizer["Cart:ItemAddedToCart"].Value
        });
    }
}
```

### 3. Throwing Custom Exceptions

```csharp
// User-friendly exception (message shown to user)
throw new UserFriendlyException(L["Order:OrderNotFound"]);

// Business exception with custom code
throw new BusinessException("INVALID_COUPON")
    .WithData("CouponCode", couponCode);

// Validation exception
throw new AbpValidationException(
    new List<ValidationResult>
    {
        new ValidationResult(
            L["Validation:Required"].Value,
            new[] { "Email" }
        )
    }
);

// Authorization exception
throw new AbpAuthorizationException(L["Error:PermissionDenied"]);
```

### 4. Language Switching (Client-Side)

#### Angular Example

```typescript
// In your component
import { ConfigStateService } from '@abp/ng.core';

export class LanguageSwitcherComponent {
  constructor(private configState: ConfigStateService) {}

  changeLanguage(culture: string) {
    this.configState.dispatchSetLanguage(culture);
  }
}
```

```html
<!-- In your template -->
<select (change)="changeLanguage($event.target.value)">
  <option value="en">English</option>
  <option value="ar">العربية</option>
</select>
```

#### HTTP Header

The language can also be changed by sending the `Accept-Language` header:

```
Accept-Language: ar
```

Or using a cookie:
```
.AspNetCore.Culture=c=ar|uic=ar
```

### 5. Using Translations in Angular

```typescript
// In your component
import { LocalizationService } from '@abp/ng.core';

export class ProductComponent {
  constructor(private localization: LocalizationService) {}

  addToCart() {
    // Use localized text
    const successMessage = this.localization.instant('Cart::ItemAddedToCart');
    console.log(successMessage);
  }
}
```

```html
<!-- In your template -->
<button>{{ 'Product::AddToCart' | abpLocalization }}</button>
<h1>{{ 'Product::Products' | abpLocalization }}</h1>
```

---

## Configuration Details

### RTL Support

Arabic language has RTL support enabled in `/src/Masroof.Ecommerce.Domain.Shared/EcommerceDomainSharedModule.cs`:

```csharp
options.Languages.Add(new LanguageInfo(
    code: "ar",
    uiCultureName: "ar",
    displayName: "العربية",
    flagIcon: "famfamfam-flags sa")
{
    IsRightToLeft = true
});
```

### Request Localization Middleware

The localization middleware is already configured in `EcommerceHttpApiHostModule.cs`:

```csharp
app.UseAbpRequestLocalization();
```

This middleware:
- Detects the user's preferred language from request headers, cookies, or query strings
- Sets the current culture for the request
- Automatically applies RTL layout for Arabic

---

## Testing Exception Handling

### Test Scenarios

1. **Test 404 Not Found**:
   ```bash
   curl -X GET http://localhost:44361/api/products/00000000-0000-0000-0000-000000000000
   ```

2. **Test Validation Error**:
   ```bash
   curl -X POST http://localhost:44361/api/products \
     -H "Content-Type: application/json" \
     -d '{"name": ""}'
   ```

3. **Test with Arabic Language**:
   ```bash
   curl -X GET http://localhost:44361/api/products/invalid-id \
     -H "Accept-Language: ar"
   ```

### Expected Response

```json
{
  "statusCode": 404,
  "errorCode": "ENTITY_NOT_FOUND",
  "message": "المنتج غير موجود",
  "details": "Entity Type: Product",
  "timestamp": "2025-11-15T10:30:00.000Z"
}
```

---

## Key Translation Examples

### English
- Product not found
- Item added to cart successfully
- Your order has been placed successfully
- Payment successful
- Insufficient stock available

### Arabic
- المنتج غير موجود
- تمت إضافة العنصر إلى السلة بنجاح
- تم تقديم طلبك بنجاح
- تمت عملية الدفع بنجاح
- المخزون غير كافٍ

---

## Best Practices

1. **Always use localization keys** instead of hardcoded strings
2. **Use the L[] shorthand** in application services (inherited from ApplicationService)
3. **Use IStringLocalizer<EcommerceResource>** in other classes
4. **Throw appropriate exception types** based on the error context
5. **Don't expose sensitive information** in exception messages
6. **Test with both languages** to ensure translations are correct
7. **Use namespaced keys** (e.g., "Product:ProductName") for better organization

---

## Troubleshooting

### Translations Not Showing

1. Ensure the JSON files are marked as **Embedded Resource** in the .csproj file
2. Verify the culture code matches exactly (case-sensitive)
3. Check that the localization key exists in the JSON file
4. Restart the application after modifying localization files

### RTL Not Working

1. Ensure `IsRightToLeft = true` is set for Arabic in `EcommerceDomainSharedModule.cs`
2. Verify your frontend framework supports RTL
3. Check that the correct language is being set in the request

### Exception Filter Not Working

1. Ensure the filter is registered in `EcommerceHttpApiHostModule.cs`
2. Check that exceptions are not being caught elsewhere before reaching the filter
3. Verify the filter is added to MVC options correctly

---

## Additional Resources

- [ABP Localization Documentation](https://docs.abp.io/en/abp/latest/Localization)
- [ABP Exception Handling](https://docs.abp.io/en/abp/latest/Exception-Handling)
- [ASP.NET Core Globalization and Localization](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization)

---

**Implementation Date**: November 15, 2025
**Version**: 1.0
**Supported Languages**: English (en), Arabic (ar)
**Total Translations**: 200+ per language
