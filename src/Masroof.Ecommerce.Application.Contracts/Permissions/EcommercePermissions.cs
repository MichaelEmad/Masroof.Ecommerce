namespace Masroof.Ecommerce.Permissions;

public static class EcommercePermissions
{
    public const string GroupName = "Ecommerce";

    public static class Dashboard
    {
        public const string DashboardGroup = GroupName + ".Dashboard";
        public const string Host = DashboardGroup + ".Host";
    }

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Categories
    {
        public const string Default = GroupName + ".Categories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Orders
    {
        public const string Default = GroupName + ".Orders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string UpdateStatus = Default + ".UpdateStatus";
    }

    public static class Payments
    {
        public const string Default = GroupName + ".Payments";
        public const string Process = Default + ".Process";
        public const string Refund = Default + ".Refund";
    }

    public static class ShoppingCart
    {
        public const string Default = GroupName + ".ShoppingCart";
    }

    public static class Addresses
    {
        public const string Default = GroupName + ".Addresses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Coupons
    {
        public const string Default = GroupName + ".Coupons";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
