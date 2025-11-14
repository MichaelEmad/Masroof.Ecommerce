using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Catalog;
using Masroof.Ecommerce.Permissions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Carts;

public class CartAppService : ApplicationService, ICartAppService
{
    private readonly IRepository<CartItem, Guid> _cartItemRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly ICurrentUser _currentUser;

    public CartAppService(
        IRepository<CartItem, Guid> cartItemRepository,
        IRepository<Product, Guid> productRepository,
        ICurrentUser currentUser)
    {
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
    }

    public async Task<List<CartItemDto>> GetMyCartAsync()
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();
        var cartItems = await _cartItemRepository
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();

        return ObjectMapper.Map<List<CartItem>, List<CartItemDto>>(cartItems);
    }

    public async Task<CartItemDto> AddToCartAsync(AddToCartDto input)
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();

        var product = await _productRepository.GetAsync(input.ProductId);

        if (!product.IsActive)
        {
            throw new UserFriendlyException("Product is not available");
        }

        if (product.Stock < input.Quantity)
        {
            throw new UserFriendlyException($"Insufficient stock. Available: {product.Stock}");
        }

        // Check if product already in cart
        var existingCartItem = await _cartItemRepository
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.ProductId == input.ProductId);

        if (existingCartItem != null)
        {
            existingCartItem.UpdateQuantity(existingCartItem.Quantity + input.Quantity);
            await _cartItemRepository.UpdateAsync(existingCartItem, true);
            return ObjectMapper.Map<CartItem, CartItemDto>(existingCartItem);
        }

        var cartItem = new CartItem(
            GuidGenerator.Create(),
            customerId,
            product.Id,
            product.Name,
            product.Price,
            input.Quantity,
            product.ImageUrl
        );

        await _cartItemRepository.InsertAsync(cartItem, true);

        return ObjectMapper.Map<CartItem, CartItemDto>(cartItem);
    }

    public async Task<CartItemDto> UpdateCartItemAsync(Guid id, UpdateCartItemDto input)
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();
        var cartItem = await _cartItemRepository.GetAsync(id);

        if (cartItem.CustomerId != customerId)
        {
            throw new UserFriendlyException("You can only update your own cart items");
        }

        var product = await _productRepository.GetAsync(cartItem.ProductId);

        if (product.Stock < input.Quantity)
        {
            throw new UserFriendlyException($"Insufficient stock. Available: {product.Stock}");
        }

        cartItem.UpdateQuantity(input.Quantity);
        await _cartItemRepository.UpdateAsync(cartItem, true);

        return ObjectMapper.Map<CartItem, CartItemDto>(cartItem);
    }

    public async Task RemoveFromCartAsync(Guid id)
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();
        var cartItem = await _cartItemRepository.GetAsync(id);

        if (cartItem.CustomerId != customerId)
        {
            throw new UserFriendlyException("You can only remove your own cart items");
        }

        await _cartItemRepository.DeleteAsync(id);
    }

    public async Task ClearCartAsync()
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();
        var cartItems = await _cartItemRepository
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();

        await _cartItemRepository.DeleteManyAsync(cartItems);
    }

    public async Task<decimal> GetCartTotalAsync()
    {
        await CheckPolicyAsync(EcommercePermissions.Cart.Default);

        var customerId = _currentUser.GetId();
        var cartItems = await _cartItemRepository
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();

        return cartItems.Sum(x => x.GetTotalPrice());
    }
}
