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

        // Products
        var productsPermission = myGroup.AddPermission(EcommercePermissions.Products.Default, L("Permission:Products"));
        productsPermission.AddChild(EcommercePermissions.Products.Create, L("Permission:Products.Create"));
        productsPermission.AddChild(EcommercePermissions.Products.Edit, L("Permission:Products.Edit"));
        productsPermission.AddChild(EcommercePermissions.Products.Delete, L("Permission:Products.Delete"));

        // Categories
        var categoriesPermission = myGroup.AddPermission(EcommercePermissions.Categories.Default, L("Permission:Categories"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Create, L("Permission:Categories.Create"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Edit, L("Permission:Categories.Edit"));
        categoriesPermission.AddChild(EcommercePermissions.Categories.Delete, L("Permission:Categories.Delete"));

        // Customers
        var customersPermission = myGroup.AddPermission(EcommercePermissions.Customers.Default, L("Permission:Customers"));
        customersPermission.AddChild(EcommercePermissions.Customers.Create, L("Permission:Customers.Create"));
        customersPermission.AddChild(EcommercePermissions.Customers.Edit, L("Permission:Customers.Edit"));
        customersPermission.AddChild(EcommercePermissions.Customers.Delete, L("Permission:Customers.Delete"));

        // Orders
        var ordersPermission = myGroup.AddPermission(EcommercePermissions.Orders.Default, L("Permission:Orders"));
        ordersPermission.AddChild(EcommercePermissions.Orders.Create, L("Permission:Orders.Create"));
        ordersPermission.AddChild(EcommercePermissions.Orders.Edit, L("Permission:Orders.Edit"));
        ordersPermission.AddChild(EcommercePermissions.Orders.Delete, L("Permission:Orders.Delete"));
        ordersPermission.AddChild(EcommercePermissions.Orders.UpdateStatus, L("Permission:Orders.UpdateStatus"));

        // Payments
        var paymentsPermission = myGroup.AddPermission(EcommercePermissions.Payments.Default, L("Permission:Payments"));
        paymentsPermission.AddChild(EcommercePermissions.Payments.Process, L("Permission:Payments.Process"));
        paymentsPermission.AddChild(EcommercePermissions.Payments.Refund, L("Permission:Payments.Refund"));

        // ShoppingCart
        myGroup.AddPermission(EcommercePermissions.ShoppingCart.Default, L("Permission:ShoppingCart"));

        // Addresses
        var addressesPermission = myGroup.AddPermission(EcommercePermissions.Addresses.Default, L("Permission:Addresses"));
        addressesPermission.AddChild(EcommercePermissions.Addresses.Create, L("Permission:Addresses.Create"));
        addressesPermission.AddChild(EcommercePermissions.Addresses.Edit, L("Permission:Addresses.Edit"));
        addressesPermission.AddChild(EcommercePermissions.Addresses.Delete, L("Permission:Addresses.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EcommerceResource>(name);
    }
}
