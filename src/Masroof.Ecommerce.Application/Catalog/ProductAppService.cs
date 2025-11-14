using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Images;
using Masroof.Ecommerce.Permissions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Catalog;

public class ProductAppService :
    CrudAppService<
        Product,
        ProductDto,
        Guid,
        GetProductListDto,
        CreateUpdateProductDto,
        CreateUpdateProductDto>,
    IProductAppService
{
    private readonly IRepository<ProductAllergen> _productAllergenRepository;
    private readonly IImageUploadService _imageUploadService;

    public ProductAppService(
        IRepository<Product, Guid> repository,
        IRepository<ProductAllergen> productAllergenRepository,
        IImageUploadService imageUploadService)
        : base(repository)
    {
        _productAllergenRepository = productAllergenRepository;
        _imageUploadService = imageUploadService;

        GetPolicyName = EcommercePermissions.Products.Default;
        GetListPolicyName = EcommercePermissions.Products.Default;
        CreatePolicyName = EcommercePermissions.Products.Create;
        UpdatePolicyName = EcommercePermissions.Products.Edit;
        DeletePolicyName = EcommercePermissions.Products.Delete;
    }

    protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(GetProductListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .Include(x => x.Category)
            .Include(x => x.ProductAllergens)
                .ThenInclude(x => x.Allergen)
            .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!) || (x.Description != null && x.Description.Contains(input.Filter!)));
    }

    public async Task<PagedResultDto<ProductDto>> GetPublicListAsync(GetProductListDto input)
    {
        // Force IsActive to true for public list
        input.IsActive = true;

        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);

        var products = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Name)
                .PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<ProductDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Product>, System.Collections.Generic.List<ProductDto>>(products)
        );
    }

    public override async Task<ProductDto> CreateAsync(CreateUpdateProductDto input)
    {
        var product = new Product(
            GuidGenerator.Create(),
            input.Name,
            input.Price,
            input.Stock,
            input.CategoryId,
            input.Description,
            input.ImageUrl
        )
        {
            IsActive = input.IsActive
        };

        await Repository.InsertAsync(product, true);

        // Add allergens
        foreach (var allergenId in input.AllergenIds)
        {
            await _productAllergenRepository.InsertAsync(
                new ProductAllergen(product.Id, allergenId),
                true
            );
        }

        return await GetAsync(product.Id);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, CreateUpdateProductDto input)
    {
        var product = await Repository.GetAsync(id);

        // Delete old image if it's being replaced
        if (!string.IsNullOrWhiteSpace(product.ImageUrl) &&
            product.ImageUrl != input.ImageUrl &&
            !string.IsNullOrWhiteSpace(input.ImageUrl))
        {
            await _imageUploadService.DeleteImageAsync(product.ImageUrl);
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Price = input.Price;
        product.Stock = input.Stock;
        product.ImageUrl = input.ImageUrl;
        product.CategoryId = input.CategoryId;
        product.IsActive = input.IsActive;

        await Repository.UpdateAsync(product, true);

        // Update allergens
        var existingAllergens = await _productAllergenRepository
            .Where(x => x.ProductId == id)
            .ToListAsync();

        // Remove old allergens
        await _productAllergenRepository.DeleteManyAsync(existingAllergens, true);

        // Add new allergens
        foreach (var allergenId in input.AllergenIds)
        {
            await _productAllergenRepository.InsertAsync(
                new ProductAllergen(product.Id, allergenId),
                true
            );
        }

        return await GetAsync(id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var product = await Repository.GetAsync(id);

        // Delete image file if exists
        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            await _imageUploadService.DeleteImageAsync(product.ImageUrl);
        }

        await base.DeleteAsync(id);
    }
}
