using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Catalog;

public interface IProductAppService :
    ICrudAppService<
        ProductDto,
        Guid,
        GetProductListDto,
        CreateUpdateProductDto,
        CreateUpdateProductDto>
{
    Task<PagedResultDto<ProductDto>> GetPublicListAsync(GetProductListDto input);
}
