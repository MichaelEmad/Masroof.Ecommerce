namespace Masroof.Ecommerce.Permissions;

public static class EcommercePermissions
{
    public const string GroupName = "Ecommerce";

    public static class Dashboard
    {
        public const string DashboardGroup = GroupName + ".Dashboard";
        public const string Host = DashboardGroup + ".Host";
    }

    public static class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Allergens
    {
        public const string Default = GroupName + ".Allergens";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Orders
    {
        public const string Default = GroupName + ".Orders";
        public const string ManageAll = Default + ".ManageAll";
    }

    public static class Cart
    {
        public const string Default = GroupName + ".Cart";
    }
}
