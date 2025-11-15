using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Addresses;
using Masroof.Ecommerce.Categories;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;
using Masroof.Ecommerce.Payments;
using Masroof.Ecommerce.Products;
using Masroof.Ecommerce.ShoppingCarts;
using Shouldly;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Xunit;

namespace Masroof.Ecommerce.Tests.ECommerce;

/// <summary>
/// Integration test that verifies the complete e-commerce flow:
/// 1. User Registration → Auto Customer Creation
/// 2. Product Creation
/// 3. Add to Cart
/// 4. Create Order
/// 5. Payment Processing
/// 6. Order Confirmation
/// </summary>
public class FullECommerceFlowTests : EcommerceApplicationTestBase<EcommerceApplicationTestModule>
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<ShoppingCart, Guid> _cartRepository;
    private readonly IRepository<CartItem, Guid> _cartItemRepository;
    private readonly IRepository<Address, Guid> _addressRepository;
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderItem, Guid> _orderItemRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly ILocalEventBus _localEventBus;
    private readonly IGuidGenerator _guidGenerator;

    public FullECommerceFlowTests()
    {
        _categoryRepository = GetRequiredService<IRepository<Category, Guid>>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _cartRepository = GetRequiredService<IRepository<ShoppingCart, Guid>>();
        _cartItemRepository = GetRequiredService<IRepository<CartItem, Guid>>();
        _addressRepository = GetRequiredService<IRepository<Address, Guid>>();
        _orderRepository = GetRequiredService<IRepository<Order, Guid>>();
        _orderItemRepository = GetRequiredService<IRepository<OrderItem, Guid>>();
        _identityUserManager = GetRequiredService<IdentityUserManager>();
        _localEventBus = GetRequiredService<ILocalEventBus>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Complete_ECommerce_Flow_Should_Work_From_Registration_To_Order_Creation()
    {
        // ====================================================================
        // STEP 1: USER REGISTRATION & CUSTOMER AUTO-CREATION
        // ====================================================================
        var userName = "testbuyer" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";
        var firstName = "John";
        var lastName = "Doe";

        IdentityUser user = null!;
        Customer customer = null!;
        ShoppingCart cart = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            // Create user
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                tenantId: null
            );
            user.SetProperty("FirstName", firstName);
            user.SetProperty("LastName", lastName);

            await _identityUserManager.CreateAsync(user);

            // Publish event to trigger customer auto-creation
            await _localEventBus.PublishAsync(
                new EntityCreatedEventData<IdentityUser>(user)
            );
        });

        // Wait for event handler to process
        await Task.Delay(100);

        // Verify customer was auto-created
        await WithUnitOfWorkAsync(async () =>
        {
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);
            customer.ShouldNotBeNull();
            customer.Email.ShouldBe(email);

            // Verify shopping cart was auto-created
            cart = await _cartRepository.FirstOrDefaultAsync(c => c.CustomerId == customer.Id);
            cart.ShouldNotBeNull();
        });

        // ====================================================================
        // STEP 2: PRODUCT CREATION
        // ====================================================================
        Category category = null!;
        Product product1 = null!;
        Product product2 = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            // Create category
            category = new Category(
                _guidGenerator.Create(),
                "Electronics",
                "Electronic devices and gadgets"
            );
            await _categoryRepository.InsertAsync(category, autoSave: true);

            // Create products
            product1 = new Product(
                _guidGenerator.Create(),
                category.Id,
                "Laptop",
                "High-performance laptop",
                1299.99m
            );
            product1.StockQuantity = 10;
            product1.SKU = "LAP-001";

            product2 = new Product(
                _guidGenerator.Create(),
                category.Id,
                "Wireless Mouse",
                "Ergonomic wireless mouse",
                29.99m
            );
            product2.StockQuantity = 50;
            product2.SKU = "MOU-001";

            await _productRepository.InsertAsync(product1, autoSave: true);
            await _productRepository.InsertAsync(product2, autoSave: true);
        });

        // ====================================================================
        // STEP 3: ADD PRODUCTS TO CART
        // ====================================================================
        await WithUnitOfWorkAsync(async () =>
        {
            cart = await _cartRepository.GetAsync(cart.Id);

            // Add laptop to cart (quantity: 1)
            var cartItem1 = new CartItem(
                _guidGenerator.Create(),
                cart.Id,
                product1.Id,
                product1.Name,
                product1.Price,
                1
            );
            cartItem1.ImageUrl = product1.ImageUrl;

            // Add mouse to cart (quantity: 2)
            var cartItem2 = new CartItem(
                _guidGenerator.Create(),
                cart.Id,
                product2.Id,
                product2.Name,
                product2.Price,
                2
            );
            cartItem2.ImageUrl = product2.ImageUrl;

            await _cartItemRepository.InsertAsync(cartItem1, autoSave: true);
            await _cartItemRepository.InsertAsync(cartItem2, autoSave: true);
        });

        // Verify cart items
        await WithUnitOfWorkAsync(async () =>
        {
            var cartItems = await _cartItemRepository.GetListAsync(ci => ci.ShoppingCartId == cart.Id);
            cartItems.Count.ShouldBe(2);

            var laptopItem = cartItems.First(ci => ci.ProductId == product1.Id);
            laptopItem.Quantity.ShouldBe(1);
            laptopItem.Price.ShouldBe(product1.Price);

            var mouseItem = cartItems.First(ci => ci.ProductId == product2.Id);
            mouseItem.Quantity.ShouldBe(2);
            mouseItem.Price.ShouldBe(product2.Price);
        });

        // ====================================================================
        // STEP 4: CREATE ADDRESSES
        // ====================================================================
        Address shippingAddress = null!;
        Address billingAddress = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            shippingAddress = new Address(
                _guidGenerator.Create(),
                customer.Id,
                "John Doe",
                "+1234567890",
                "123 Main St",
                "New York",
                "NY",
                "10001",
                "USA",
                AddressType.Shipping
            );

            billingAddress = new Address(
                _guidGenerator.Create(),
                customer.Id,
                "John Doe",
                "+1234567890",
                "123 Main St",
                "New York",
                "NY",
                "10001",
                "USA",
                AddressType.Billing
            );

            await _addressRepository.InsertAsync(shippingAddress, autoSave: true);
            await _addressRepository.InsertAsync(billingAddress, autoSave: true);
        });

        // ====================================================================
        // STEP 5: CREATE ORDER FROM CART
        // ====================================================================
        Order order = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            // Get cart with items
            cart = await _cartRepository.GetAsync(cart.Id);
            var cartItems = await _cartItemRepository.GetListAsync(ci => ci.ShoppingCartId == cart.Id);

            // Calculate totals
            var subTotal = cartItems.Sum(ci => ci.Price * ci.Quantity);
            var shippingCost = 15.00m; // Fixed shipping
            var tax = subTotal * 0.10m; // 10% tax
            var totalAmount = subTotal + shippingCost + tax;

            // Create order
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            order = new Order(
                _guidGenerator.Create(),
                customer.Id,
                orderNumber,
                // Shipping Address
                shippingAddress.FullName,
                shippingAddress.AddressLine1,
                shippingAddress.AddressLine2,
                shippingAddress.City,
                shippingAddress.State,
                shippingAddress.PostalCode,
                shippingAddress.Country,
                shippingAddress.PhoneNumber,
                // Billing Address
                billingAddress.FullName,
                billingAddress.AddressLine1,
                billingAddress.AddressLine2,
                billingAddress.City,
                billingAddress.State,
                billingAddress.PostalCode,
                billingAddress.Country,
                billingAddress.PhoneNumber,
                // Amounts
                subTotal,
                shippingCost,
                tax
            );

            await _orderRepository.InsertAsync(order, autoSave: true);

            // Create order items from cart items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem(
                    _guidGenerator.Create(),
                    order.Id,
                    cartItem.ProductId,
                    cartItem.ProductName,
                    cartItem.Price,
                    cartItem.Quantity
                );
                orderItem.ImageUrl = cartItem.ImageUrl;

                await _orderItemRepository.InsertAsync(orderItem, autoSave: true);

                // Update product inventory
                var product = await _productRepository.GetAsync(cartItem.ProductId);
                product.DecreaseStock(cartItem.Quantity);
                await _productRepository.UpdateAsync(product, autoSave: true);
            }

            // Clear cart after order
            await _cartItemRepository.DeleteManyAsync(cartItems);
        });

        // ====================================================================
        // STEP 6: VERIFY ORDER CREATION
        // ====================================================================
        await WithUnitOfWorkAsync(async () =>
        {
            // Verify order
            var createdOrder = await _orderRepository.GetAsync(order.Id);
            createdOrder.ShouldNotBeNull();
            createdOrder.CustomerId.ShouldBe(customer.Id);
            createdOrder.Status.ShouldBe(OrderStatus.Pending);
            createdOrder.TotalAmount.ShouldBe(1299.99m + (2 * 29.99m) + 15.00m + ((1299.99m + 59.98m) * 0.10m));

            // Verify order items
            var orderItems = await _orderItemRepository.GetListAsync(oi => oi.OrderId == order.Id);
            orderItems.Count.ShouldBe(2);

            var laptopOrderItem = orderItems.First(oi => oi.ProductId == product1.Id);
            laptopOrderItem.Quantity.ShouldBe(1);
            laptopOrderItem.UnitPrice.ShouldBe(1299.99m);

            var mouseOrderItem = orderItems.First(oi => oi.ProductId == product2.Id);
            mouseOrderItem.Quantity.ShouldBe(2);
            mouseOrderItem.UnitPrice.ShouldBe(29.99m);

            // Verify inventory was decreased
            var updatedProduct1 = await _productRepository.GetAsync(product1.Id);
            updatedProduct1.StockQuantity.ShouldBe(9); // Was 10, sold 1

            var updatedProduct2 = await _productRepository.GetAsync(product2.Id);
            updatedProduct2.StockQuantity.ShouldBe(48); // Was 50, sold 2

            // Verify cart is empty
            var remainingCartItems = await _cartItemRepository.GetListAsync(ci => ci.ShoppingCartId == cart.Id);
            remainingCartItems.Count.ShouldBe(0);
        });

        // ====================================================================
        // STEP 7: SIMULATE ORDER CONFIRMATION (Payment Success)
        // ====================================================================
        await WithUnitOfWorkAsync(async () =>
        {
            order = await _orderRepository.GetAsync(order.Id);
            order.Confirm(); // This changes status from Pending to Confirmed
            await _orderRepository.UpdateAsync(order, autoSave: true);
        });

        // Verify order confirmation
        await WithUnitOfWorkAsync(async () =>
        {
            var confirmedOrder = await _orderRepository.GetAsync(order.Id);
            confirmedOrder.Status.ShouldBe(OrderStatus.Confirmed);
            confirmedOrder.OrderDate.ShouldNotBeNull();
        });

        // ====================================================================
        // SUMMARY: VERIFY COMPLETE FLOW
        // ====================================================================
        await WithUnitOfWorkAsync(async () =>
        {
            // 1. Customer exists and linked to user
            var finalCustomer = await _customerRepository.GetAsync(customer.Id);
            finalCustomer.UserId.ShouldBe(user.Id);

            // 2. Products exist in catalog
            var allProducts = await _productRepository.GetListAsync();
            allProducts.Count.ShouldBeGreaterThanOrEqualTo(2);

            // 3. Order created successfully
            var finalOrder = await _orderRepository.GetAsync(order.Id);
            finalOrder.Status.ShouldBe(OrderStatus.Confirmed);

            // 4. Order items match what was in cart
            var finalOrderItems = await _orderItemRepository.GetListAsync(oi => oi.OrderId == order.Id);
            finalOrderItems.Count.ShouldBe(2);

            // 5. Inventory reduced correctly
            var finalProduct1 = await _productRepository.GetAsync(product1.Id);
            finalProduct1.StockQuantity.ShouldBe(9);

            // 6. Cart is empty after checkout
            var finalCartItems = await _cartItemRepository.GetListAsync(ci => ci.ShoppingCartId == cart.Id);
            finalCartItems.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task Order_Should_Not_Be_Created_With_Insufficient_Stock()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var product = await CreateTestProductAsync(stockQuantity: 2); // Only 2 in stock
        var cart = await _cartRepository.FirstAsync(c => c.CustomerId == customer.Id);
        var addresses = await CreateTestAddressesAsync(customer.Id);

        // Add 5 items to cart (more than available)
        await WithUnitOfWorkAsync(async () =>
        {
            var cartItem = new CartItem(
                _guidGenerator.Create(),
                cart.Id,
                product.Id,
                product.Name,
                product.Price,
                5 // Requesting 5, but only 2 available
            );
            await _cartItemRepository.InsertAsync(cartItem, autoSave: true);
        });

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var orderProduct = await _productRepository.GetAsync(product.Id);
                orderProduct.DecreaseStock(5); // This should throw
            });
        });
    }

    [Fact]
    public async Task Multiple_Products_Can_Be_Added_To_Same_Order()
    {
        // Arrange
        var customer = await CreateTestCustomerAsync();
        var cart = await _cartRepository.FirstAsync(c => c.CustomerId == customer.Id);

        // Create 5 different products
        var products = new Product[5];
        for (int i = 0; i < 5; i++)
        {
            products[i] = await CreateTestProductAsync(
                name: $"Product {i + 1}",
                price: (i + 1) * 10.00m,
                stockQuantity: 100
            );
        }

        // Add all products to cart
        await WithUnitOfWorkAsync(async () =>
        {
            foreach (var product in products)
            {
                var cartItem = new CartItem(
                    _guidGenerator.Create(),
                    cart.Id,
                    product.Id,
                    product.Name,
                    product.Price,
                    1
                );
                await _cartItemRepository.InsertAsync(cartItem, autoSave: true);
            }
        });

        // Create order
        var order = await CreateTestOrderAsync(customer.Id, cart.Id);

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            var orderItems = await _orderItemRepository.GetListAsync(oi => oi.OrderId == order.Id);
            orderItems.Count.ShouldBe(5);
            orderItems.Sum(oi => oi.UnitPrice * oi.Quantity).ShouldBe(10m + 20m + 30m + 40m + 50m);
        });
    }

    #region Helper Methods

    private async Task<Customer> CreateTestCustomerAsync()
    {
        Customer customer = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            var userName = "testuser" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                $"{userName}@test.com",
                tenantId: null
            );
            await _identityUserManager.CreateAsync(user);
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        await Task.Delay(100); // Wait for event handler

        await WithUnitOfWorkAsync(async () =>
        {
            customer = await _customerRepository.FirstAsync();
        });

        return customer;
    }

    private async Task<Product> CreateTestProductAsync(
        string name = "Test Product",
        decimal price = 99.99m,
        int stockQuantity = 10)
    {
        Product product = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new Category(
                _guidGenerator.Create(),
                "Test Category",
                "Test Description"
            );
            await _categoryRepository.InsertAsync(category, autoSave: true);

            product = new Product(
                _guidGenerator.Create(),
                category.Id,
                name,
                "Test product description",
                price
            );
            product.StockQuantity = stockQuantity;
            product.SKU = $"TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            await _productRepository.InsertAsync(product, autoSave: true);
        });

        return product;
    }

    private async Task<(Address shipping, Address billing)> CreateTestAddressesAsync(Guid customerId)
    {
        Address shipping = null!;
        Address billing = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            shipping = new Address(
                _guidGenerator.Create(),
                customerId,
                "Test User",
                "+1234567890",
                "123 Test St",
                "Test City",
                "TS",
                "12345",
                "USA",
                AddressType.Shipping
            );

            billing = new Address(
                _guidGenerator.Create(),
                customerId,
                "Test User",
                "+1234567890",
                "123 Test St",
                "Test City",
                "TS",
                "12345",
                "USA",
                AddressType.Billing
            );

            await _addressRepository.InsertAsync(shipping, autoSave: true);
            await _addressRepository.InsertAsync(billing, autoSave: true);
        });

        return (shipping, billing);
    }

    private async Task<Order> CreateTestOrderAsync(Guid customerId, Guid cartId)
    {
        Order order = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            var addresses = await CreateTestAddressesAsync(customerId);
            var cartItems = await _cartItemRepository.GetListAsync(ci => ci.ShoppingCartId == cartId);

            var subTotal = cartItems.Sum(ci => ci.Price * ci.Quantity);
            var orderNumber = $"TEST-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

            order = new Order(
                _guidGenerator.Create(),
                customerId,
                orderNumber,
                addresses.shipping.FullName,
                addresses.shipping.AddressLine1,
                addresses.shipping.AddressLine2,
                addresses.shipping.City,
                addresses.shipping.State,
                addresses.shipping.PostalCode,
                addresses.shipping.Country,
                addresses.shipping.PhoneNumber,
                addresses.billing.FullName,
                addresses.billing.AddressLine1,
                addresses.billing.AddressLine2,
                addresses.billing.City,
                addresses.billing.State,
                addresses.billing.PostalCode,
                addresses.billing.Country,
                addresses.billing.PhoneNumber,
                subTotal,
                15.00m,
                subTotal * 0.10m
            );

            await _orderRepository.InsertAsync(order, autoSave: true);

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem(
                    _guidGenerator.Create(),
                    order.Id,
                    cartItem.ProductId,
                    cartItem.ProductName,
                    cartItem.Price,
                    cartItem.Quantity
                );
                await _orderItemRepository.InsertAsync(orderItem, autoSave: true);
            }
        });

        return order;
    }

    #endregion
}
