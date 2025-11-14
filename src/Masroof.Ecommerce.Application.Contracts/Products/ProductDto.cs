using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Products;

public class ProductDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Weight { get; set; }
    public string? Brand { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public decimal EffectivePrice { get; set; }
    public bool InStock { get; set; }
}
