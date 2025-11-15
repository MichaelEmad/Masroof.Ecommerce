export class EcommercePermissions {
  static readonly GroupName = 'Ecommerce';

  static readonly Dashboard = class {
    static readonly DashboardGroup = EcommercePermissions.GroupName + '.Dashboard';
    static readonly Host = EcommercePermissions.Dashboard.DashboardGroup + '.Host';
  };

  static readonly Products = class {
    static readonly Default = EcommercePermissions.GroupName + '.Products';
    static readonly Create = EcommercePermissions.Products.Default + '.Create';
    static readonly Edit = EcommercePermissions.Products.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Products.Default + '.Delete';
  };

  static readonly Categories = class {
    static readonly Default = EcommercePermissions.GroupName + '.Categories';
    static readonly Create = EcommercePermissions.Categories.Default + '.Create';
    static readonly Edit = EcommercePermissions.Categories.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Categories.Default + '.Delete';
  };

  static readonly Customers = class {
    static readonly Default = EcommercePermissions.GroupName + '.Customers';
    static readonly Create = EcommercePermissions.Customers.Default + '.Create';
    static readonly Edit = EcommercePermissions.Customers.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Customers.Default + '.Delete';
  };

  static readonly Orders = class {
    static readonly Default = EcommercePermissions.GroupName + '.Orders';
    static readonly Create = EcommercePermissions.Orders.Default + '.Create';
    static readonly Edit = EcommercePermissions.Orders.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Orders.Default + '.Delete';
    static readonly UpdateStatus = EcommercePermissions.Orders.Default + '.UpdateStatus';
  };

  static readonly Payments = class {
    static readonly Default = EcommercePermissions.GroupName + '.Payments';
    static readonly Process = EcommercePermissions.Payments.Default + '.Process';
    static readonly Refund = EcommercePermissions.Payments.Default + '.Refund';
  };

  static readonly ShoppingCart = class {
    static readonly Default = EcommercePermissions.GroupName + '.ShoppingCart';
  };

  static readonly Addresses = class {
    static readonly Default = EcommercePermissions.GroupName + '.Addresses';
    static readonly Create = EcommercePermissions.Addresses.Default + '.Create';
    static readonly Edit = EcommercePermissions.Addresses.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Addresses.Default + '.Delete';
  };

  static readonly Coupons = class {
    static readonly Default = EcommercePermissions.GroupName + '.Coupons';
    static readonly Create = EcommercePermissions.Coupons.Default + '.Create';
    static readonly Edit = EcommercePermissions.Coupons.Default + '.Edit';
    static readonly Delete = EcommercePermissions.Coupons.Default + '.Delete';
  };
}
