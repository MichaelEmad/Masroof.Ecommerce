using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Products;

public interface IProductAppService : ICrudAppService<ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>
{
    Task<ListResultDto<ProductDto>> GetFeaturedProductsAsync();
    Task<ListResultDto<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
    Task<PagedResultDto<ProductDto>> GetPublicProductsAsync(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<ProductDto>> GetFilteredProductsAsync(ProductFilterDto input);
    Task IncrementViewCountAsync(Guid id);
}
