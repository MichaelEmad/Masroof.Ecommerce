using System;
using Volo.Abp.Domain.Entities;

namespace Masroof.Ecommerce.Catalog;

public class ProductAllergen : Entity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid AllergenId { get; set; }
    public Allergen? Allergen { get; set; }

    protected ProductAllergen()
    {
    }

    public ProductAllergen(Guid productId, Guid allergenId)
    {
        ProductId = productId;
        AllergenId = allergenId;
    }

    public override object[] GetKeys()
    {
        return new object[] { ProductId, AllergenId };
    }
}
