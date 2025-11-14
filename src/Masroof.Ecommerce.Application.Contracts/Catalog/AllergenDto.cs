using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Catalog;

public class AllergenDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
}

public class CreateUpdateAllergenDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
}
