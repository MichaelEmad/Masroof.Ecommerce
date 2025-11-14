using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Products;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.ShoppingCarts;

[Authorize]
public class ShoppingCartAppService : ApplicationService, IShoppingCartAppService
{
    private readonly IRepository<ShoppingCart, Guid> _cartRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly ICurrentUser _currentUser;

    public ShoppingCartAppService(
        IRepository<ShoppingCart, Guid> cartRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Product, Guid> productRepository,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<ShoppingCartDto> GetMyCartAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    public async Task<ShoppingCartDto> AddItemAsync(AddToCartDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        var product = await _productRepository.GetAsync(input.ProductId);

        if (!product.IsActive)
        {
            throw new UserFriendlyException("This product is not available");
        }

        if (product.StockQuantity < input.Quantity)
        {
            throw new UserFriendlyException($"Only {product.StockQuantity} items available in stock");
        }

        cart.AddItem(
            product.Id,
            product.Name,
            product.GetEffectivePrice(),
            input.Quantity,
            product.ImageUrl
        );

        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    public async Task<ShoppingCartDto> UpdateItemQuantityAsync(Guid productId, UpdateCartItemDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        if (input.Quantity == 0)
        {
            cart.RemoveItem(productId);
        }
        else
        {
            var product = await _productRepository.GetAsync(productId);

            if (product.StockQuantity < input.Quantity)
            {
                throw new UserFriendlyException($"Only {product.StockQuantity} items available in stock");
            }

            cart.UpdateItemQuantity(productId, input.Quantity);
        }

        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    public async Task<ShoppingCartDto> RemoveItemAsync(Guid productId)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        cart.RemoveItem(productId);

        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    public async Task ClearCartAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        cart.Clear();

        await _cartRepository.UpdateAsync(cart, autoSave: true);
    }

    public async Task<ShoppingCartDto> ApplyCouponAsync(ApplyCouponDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        // TODO: Implement coupon validation
        // For now, apply a fixed 10% discount as example
        var discountAmount = cart.GetSubTotal() * 0.10m;

        cart.ApplyCoupon(input.CouponCode, discountAmount);

        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    public async Task<ShoppingCartDto> RemoveCouponAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var cart = await GetOrCreateCartAsync(customer.Id);

        cart.RemoveCoupon();

        await _cartRepository.UpdateAsync(cart, autoSave: true);

        return ObjectMapper.Map<ShoppingCart, ShoppingCartDto>(cart);
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = _currentUser.GetId();
        var customers = await _customerRepository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("Customer profile not found. Please create your profile first.");
        }

        return customer;
    }

    private async Task<ShoppingCart> GetOrCreateCartAsync(Guid customerId)
    {
        var carts = await _cartRepository.GetListAsync();
        var cart = carts.FirstOrDefault(c => c.CustomerId == customerId);

        if (cart == null)
        {
            cart = new ShoppingCart(GuidGenerator.Create(), customerId);
            cart = await _cartRepository.InsertAsync(cart, autoSave: true);
        }

        return cart;
    }
}
