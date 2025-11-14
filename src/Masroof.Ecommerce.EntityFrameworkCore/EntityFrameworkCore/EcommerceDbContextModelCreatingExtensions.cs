using Masroof.Ecommerce.Catalog;
using Masroof.Ecommerce.Carts;
using Masroof.Ecommerce.Coupons;
using Masroof.Ecommerce.Orders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Masroof.Ecommerce.EntityFrameworkCore;

public static class EcommerceDbContextModelCreatingExtensions
{
    public static void ConfigureEcommerce(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // Category
        builder.Entity<Category>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Categories", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(512);

            b.HasIndex(x => x.Name);
        });

        // Allergen
        builder.Entity<Allergen>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Allergens", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Icon).HasMaxLength(256);

            b.HasIndex(x => x.Name);
        });

        // Product
        builder.Entity<Product>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Products", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(2048);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImageUrl).HasMaxLength(512);

            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.IsActive);
        });

        // ProductAllergen
        builder.Entity<ProductAllergen>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "ProductAllergens", EcommerceConsts.DbSchema);
            b.HasKey(x => new { x.ProductId, x.AllergenId });

            b.HasOne(x => x.Product)
                .WithMany(x => x.ProductAllergens)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Allergen)
                .WithMany()
                .HasForeignKey(x => x.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        builder.Entity<CartItem>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "CartItems", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ProductPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.ProductImageUrl).HasMaxLength(512);

            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => new { x.CustomerId, x.ProductId });
        });

        // Coupon
        builder.Entity<Coupon>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Coupons", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            b.Property(x => x.MinimumOrderAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.MaximumDiscountAmount).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.ValidFrom);
            b.HasIndex(x => x.ValidTo);
        });

        // Order
        builder.Entity<Order>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Orders", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.CustomerName).IsRequired().HasMaxLength(256);
            b.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

            b.HasMany(x => x.OrderItems)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CreationTime);
        });

        // OrderItem
        builder.Entity<OrderItem>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "OrderItems", EcommerceConsts.DbSchema);
            b.HasKey(x => x.Id);

            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.ProductId);
        });
    }
}
