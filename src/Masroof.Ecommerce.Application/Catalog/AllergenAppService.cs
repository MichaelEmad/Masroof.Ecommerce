using System;
using Masroof.Ecommerce.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Catalog;

public class AllergenAppService :
    CrudAppService<
        Allergen,
        AllergenDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAllergenDto,
        CreateUpdateAllergenDto>,
    IAllergenAppService
{
    public AllergenAppService(IRepository<Allergen, Guid> repository)
        : base(repository)
    {
        GetPolicyName = EcommercePermissions.Allergens.Default;
        GetListPolicyName = EcommercePermissions.Allergens.Default;
        CreatePolicyName = EcommercePermissions.Allergens.Create;
        UpdatePolicyName = EcommercePermissions.Allergens.Edit;
        DeletePolicyName = EcommercePermissions.Allergens.Delete;
    }
}
