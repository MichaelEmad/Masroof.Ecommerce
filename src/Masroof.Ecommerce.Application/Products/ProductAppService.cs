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
    public async Task<PagedResultDto<ProductDto>> GetFilteredProductsAsync(ProductFilterDto input)
    {
        var products = await Repository.GetListAsync();
        var query = products.Where(p => p.IsActive);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            var searchTerm = input.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(searchTerm))
            );
        }

        // Apply category filter
        if (input.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == input.CategoryId.Value);
        }

        // Apply price range filter
        if (input.MinPrice.HasValue)
        {
            query = query.Where(p => p.GetEffectivePrice() >= input.MinPrice.Value);
        }
        if (input.MaxPrice.HasValue)
        {
            query = query.Where(p => p.GetEffectivePrice() <= input.MaxPrice.Value);
        }

        // Apply stock filter
        if (input.InStockOnly == true)
        {
            query = query.Where(p => p.IsInStock());
        }

        // Apply sorting
        query = input.SortBy?.ToLower() switch
        {
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.GetEffectivePrice()),
            "price_desc" => query.OrderByDescending(p => p.GetEffectivePrice()),
            "newest" => query.OrderByDescending(p => p.CreationTime),
            "popular" => query.OrderByDescending(p => p.SoldCount).ThenByDescending(p => p.ViewCount),
            _ => query.OrderByDescending(p => p.CreationTime)
        };

        var totalCount = query.Count();

        var pagedProducts = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<ProductDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Product>, System.Collections.Generic.List<ProductDto>>(pagedProducts)
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
