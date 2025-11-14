using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Catalog;

public class Product : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductAllergen> ProductAllergens { get; set; } = new List<ProductAllergen>();

    protected Product()
    {
    }

    public Product(
        Guid id,
        string name,
        decimal price,
        int stock,
        Guid categoryId,
        string? description = null,
        string? imageUrl = null)
        : base(id)
    {
        Name = name;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void ReduceStock(int quantity)
    {
        if (Stock < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for product {Name}. Available: {Stock}, Requested: {quantity}");
        }
        Stock -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        Stock += quantity;
    }
}
