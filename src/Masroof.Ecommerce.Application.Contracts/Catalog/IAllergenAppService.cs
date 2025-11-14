using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Catalog;

public interface IAllergenAppService :
    ICrudAppService<
        AllergenDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAllergenDto,
        CreateUpdateAllergenDto>
{
}
