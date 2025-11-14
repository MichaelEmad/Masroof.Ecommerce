using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Products;

public class CreateUpdateProductDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DiscountPrice { get; set; }

    [StringLength(50)]
    public string? SKU { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Weight { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }
}
