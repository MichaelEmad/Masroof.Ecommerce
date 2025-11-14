using AutoMapper;
using Masroof.Ecommerce.Catalog;
using Masroof.Ecommerce.Carts;
using Masroof.Ecommerce.Orders;
using System.Linq;

namespace Masroof.Ecommerce;

public class EcommerceApplicationAutoMapperProfile : Profile
{
    public EcommerceApplicationAutoMapperProfile()
    {
        // Category
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateUpdateCategoryDto, Category>();

        // Allergen
        CreateMap<Allergen, AllergenDto>();
        CreateMap<CreateUpdateAllergenDto, Allergen>();

        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.Allergens, opt => opt.MapFrom(src => src.ProductAllergens.Select(pa => pa.Allergen).ToList()));
        CreateMap<CreateUpdateProductDto, Product>()
            .ForMember(dest => dest.ProductAllergens, opt => opt.Ignore());

        // CartItem
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetTotalPrice()));

        // Order
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetTotalPrice()));
    }
}
