using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Products;

public class ProductFilterDto : PagedAndSortedResultRequestDto
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool? InStockOnly { get; set; }
}
