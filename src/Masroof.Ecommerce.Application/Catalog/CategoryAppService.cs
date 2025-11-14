using System;
using Masroof.Ecommerce.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Catalog;

public class CategoryAppService :
    CrudAppService<
        Category,
        CategoryDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCategoryDto,
        CreateUpdateCategoryDto>,
    ICategoryAppService
{
    public CategoryAppService(IRepository<Category, Guid> repository)
        : base(repository)
    {
        GetPolicyName = EcommercePermissions.Categories.Default;
        GetListPolicyName = EcommercePermissions.Categories.Default;
        CreatePolicyName = EcommercePermissions.Categories.Create;
        UpdatePolicyName = EcommercePermissions.Categories.Edit;
        DeletePolicyName = EcommercePermissions.Categories.Delete;
    }
}
