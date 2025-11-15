import { Routes } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';

export const ecommerceRoutes: Routes = [
  {
    path: 'products',
    loadComponent: () =>
      import('./product-catalog/product-catalog.component').then(m => m.ProductCatalogComponent),
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./shopping-cart/shopping-cart.component').then(m => m.ShoppingCartComponent),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard],
  },
  {
    path: 'checkout',
    loadComponent: () =>
      import('./checkout/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [authGuard],
  },
  {
    path: 'my-orders',
    loadComponent: () =>
      import('./my-orders/my-orders.component').then(m => m.MyOrdersComponent),
    canActivate: [authGuard],
  },
  {
    path: 'admin-dashboard',
    loadComponent: () =>
      import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'Ecommerce.Dashboard.Host',
    },
  },
  {
    path: 'admin-products',
    loadComponent: () =>
      import('./admin-products/admin-products.component').then(m => m.AdminProductsComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'Ecommerce.Products',
    },
  },
  {
    path: 'admin-categories',
    loadComponent: () =>
      import('./admin-categories/admin-categories.component').then(m => m.AdminCategoriesComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'Ecommerce.Categories',
    },
  },
  {
    path: 'admin-orders',
    loadComponent: () =>
      import('./admin-orders/admin-orders.component').then(m => m.AdminOrdersComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'Ecommerce.Orders',
    },
  },
  {
    path: 'coupon-management',
    loadComponent: () =>
      import('./coupon-management/coupon-management.component').then(m => m.CouponManagementComponent),
    canActivate: [authGuard, permissionGuard],
    data: {
      requiredPolicy: 'Ecommerce.Coupons',
    },
  },
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full',
  },
];
