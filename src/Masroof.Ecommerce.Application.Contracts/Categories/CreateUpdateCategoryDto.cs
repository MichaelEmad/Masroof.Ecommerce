using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Categories;

public class CreateUpdateCategoryDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [StringLength(256)]
    public string? MetaTitle { get; set; }

    [StringLength(500)]
    public string? MetaDescription { get; set; }

    [StringLength(500)]
    public string? MetaKeywords { get; set; }
}
