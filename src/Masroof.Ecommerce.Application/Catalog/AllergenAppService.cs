using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.Images;
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
    private readonly IImageUploadService _imageUploadService;

    public AllergenAppService(
        IRepository<Allergen, Guid> repository,
        IImageUploadService imageUploadService)
        : base(repository)
    {
        _imageUploadService = imageUploadService;

        GetPolicyName = EcommercePermissions.Allergens.Default;
        GetListPolicyName = EcommercePermissions.Allergens.Default;
        CreatePolicyName = EcommercePermissions.Allergens.Create;
        UpdatePolicyName = EcommercePermissions.Allergens.Edit;
        DeletePolicyName = EcommercePermissions.Allergens.Delete;
    }

    public override async Task<AllergenDto> UpdateAsync(Guid id, CreateUpdateAllergenDto input)
    {
        var allergen = await Repository.GetAsync(id);

        // Delete old icon if it's being replaced
        if (!string.IsNullOrWhiteSpace(allergen.Icon) &&
            allergen.Icon != input.Icon &&
            !string.IsNullOrWhiteSpace(input.Icon))
        {
            await _imageUploadService.DeleteImageAsync(allergen.Icon);
        }

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var allergen = await Repository.GetAsync(id);

        // Delete icon file if exists
        if (!string.IsNullOrWhiteSpace(allergen.Icon))
        {
            await _imageUploadService.DeleteImageAsync(allergen.Icon);
        }

        await base.DeleteAsync(id);
    }
}
