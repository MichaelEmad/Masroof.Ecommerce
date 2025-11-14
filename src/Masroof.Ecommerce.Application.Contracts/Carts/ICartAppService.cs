using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Carts;

public interface ICartAppService : IApplicationService
{
    Task<List<CartItemDto>> GetMyCartAsync();
    Task<CartItemDto> AddToCartAsync(AddToCartDto input);
    Task<CartItemDto> UpdateCartItemAsync(Guid id, UpdateCartItemDto input);
    Task RemoveFromCartAsync(Guid id);
    Task ClearCartAsync();
    Task<decimal> GetCartTotalAsync();
}
