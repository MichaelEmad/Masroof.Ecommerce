using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.ShoppingCarts;

public interface IShoppingCartAppService : IApplicationService
{
    Task<ShoppingCartDto> GetMyCartAsync();
    Task<ShoppingCartDto> AddItemAsync(AddToCartDto input);
    Task<ShoppingCartDto> UpdateItemQuantityAsync(Guid productId, UpdateCartItemDto input);
    Task<ShoppingCartDto> RemoveItemAsync(Guid productId);
    Task ClearCartAsync();
    Task<ShoppingCartDto> ApplyCouponAsync(ApplyCouponDto input);
    Task<ShoppingCartDto> RemoveCouponAsync();
}
