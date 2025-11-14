using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Catalog;

public interface ICategoryAppService :
    ICrudAppService<
        CategoryDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateCategoryDto,
        CreateUpdateCategoryDto>
{
}
