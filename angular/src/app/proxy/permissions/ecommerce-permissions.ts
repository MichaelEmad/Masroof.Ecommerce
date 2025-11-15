export class EcommercePermissions {
  static readonly GroupName = 'Ecommerce';

  static readonly Dashboard = class {
    static readonly DashboardGroup = 'Ecommerce.Dashboard';
    static readonly Host = 'Ecommerce.Dashboard.Host';
  };

  static readonly Products = class {
    static readonly Default = 'Ecommerce.Products';
    static readonly Create = 'Ecommerce.Products.Create';
    static readonly Edit = 'Ecommerce.Products.Edit';
    static readonly Delete = 'Ecommerce.Products.Delete';
  };

  static readonly Categories = class {
    static readonly Default = 'Ecommerce.Categories';
    static readonly Create = 'Ecommerce.Categories.Create';
    static readonly Edit = 'Ecommerce.Categories.Edit';
    static readonly Delete = 'Ecommerce.Categories.Delete';
  };

  static readonly Customers = class {
    static readonly Default = 'Ecommerce.Customers';
    static readonly Create = 'Ecommerce.Customers.Create';
    static readonly Edit = 'Ecommerce.Customers.Edit';
    static readonly Delete = 'Ecommerce.Customers.Delete';
  };

  static readonly Orders = class {
    static readonly Default = 'Ecommerce.Orders';
    static readonly Create = 'Ecommerce.Orders.Create';
    static readonly Edit = 'Ecommerce.Orders.Edit';
    static readonly Delete = 'Ecommerce.Orders.Delete';
    static readonly UpdateStatus = 'Ecommerce.Orders.UpdateStatus';
  };

  static readonly Payments = class {
    static readonly Default = 'Ecommerce.Payments';
    static readonly Process = 'Ecommerce.Payments.Process';
    static readonly Refund = 'Ecommerce.Payments.Refund';
  };

  static readonly ShoppingCart = class {
    static readonly Default = 'Ecommerce.ShoppingCart';
  };

  static readonly Addresses = class {
    static readonly Default = 'Ecommerce.Addresses';
    static readonly Create = 'Ecommerce.Addresses.Create';
    static readonly Edit = 'Ecommerce.Addresses.Edit';
    static readonly Delete = 'Ecommerce.Addresses.Delete';
  };

  static readonly Coupons = class {
    static readonly Default = 'Ecommerce.Coupons';
    static readonly Create = 'Ecommerce.Coupons.Create';
    static readonly Edit = 'Ecommerce.Coupons.Edit';
    static readonly Delete = 'Ecommerce.Coupons.Delete';
  };
}
