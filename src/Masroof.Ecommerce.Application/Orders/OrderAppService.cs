using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Catalog;
using Masroof.Ecommerce.Carts;
using Masroof.Ecommerce.Permissions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Orders;

public class OrderAppService : ApplicationService, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<CartItem, Guid> _cartItemRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBackgroundJobManager _backgroundJobManager;

    public OrderAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<CartItem, Guid> cartItemRepository,
        IRepository<IdentityUser, Guid> userRepository,
        ICurrentUser currentUser,
        IBackgroundJobManager backgroundJobManager)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cartItemRepository = cartItemRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto input)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.Default);

        var customerId = _currentUser.GetId();
        var user = await _userRepository.GetAsync(customerId);

        // Get cart items or use provided items
        List<CartItem> cartItems;
        if (input.OrderItems == null || !input.OrderItems.Any())
        {
            cartItems = await _cartItemRepository
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new UserFriendlyException("Cart is empty");
            }
        }
        else
        {
            cartItems = new List<CartItem>();
            foreach (var item in input.OrderItems)
            {
                var product = await _productRepository.GetAsync(item.ProductId);
                cartItems.Add(new CartItem(
                    Guid.NewGuid(),
                    customerId,
                    product.Id,
                    product.Name,
                    product.Price,
                    item.Quantity,
                    product.ImageUrl
                ));
            }
        }

        // Validate stock and prepare order
        var productIds = cartItems.Select(x => x.ProductId).ToList();
        var products = await _productRepository
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync();

        var order = new Order(
            GuidGenerator.Create(),
            customerId,
            user.Name ?? user.UserName!,
            user.Email!
        );

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(x => x.Id == cartItem.ProductId);
            if (product == null)
            {
                throw new UserFriendlyException($"Product {cartItem.ProductName} not found");
            }

            if (!product.IsActive)
            {
                throw new UserFriendlyException($"Product {product.Name} is not available");
            }

            if (product.Stock < cartItem.Quantity)
            {
                throw new UserFriendlyException(
                    $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {cartItem.Quantity}");
            }

            // Deduct stock
            product.ReduceStock(cartItem.Quantity);
            await _productRepository.UpdateAsync(product, true);

            // Add order item
            order.AddItem(
                product.Id,
                product.Name,
                product.Price,
                cartItem.Quantity
            );
        }

        await _orderRepository.InsertAsync(order, true);

        // Clear cart
        if (input.OrderItems == null || !input.OrderItems.Any())
        {
            await _cartItemRepository.DeleteManyAsync(cartItems, true);
        }

        // Queue email notification
        await _backgroundJobManager.EnqueueAsync(
            new OrderEmailNotificationArgs
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount
            }
        );

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<OrderDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.Default);

        var customerId = _currentUser.GetId();
        var hasManageAllPermission = await AuthorizationService.IsGrantedAsync(EcommercePermissions.Orders.ManageAll);

        var order = await _orderRepository
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            throw new UserFriendlyException("Order not found");
        }

        if (!hasManageAllPermission && order.CustomerId != customerId)
        {
            throw new UserFriendlyException("You can only view your own orders");
        }

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.ManageAll);

        var query = await _orderRepository.WithDetailsAsync(x => x.OrderItems);

        query = query
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                .PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<OrderDto>(
            totalCount,
            ObjectMapper.Map<List<Order>, List<OrderDto>>(orders)
        );
    }

    public async Task<PagedResultDto<OrderDto>> GetMyOrdersAsync(GetOrderListDto input)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.Default);

        var customerId = _currentUser.GetId();

        var query = await _orderRepository.WithDetailsAsync(x => x.OrderItems);

        query = query
            .Where(x => x.CustomerId == customerId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var orders = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                .PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<OrderDto>(
            totalCount,
            ObjectMapper.Map<List<Order>, List<OrderDto>>(orders)
        );
    }

    public async Task<OrderDto> MarkAsPaidAsync(Guid id)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.ManageAll);

        var order = await _orderRepository
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            throw new UserFriendlyException("Order not found");
        }

        order.MarkAsPaid();
        await _orderRepository.UpdateAsync(order, true);

        // Queue email notification for payment confirmation
        await _backgroundJobManager.EnqueueAsync(
            new OrderPaymentConfirmationArgs
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                CustomerName = order.CustomerName
            }
        );

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id)
    {
        await CheckPolicyAsync(EcommercePermissions.Orders.Default);

        var customerId = _currentUser.GetId();
        var hasManageAllPermission = await AuthorizationService.IsGrantedAsync(EcommercePermissions.Orders.ManageAll);

        var order = await _orderRepository
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            throw new UserFriendlyException("Order not found");
        }

        if (!hasManageAllPermission && order.CustomerId != customerId)
        {
            throw new UserFriendlyException("You can only cancel your own orders");
        }

        order.MarkAsCancelled();

        // Restore stock
        foreach (var orderItem in order.OrderItems)
        {
            var product = await _productRepository.GetAsync(orderItem.ProductId);
            product.IncreaseStock(orderItem.Quantity);
            await _productRepository.UpdateAsync(product, true);
        }

        await _orderRepository.UpdateAsync(order, true);

        return ObjectMapper.Map<Order, OrderDto>(order);
    }
}
