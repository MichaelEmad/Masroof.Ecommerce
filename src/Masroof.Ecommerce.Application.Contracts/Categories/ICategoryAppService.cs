using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Categories;

public interface ICategoryAppService : ICrudAppService<CategoryDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCategoryDto>
{
    Task<ListResultDto<CategoryDto>> GetRootCategoriesAsync();
    Task<ListResultDto<CategoryDto>> GetSubCategoriesAsync(Guid parentId);
    Task<ListResultDto<CategoryDto>> GetActiveCategoriesAsync();
}
