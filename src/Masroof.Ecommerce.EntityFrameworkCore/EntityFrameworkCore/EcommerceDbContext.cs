using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Masroof.Ecommerce.Products;
using Masroof.Ecommerce.Categories;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Addresses;
using Masroof.Ecommerce.Orders;
using Masroof.Ecommerce.Payments;
using Masroof.Ecommerce.ShoppingCarts;
using Masroof.Ecommerce.Coupons;

namespace Masroof.Ecommerce.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityProDbContext))]
[ConnectionStringName("Default")]
public class EcommerceDbContext :
    AbpDbContext<EcommerceDbContext>,
    IIdentityProDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    // E-commerce entities
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentityPro();
        builder.ConfigureOpenIddictPro();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        // Product
        builder.Entity<Product>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Products", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            b.Property(x => x.ShortDescription).HasMaxLength(500);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.SKU).HasMaxLength(50);
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.Property(x => x.Brand).HasMaxLength(100);
            b.Property(x => x.Weight).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.SKU);
            b.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ProductImage
        builder.Entity<ProductImage>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "ProductImages", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(256);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            b.Property(x => x.Url).IsRequired().HasMaxLength(1000);
            b.HasIndex(x => x.ProductId);
            b.HasIndex(x => x.IsPrimary);
        });

        // Category
        builder.Entity<Category>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Categories", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.Property(x => x.Slug).HasMaxLength(256);
            b.Property(x => x.MetaTitle).HasMaxLength(256);
            b.Property(x => x.MetaDescription).HasMaxLength(500);
            b.Property(x => x.MetaKeywords).HasMaxLength(500);
            b.HasIndex(x => x.ParentCategoryId);
            b.HasIndex(x => x.Slug);
        });

        // Customer
        builder.Entity<Customer>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Customers", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FirstName).IsRequired().HasMaxLength(128);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(50);
            b.Property(x => x.ProfilePictureUrl).HasMaxLength(500);
            b.Property(x => x.TotalSpent).HasColumnType("decimal(18,2)");
            b.Property(x => x.CustomerNotes).HasMaxLength(2000);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Email);
        });

        // Address
        builder.Entity<Address>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Addresses", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FullName).IsRequired().HasMaxLength(128);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.AddressLine1).IsRequired().HasMaxLength(256);
            b.Property(x => x.AddressLine2).HasMaxLength(256);
            b.Property(x => x.City).IsRequired().HasMaxLength(100);
            b.Property(x => x.State).IsRequired().HasMaxLength(100);
            b.Property(x => x.PostalCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.CustomerId);
        });

        // Order
        builder.Entity<Order>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Orders", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
            b.Property(x => x.Tax).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.CouponCode).HasMaxLength(50);
            b.Property(x => x.ShippingFullName).IsRequired().HasMaxLength(128);
            b.Property(x => x.ShippingAddressLine1).IsRequired().HasMaxLength(256);
            b.Property(x => x.ShippingAddressLine2).HasMaxLength(256);
            b.Property(x => x.ShippingCity).IsRequired().HasMaxLength(100);
            b.Property(x => x.ShippingState).IsRequired().HasMaxLength(100);
            b.Property(x => x.ShippingPostalCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.ShippingCountry).IsRequired().HasMaxLength(100);
            b.Property(x => x.ShippingPhone).IsRequired().HasMaxLength(50);
            b.Property(x => x.BillingFullName).IsRequired().HasMaxLength(128);
            b.Property(x => x.BillingAddressLine1).IsRequired().HasMaxLength(256);
            b.Property(x => x.BillingAddressLine2).HasMaxLength(256);
            b.Property(x => x.BillingCity).IsRequired().HasMaxLength(100);
            b.Property(x => x.BillingState).IsRequired().HasMaxLength(100);
            b.Property(x => x.BillingPostalCode).IsRequired().HasMaxLength(20);
            b.Property(x => x.BillingCountry).IsRequired().HasMaxLength(100);
            b.Property(x => x.BillingPhone).IsRequired().HasMaxLength(50);
            b.Property(x => x.CustomerNotes).HasMaxLength(2000);
            b.Property(x => x.AdminNotes).HasMaxLength(2000);
            b.Property(x => x.TrackingNumber).HasMaxLength(100);
            b.Property(x => x.ShippingCarrier).HasMaxLength(100);
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.OrderNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasMany(x => x.Items).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        builder.Entity<OrderItem>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "OrderItems", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.HasIndex(x => x.ProductId);
        });

        // Payment
        builder.Entity<Payment>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Payments", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TransactionId).HasMaxLength(256);
            b.Property(x => x.PaymentGateway).HasMaxLength(50);
            b.Property(x => x.PaymentGatewayResponse).HasMaxLength(2000);
            b.Property(x => x.FailureReason).HasMaxLength(1000);
            b.Property(x => x.CardLast4Digits).HasMaxLength(4);
            b.Property(x => x.CardBrand).HasMaxLength(50);
            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.TransactionId);
        });

        // ShoppingCart
        builder.Entity<ShoppingCart>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "ShoppingCarts", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CouponCode).HasMaxLength(50);
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.CustomerId).IsUnique();
            b.HasMany(x => x.Items).WithOne().HasForeignKey("ShoppingCartId").OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        builder.Entity<CartItem>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "CartItems", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImageUrl).HasMaxLength(500);
            b.HasIndex(x => x.ProductId);
        });

        // Coupon
        builder.Entity<Coupon>(b =>
        {
            b.ToTable(EcommerceConsts.DbTablePrefix + "Coupons", EcommerceConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            b.Property(x => x.MinimumOrderAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.MaximumDiscountAmount).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.ValidFrom);
            b.HasIndex(x => x.ValidTo);
        });
    }
}
