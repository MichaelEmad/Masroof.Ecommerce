using AutoMapper;
using Masroof.Ecommerce.Products;
using Masroof.Ecommerce.Categories;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Addresses;
using Masroof.Ecommerce.Orders;
using Masroof.Ecommerce.Payments;
using Masroof.Ecommerce.ShoppingCarts;

namespace Masroof.Ecommerce;

public class EcommerceApplicationAutoMapperProfile : Profile
{
    public EcommerceApplicationAutoMapperProfile()
    {
        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.EffectivePrice, opt => opt.MapFrom(src => src.GetEffectivePrice()))
            .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.IsInStock()));
        CreateMap<CreateUpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
            .ForMember(dest => dest.SoldCount, opt => opt.Ignore());

        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.IsRootCategory, opt => opt.MapFrom(src => src.IsRootCategory()));
        CreateMap<CreateUpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Slug, opt => opt.Ignore());

        // Customer
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.IsVipCustomer, opt => opt.MapFrom(src => src.IsVipCustomer()));
        CreateMap<CreateUpdateCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSpent, opt => opt.Ignore());

        // Address
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.FormattedAddress, opt => opt.MapFrom(src => src.GetFormattedAddress()));
        CreateMap<CreateUpdateAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore());

        // Order
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetTotalPrice()));

        // Payment
        CreateMap<Payment, PaymentDto>();
        CreateMap<CreatePaymentDto, Payment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.Amount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // ShoppingCart
        CreateMap<ShoppingCart, ShoppingCartDto>()
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.GetSubTotal()))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.GetTotal()))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.GetTotalItems()))
            .ForMember(dest => dest.IsEmpty, opt => opt.MapFrom(src => src.IsEmpty()));
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetTotalPrice()));
    }
}
