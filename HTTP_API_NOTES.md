# HTTP API Configuration

## Auto-Generated Controllers

ABP Framework automatically generates HTTP controllers from application services that implement:
- `IApplicationService`
- `ICrudAppService<>`
- Custom application service interfaces

## Available Endpoints

All our application services will be automatically exposed as HTTP endpoints:

### Products API
- `GET /api/app/product` - List products
- `GET /api/app/product/{id}` - Get product by ID
- `POST /api/app/product` - Create product
- `PUT /api/app/product/{id}` - Update product
- `DELETE /api/app/product/{id}` - Delete product
- `GET /api/app/product/featured-products` - Get featured products
- `GET /api/app/product/products-by-category/{categoryId}` - Get products by category
- `GET /api/app/product/public-products` - Get public products
- `POST /api/app/product/increment-view-count/{id}` - Increment view count

### Categories API
- `GET /api/app/category` - List categories
- `GET /api/app/category/{id}` - Get category by ID
- `POST /api/app/category` - Create category
- `PUT /api/app/category/{id}` - Update category
- `DELETE /api/app/category/{id}` - Delete category
- `GET /api/app/category/root-categories` - Get root categories
- `GET /api/app/category/sub-categories/{parentId}` - Get subcategories
- `GET /api/app/category/active-categories` - Get active categories

### Customers API
- `GET /api/app/customer` - List customers
- `GET /api/app/customer/{id}` - Get customer by ID
- `POST /api/app/customer` - Create customer
- `PUT /api/app/customer/{id}` - Update customer
- `DELETE /api/app/customer/{id}` - Delete customer
- `GET /api/app/customer/my-profile` - Get my profile
- `PUT /api/app/customer/my-profile` - Update my profile
- `GET /api/app/customer/vip-customers` - Get VIP customers

### Addresses API
- `GET /api/app/address` - List addresses
- `GET /api/app/address/{id}` - Get address by ID
- `POST /api/app/address` - Create address
- `PUT /api/app/address/{id}` - Update address
- `DELETE /api/app/address/{id}` - Delete address
- `GET /api/app/address/my-addresses` - Get my addresses
- `GET /api/app/address/default-address/{customerId}` - Get default address
- `POST /api/app/address/set-as-default/{id}` - Set as default

### Shopping Cart API
- `GET /api/app/shopping-cart/my-cart` - Get my cart
- `POST /api/app/shopping-cart/add-item` - Add item to cart
- `PUT /api/app/shopping-cart/update-item-quantity/{productId}` - Update item quantity
- `DELETE /api/app/shopping-cart/remove-item/{productId}` - Remove item
- `DELETE /api/app/shopping-cart/clear-cart` - Clear cart
- `POST /api/app/shopping-cart/apply-coupon` - Apply coupon
- `DELETE /api/app/shopping-cart/remove-coupon` - Remove coupon

### Orders API
- `GET /api/app/order/{id}` - Get order by ID
- `GET /api/app/order` - List orders
- `POST /api/app/order` - Create order
- `PUT /api/app/order/update-status/{id}` - Update order status
- `GET /api/app/order/my-orders` - Get my orders
- `GET /api/app/order/my-order/{id}` - Get my order
- `POST /api/app/order/cancel/{id}` - Cancel order

### Payments API
- `GET /api/app/payment/{id}` - Get payment by ID
- `GET /api/app/payment` - List payments
- `POST /api/app/payment` - Create payment
- `POST /api/app/payment/process-payment/{id}` - Process payment
- `POST /api/app/payment/refund/{id}` - Refund payment
- `GET /api/app/payment/by-order-id/{orderId}` - Get payment by order ID

## Swagger UI

All endpoints are automatically documented and available in Swagger UI at:
`https://localhost:44300/swagger` (when running HttpApi.Host)

## Authentication

Most endpoints require authentication via Bearer token:
```
Authorization: Bearer {your-token-here}
```

Public endpoints (no authentication required):
- Product listing and details
- Category listing
- Featured products

## CORS Configuration

CORS is pre-configured in the HttpApi.Host project for Angular development server.
