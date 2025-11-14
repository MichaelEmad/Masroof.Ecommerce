using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Categories;

[Authorize(EcommercePermissions.Categories.Default)]
public class CategoryAppService : CrudAppService<Category, CategoryDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCategoryDto>, ICategoryAppService
{
    public CategoryAppService(IRepository<Category, Guid> repository) : base(repository)
    {
        GetPolicyName = EcommercePermissions.Categories.Default;
        GetListPolicyName = EcommercePermissions.Categories.Default;
        CreatePolicyName = EcommercePermissions.Categories.Create;
        UpdatePolicyName = EcommercePermissions.Categories.Edit;
        DeletePolicyName = EcommercePermissions.Categories.Delete;
    }

    [AllowAnonymous]
    public async Task<ListResultDto<CategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await Repository.GetListAsync();
        var rootCategories = categories
            .Where(c => c.IsRootCategory() && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();

        return new ListResultDto<CategoryDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Category>, System.Collections.Generic.List<CategoryDto>>(rootCategories)
        );
    }

    [AllowAnonymous]
    public async Task<ListResultDto<CategoryDto>> GetSubCategoriesAsync(Guid parentId)
    {
        var categories = await Repository.GetListAsync();
        var subCategories = categories
            .Where(c => c.ParentCategoryId == parentId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();

        return new ListResultDto<CategoryDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Category>, System.Collections.Generic.List<CategoryDto>>(subCategories)
        );
    }

    [AllowAnonymous]
    public async Task<ListResultDto<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await Repository.GetListAsync();
        var activeCategories = categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();

        return new ListResultDto<CategoryDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Category>, System.Collections.Generic.List<CategoryDto>>(activeCategories)
        );
    }

    protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await Repository.GetQueryableAsync())
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name);
    }
}
