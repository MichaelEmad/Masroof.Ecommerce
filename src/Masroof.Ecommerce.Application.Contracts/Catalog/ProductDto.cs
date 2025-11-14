using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Catalog;

public class ProductDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public List<AllergenDto> Allergens { get; set; } = new();
}

public class CreateUpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> AllergenIds { get; set; } = new();
}

public class GetProductListDto : PagedAndSortedResultRequestDto
{
    public Guid? CategoryId { get; set; }
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
