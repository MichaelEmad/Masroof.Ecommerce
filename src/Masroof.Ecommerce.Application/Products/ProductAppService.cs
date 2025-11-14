using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Products;

[Authorize(EcommercePermissions.Products.Default)]
public class ProductAppService : CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>, IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository) : base(repository)
    {
        GetPolicyName = EcommercePermissions.Products.Default;
        GetListPolicyName = EcommercePermissions.Products.Default;
        CreatePolicyName = EcommercePermissions.Products.Create;
        UpdatePolicyName = EcommercePermissions.Products.Edit;
        DeletePolicyName = EcommercePermissions.Products.Delete;
    }

    [AllowAnonymous]
    public async Task<ListResultDto<ProductDto>> GetFeaturedProductsAsync()
    {
        var products = await Repository.GetListAsync();
        var featuredProducts = products
            .Where(p => p.IsFeatured && p.IsActive && p.IsInStock())
            .OrderByDescending(p => p.CreationTime)
            .Take(10)
            .ToList();

        return new ListResultDto<ProductDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Product>, System.Collections.Generic.List<ProductDto>>(featuredProducts)
        );
    }

    [AllowAnonymous]
    public async Task<ListResultDto<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
    {
        var products = await Repository.GetListAsync();
        var categoryProducts = products
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToList();

        return new ListResultDto<ProductDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Product>, System.Collections.Generic.List<ProductDto>>(categoryProducts)
        );
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<ProductDto>> GetPublicProductsAsync(PagedAndSortedResultRequestDto input)
    {
        var products = await Repository.GetListAsync();
        var activeProducts = products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        var totalCount = products.Count(p => p.IsActive);

        return new PagedResultDto<ProductDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Product>, System.Collections.Generic.List<ProductDto>>(activeProducts)
        );
    }

    [AllowAnonymous]
    public async Task IncrementViewCountAsync(Guid id)
    {
        var product = await Repository.GetAsync(id);
        product.IncrementViewCount();
        await Repository.UpdateAsync(product);
    }

    protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await Repository.GetQueryableAsync())
            .OrderByDescending(p => p.CreationTime);
    }
}
