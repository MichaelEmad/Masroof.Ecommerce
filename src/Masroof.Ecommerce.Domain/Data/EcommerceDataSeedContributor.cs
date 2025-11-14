using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.Categories;
using Masroof.Ecommerce.Products;
using Masroof.Ecommerce.Coupons;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Masroof.Ecommerce.Data;

public class EcommerceDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<Coupon, Guid> _couponRepository;
    private readonly IGuidGenerator _guidGenerator;

    public EcommerceDataSeedContributor(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<Coupon, Guid> couponRepository,
        IGuidGenerator guidGenerator)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedCategoriesAsync();
        await SeedProductsAsync();
        await SeedCouponsAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _categoryRepository.GetCountAsync() > 0)
        {
            return; // Already seeded
        }

        // Root Categories
        var electronicsId = _guidGenerator.Create();
        var fashionId = _guidGenerator.Create();
        var homeId = _guidGenerator.Create();
        var sportsId = _guidGenerator.Create();

        var electronics = new Category(
            electronicsId,
            "Electronics",
            "Latest electronic devices and gadgets"
        );
        electronics.ImageUrl = "/images/categories/electronics.jpg";
        electronics.DisplayOrder = 1;

        var fashion = new Category(
            fashionId,
            "Fashion",
            "Trendy clothing and accessories"
        );
        fashion.ImageUrl = "/images/categories/fashion.jpg";
        fashion.DisplayOrder = 2;

        var home = new Category(
            homeId,
            "Home & Garden",
            "Everything for your home and garden"
        );
        home.ImageUrl = "/images/categories/home.jpg";
        home.DisplayOrder = 3;

        var sports = new Category(
            sportsId,
            "Sports & Outdoors",
            "Sports equipment and outdoor gear"
        );
        sports.ImageUrl = "/images/categories/sports.jpg";
        sports.DisplayOrder = 4;

        await _categoryRepository.InsertManyAsync(new[] { electronics, fashion, home, sports }, autoSave: true);

        // Subcategories for Electronics
        var laptops = new Category(
            _guidGenerator.Create(),
            "Laptops",
            "Portable computers for work and play",
            electronicsId
        );
        laptops.DisplayOrder = 1;

        var smartphones = new Category(
            _guidGenerator.Create(),
            "Smartphones",
            "Latest mobile phones",
            electronicsId
        );
        smartphones.DisplayOrder = 2;

        var headphones = new Category(
            _guidGenerator.Create(),
            "Headphones",
            "Audio devices and accessories",
            electronicsId
        );
        headphones.DisplayOrder = 3;

        // Subcategories for Fashion
        var menClothing = new Category(
            _guidGenerator.Create(),
            "Men's Clothing",
            "Fashion for men",
            fashionId
        );
        menClothing.DisplayOrder = 1;

        var womenClothing = new Category(
            _guidGenerator.Create(),
            "Women's Clothing",
            "Fashion for women",
            fashionId
        );
        womenClothing.DisplayOrder = 2;

        await _categoryRepository.InsertManyAsync(new[] { laptops, smartphones, headphones, menClothing, womenClothing }, autoSave: true);
    }

    private async Task SeedProductsAsync()
    {
        if (await _productRepository.GetCountAsync() > 0)
        {
            return; // Already seeded
        }

        var categories = await _categoryRepository.GetListAsync();
        var laptopsCategory = categories.Find(c => c.Name == "Laptops");
        var smartphonesCategory = categories.Find(c => c.Name == "Smartphones");
        var headphonesCategory = categories.Find(c => c.Name == "Headphones");

        // Laptop Products
        var laptop1 = new Product(
            _guidGenerator.Create(),
            "Dell XPS 13",
            "Powerful ultrabook with stunning display. Intel Core i7, 16GB RAM, 512GB SSD.",
            1299.99m,
            15,
            laptopsCategory?.Id
        );
        laptop1.ShortDescription = "Premium ultrabook for professionals";
        laptop1.SKU = "DELL-XPS13-001";
        laptop1.Brand = "Dell";
        laptop1.Weight = 1.2m;
        laptop1.IsFeatured = true;
        laptop1.ImageUrl = "/images/products/dell-xps13.jpg";

        var laptop2 = new Product(
            _guidGenerator.Create(),
            "MacBook Air M2",
            "Apple's latest MacBook Air with M2 chip. 8GB RAM, 256GB SSD, Retina Display.",
            1199.99m,
            20,
            laptopsCategory?.Id
        );
        laptop2.ShortDescription = "Thin, light, and powerful";
        laptop2.SKU = "APPLE-MBA-M2";
        laptop2.Brand = "Apple";
        laptop2.Weight = 1.24m;
        laptop2.IsFeatured = true;
        laptop2.DiscountPrice = 1099.99m;
        laptop2.ImageUrl = "/images/products/macbook-air-m2.jpg";

        // Smartphone Products
        var phone1 = new Product(
            _guidGenerator.Create(),
            "iPhone 14 Pro",
            "Latest iPhone with A16 Bionic chip. 128GB storage, Pro camera system.",
            999.99m,
            30,
            smartphonesCategory?.Id
        );
        phone1.ShortDescription = "Pro camera. Pro display. Pro performance.";
        phone1.SKU = "APPLE-IP14P-128";
        phone1.Brand = "Apple";
        phone1.Weight = 0.206m;
        phone1.IsFeatured = true;
        phone1.ImageUrl = "/images/products/iphone-14-pro.jpg";

        var phone2 = new Product(
            _guidGenerator.Create(),
            "Samsung Galaxy S23",
            "Samsung's flagship with Snapdragon 8 Gen 2. 256GB storage, 50MP camera.",
            899.99m,
            25,
            smartphonesCategory?.Id
        );
        phone2.ShortDescription = "Epic performance meets epic camera";
        phone2.SKU = "SAMSUNG-S23-256";
        phone2.Brand = "Samsung";
        phone2.Weight = 0.168m;
        phone2.IsFeatured = true;
        phone2.DiscountPrice = 799.99m;
        phone2.ImageUrl = "/images/products/galaxy-s23.jpg";

        // Headphones Products
        var headphone1 = new Product(
            _guidGenerator.Create(),
            "Sony WH-1000XM5",
            "Industry-leading noise canceling wireless headphones. 30-hour battery life.",
            399.99m,
            40,
            headphonesCategory?.Id
        );
        headphone1.ShortDescription = "Best noise canceling headphones";
        headphone1.SKU = "SONY-WH1000XM5";
        headphone1.Brand = "Sony";
        headphone1.Weight = 0.25m;
        headphone1.IsFeatured = true;
        headphone1.ImageUrl = "/images/products/sony-wh1000xm5.jpg";

        var headphone2 = new Product(
            _guidGenerator.Create(),
            "AirPods Pro 2",
            "Apple AirPods Pro with active noise cancellation and spatial audio.",
            249.99m,
            50,
            headphonesCategory?.Id
        );
        headphone2.ShortDescription = "Adaptive audio. Now playing.";
        headphone2.SKU = "APPLE-APP2";
        headphone2.Brand = "Apple";
        headphone2.Weight = 0.056m;
        headphone2.IsFeatured = false;
        headphone2.ImageUrl = "/images/products/airpods-pro-2.jpg";

        await _productRepository.InsertManyAsync(
            new[] { laptop1, laptop2, phone1, phone2, headphone1, headphone2 },
            autoSave: true
        );
    }

    private async Task SeedCouponsAsync()
    {
        if (await _couponRepository.GetCountAsync() > 0)
        {
            return; // Already seeded
        }

        var now = DateTime.UtcNow;

        // Welcome Coupon - 10% off
        var welcomeCoupon = new Coupon(
            _guidGenerator.Create(),
            "WELCOME10",
            DiscountType.Percentage,
            10m,
            now,
            now.AddMonths(3)
        );
        welcomeCoupon.Description = "Welcome discount for new customers";
        welcomeCoupon.MinimumOrderAmount = 50m;
        welcomeCoupon.MaxUsageCount = 100;

        // Summer Sale - 15% off
        var summerCoupon = new Coupon(
            _guidGenerator.Create(),
            "SUMMER15",
            DiscountType.Percentage,
            15m,
            now,
            now.AddMonths(2)
        );
        summerCoupon.Description = "Summer sale discount";
        summerCoupon.MinimumOrderAmount = 100m;
        summerCoupon.MaximumDiscountAmount = 50m;
        summerCoupon.MaxUsageCount = 50;

        // Fixed Discount - $25 off
        var fixedCoupon = new Coupon(
            _guidGenerator.Create(),
            "SAVE25",
            DiscountType.FixedAmount,
            25m,
            now,
            now.AddMonths(6)
        );
        fixedCoupon.Description = "Save $25 on orders over $200";
        fixedCoupon.MinimumOrderAmount = 200m;
        fixedCoupon.MaxUsageCount = 200;

        // VIP Coupon - 20% off
        var vipCoupon = new Coupon(
            _guidGenerator.Create(),
            "VIP20",
            DiscountType.Percentage,
            20m,
            now,
            now.AddYears(1)
        );
        vipCoupon.Description = "VIP customer exclusive discount";
        vipCoupon.MinimumOrderAmount = 500m;
        vipCoupon.MaxUsageCount = 25;

        // Flash Sale - 30% off (limited time)
        var flashCoupon = new Coupon(
            _guidGenerator.Create(),
            "FLASH30",
            DiscountType.Percentage,
            30m,
            now,
            now.AddDays(7)
        );
        flashCoupon.Description = "Flash sale - 7 days only!";
        flashCoupon.MinimumOrderAmount = 150m;
        flashCoupon.MaximumDiscountAmount = 100m;
        flashCoupon.MaxUsageCount = 30;

        await _couponRepository.InsertManyAsync(
            new[] { welcomeCoupon, summerCoupon, fixedCoupon, vipCoupon, flashCoupon },
            autoSave: true
        );
    }
}
