using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Addresses;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Permissions;
using Masroof.Ecommerce.Products;
using Masroof.Ecommerce.ShoppingCarts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Orders;

[Authorize]
public class OrderAppService : ApplicationService, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Address, Guid> _addressRepository;
    private readonly IRepository<ShoppingCart, Guid> _cartRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly ICurrentUser _currentUser;

    public OrderAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Address, Guid> addressRepository,
        IRepository<ShoppingCart, Guid> cartRepository,
        IRepository<Product, Guid> productRepository,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    [Authorize(EcommercePermissions.Orders.Default)]
    public async Task<OrderDto> GetAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    [Authorize(EcommercePermissions.Orders.Default)]
    public async Task<PagedResultDto<OrderDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var orders = await _orderRepository.GetListAsync();
        var totalCount = orders.Count;

        var orderedList = orders
            .OrderByDescending(o => o.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<OrderDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Order>, System.Collections.Generic.List<OrderDto>>(orderedList)
        );
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetCartAsync(customer.Id);

        if (cart.IsEmpty())
        {
            throw new UserFriendlyException("Cart is empty");
        }

        // Get addresses
        var shippingAddress = await _addressRepository.GetAsync(input.ShippingAddressId);
        var billingAddress = await _addressRepository.GetAsync(input.BillingAddressId);

        // Validate addresses belong to customer
        if (shippingAddress.CustomerId != customer.Id || billingAddress.CustomerId != customer.Id)
        {
            throw new UserFriendlyException("Invalid addresses");
        }

        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{GuidGenerator.Create().ToString("N").Substring(0, 6).ToUpper()}";

        // Create order
        var order = new Order(GuidGenerator.Create(), orderNumber, customer.Id);

        // Set addresses
        order.SetShippingAddress(
            shippingAddress.FullName,
            shippingAddress.AddressLine1,
            shippingAddress.City,
            shippingAddress.State,
            shippingAddress.PostalCode,
            shippingAddress.Country,
            shippingAddress.PhoneNumber,
            shippingAddress.AddressLine2
        );

        order.SetBillingAddress(
            billingAddress.FullName,
            billingAddress.AddressLine1,
            billingAddress.City,
            billingAddress.State,
            billingAddress.PostalCode,
            billingAddress.Country,
            billingAddress.PhoneNumber,
            billingAddress.AddressLine2
        );

        // Add items from cart
        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetAsync(cartItem.ProductId);

            // Check stock
            if (product.StockQuantity < cartItem.Quantity)
            {
                throw new UserFriendlyException($"Insufficient stock for {product.Name}");
            }

            order.AddItem(
                cartItem.ProductId,
                cartItem.ProductName,
                cartItem.Price,
                cartItem.Quantity,
                cartItem.ImageUrl
            );

            // Decrement stock
            product.DecrementStock(cartItem.Quantity);
            product.IncrementSoldCount(cartItem.Quantity);
            await _productRepository.UpdateAsync(product, autoSave: false);
        }

        // Apply coupon if present
        if (!string.IsNullOrEmpty(cart.CouponCode))
        {
            order.ApplyCoupon(cart.CouponCode, cart.DiscountAmount);
        }

        // Set shipping cost (flat rate for now)
        order.SetShippingCost(10m);

        // Set tax (10% for now)
        order.SetTax(order.SubTotal * 0.10m);

        // Set customer notes
        if (!string.IsNullOrEmpty(input.CustomerNotes))
        {
            order.CustomerNotes = input.CustomerNotes;
        }

        // Save order
        await _orderRepository.InsertAsync(order, autoSave: true);

        // Update customer stats
        customer.IncrementOrderStats(order.TotalAmount);
        await _customerRepository.UpdateAsync(customer, autoSave: true);

        // Clear cart
        cart.Clear();
        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    [Authorize(EcommercePermissions.Orders.UpdateStatus)]
    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto input)
    {
        var order = await _orderRepository.GetAsync(id);

        switch (input.Status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Processing:
                order.Process();
                break;
            case OrderStatus.Shipped:
                if (string.IsNullOrEmpty(input.TrackingNumber) || string.IsNullOrEmpty(input.ShippingCarrier))
                {
                    throw new UserFriendlyException("Tracking number and shipping carrier are required for shipped status");
                }
                order.Ship(input.TrackingNumber, input.ShippingCarrier);
                break;
            case OrderStatus.Delivered:
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        if (!string.IsNullOrEmpty(input.AdminNotes))
        {
            order.AdminNotes = input.AdminNotes;
        }

        await _orderRepository.UpdateAsync(order, autoSave: true);

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<ListResultDto<OrderDto>> GetMyOrdersAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var orders = await _orderRepository.GetListAsync();
        var myOrders = orders
            .Where(o => o.CustomerId == customer.Id)
            .OrderByDescending(o => o.CreationTime)
            .ToList();

        return new ListResultDto<OrderDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Order>, System.Collections.Generic.List<OrderDto>>(myOrders)
        );
    }

    public async Task<OrderDto> GetMyOrderAsync(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        var order = await _orderRepository.GetAsync(id);

        if (order.CustomerId != customer.Id)
        {
            throw new UserFriendlyException("Order not found");
        }

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task CancelAsync(Guid id)
    {
        var customer = await GetCurrentCustomerAsync();
        var order = await _orderRepository.GetAsync(id);

        if (order.CustomerId != customer.Id && !_currentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("You don't have permission to cancel this order");
        }

        if (!order.CanBeCancelled())
        {
            throw new UserFriendlyException("This order cannot be cancelled");
        }

        order.Cancel();
        await _orderRepository.UpdateAsync(order, autoSave: true);
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = _currentUser.GetId();
        var customers = await _customerRepository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("Customer profile not found");
        }

        return customer;
    }

    private async Task<ShoppingCart> GetCartAsync(Guid customerId)
    {
        var carts = await _cartRepository.GetListAsync();
        var cart = carts.FirstOrDefault(c => c.CustomerId == customerId);

        if (cart == null)
        {
            throw new UserFriendlyException("Cart not found");
        }

        return cart;
    }
}
