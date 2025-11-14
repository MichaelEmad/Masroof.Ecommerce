using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Categories;

public class Category : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }

    protected Category()
    {
    }

    public Category(
        Guid id,
        string name,
        string description,
        Guid? parentCategoryId = null
    ) : base(id)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
        DisplayOrder = 0;
        Slug = GenerateSlug(name);
    }

    public void UpdateName(string name)
    {
        Name = name;
        Slug = GenerateSlug(name);
    }

    private static string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }

    public void SetParentCategory(Guid? parentId)
    {
        ParentCategoryId = parentId;
    }

    public bool IsRootCategory()
    {
        return ParentCategoryId == null;
    }
}
