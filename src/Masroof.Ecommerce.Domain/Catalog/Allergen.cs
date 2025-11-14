using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Catalog;

public class Allergen : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    protected Allergen()
    {
    }

    public Allergen(Guid id, string name, string? description = null, string? icon = null)
        : base(id)
    {
        Name = name;
        Description = description;
        Icon = icon;
    }
}
