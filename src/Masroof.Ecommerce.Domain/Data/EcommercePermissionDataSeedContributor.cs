using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace Masroof.Ecommerce.Data;

public class EcommercePermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public EcommercePermissionDataSeedContributor(
        IPermissionDataSeeder permissionDataSeeder,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _permissionDataSeeder = permissionDataSeeder;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await SeedAdminPermissionsAsync();
        }
    }

    private async Task SeedAdminPermissionsAsync()
    {
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            new[]
            {
                // Dashboard
                EcommercePermissions.Dashboard.Host,

                // Products - Full CRUD
                EcommercePermissions.Products.Default,
                EcommercePermissions.Products.Create,
                EcommercePermissions.Products.Edit,
                EcommercePermissions.Products.Delete,

                // Categories - Full CRUD
                EcommercePermissions.Categories.Default,
                EcommercePermissions.Categories.Create,
                EcommercePermissions.Categories.Edit,
                EcommercePermissions.Categories.Delete,

                // Customers - Full CRUD
                EcommercePermissions.Customers.Default,
                EcommercePermissions.Customers.Create,
                EcommercePermissions.Customers.Edit,
                EcommercePermissions.Customers.Delete,

                // Orders - Full CRUD + UpdateStatus
                EcommercePermissions.Orders.Default,
                EcommercePermissions.Orders.Create,
                EcommercePermissions.Orders.Edit,
                EcommercePermissions.Orders.Delete,
                EcommercePermissions.Orders.UpdateStatus,

                // Payments
                EcommercePermissions.Payments.Default,
                EcommercePermissions.Payments.Process,
                EcommercePermissions.Payments.Refund,

                // Shopping Cart
                EcommercePermissions.ShoppingCart.Default,

                // Addresses - Full CRUD
                EcommercePermissions.Addresses.Default,
                EcommercePermissions.Addresses.Create,
                EcommercePermissions.Addresses.Edit,
                EcommercePermissions.Addresses.Delete,

                // Coupons - Full CRUD
                EcommercePermissions.Coupons.Default,
                EcommercePermissions.Coupons.Create,
                EcommercePermissions.Coupons.Edit,
                EcommercePermissions.Coupons.Delete,
            },
            null
        );
    }
}
