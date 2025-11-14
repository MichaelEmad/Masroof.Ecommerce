using Masroof.Ecommerce.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Masroof.Ecommerce.Permissions;

public class EcommercePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EcommercePermissions.GroupName);

        myGroup.AddPermission(EcommercePermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);

        // Categories - SuperAdmin only
        var categoriesPermission = myGroup.AddPermission(EcommercePermissions.Categories.Default, L("Permission:Categories"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Create, L("Permission:Categories.Create"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Edit, L("Permission:Categories.Edit"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Delete, L("Permission:Categories.Delete"));

        // Allergens - SuperAdmin only
        var allergensPermission = myGroup.AddPermission(EcommercePermissions.Allergens.Default, L("Permission:Allergens"));
        allergensPermission.AddChild(EcommercePermissions.Allergens.Create, L("Permission:Allergens.Create"));
        allergensPermission.AddChild(EcommercePermissions.Allergens.Edit, L("Permission:Allergens.Edit"));
        allergensPermission.AddChild(EcommercePermissions.Allergens.Delete, L("Permission:Allergens.Delete"));

        // Products - SuperAdmin only
        var productsPermission = myGroup.AddPermission(EcommercePermissions.Products.Default, L("Permission:Products"));
        productsPermission.AddChild(EcommercePermissions.Products.Create, L("Permission:Products.Create"));
        productsPermission.AddChild(EcommercePermissions.Products.Edit, L("Permission:Products.Edit"));
        productsPermission.AddChild(EcommercePermissions.Products.Delete, L("Permission:Products.Delete"));

        // Orders - SuperAdmin can manage all, Customers can manage their own
        var ordersPermission = myGroup.AddPermission(EcommercePermissions.Orders.Default, L("Permission:Orders"));
        ordersPermission.AddChild(EcommercePermissions.Orders.ManageAll, L("Permission:Orders.ManageAll"));

        // Cart - All authenticated users
        myGroup.AddPermission(EcommercePermissions.Cart.Default, L("Permission:Cart"));

        // Coupons - SuperAdmin only
        var couponsPermission = myGroup.AddPermission(EcommercePermissions.Coupons.Default, L("Permission:Coupons"));
        couponsPermission.AddChild(EcommercePermissions.Coupons.Create, L("Permission:Coupons.Create"));
        couponsPermission.AddChild(EcommercePermissions.Coupons.Edit, L("Permission:Coupons.Edit"));
        couponsPermission.AddChild(EcommercePermissions.Coupons.Delete, L("Permission:Coupons.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EcommerceResource>(name);
    }
}
