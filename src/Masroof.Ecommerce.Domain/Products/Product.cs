using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Products;

public class Product : FullAuditedAggregateRoot<Guid>
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
    public decimal Weight { get; set; }
    public string? Brand { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }

    protected Product()
    {
    }

    public Product(
        Guid id,
        string name,
        string description,
        decimal price,
        int stockQuantity,
        Guid? categoryId = null
    ) : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        IsActive = true;
        IsFeatured = false;
        ViewCount = 0;
        SoldCount = 0;
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
    }

    public void DecrementStock(int quantity)
    {
        if (StockQuantity < quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }
        StockQuantity -= quantity;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void IncrementSoldCount(int quantity)
    {
        SoldCount += quantity;
    }

    public bool IsInStock()
    {
        return StockQuantity > 0;
    }

    public decimal GetEffectivePrice()
    {
        return DiscountPrice ?? Price;
    }
}
