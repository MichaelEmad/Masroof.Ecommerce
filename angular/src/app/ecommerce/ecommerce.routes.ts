import { Routes } from '@angular/router';
import { authGuard } from '@abp/ng.core';

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
    path: 'my-orders',
    loadComponent: () =>
      import('./my-orders/my-orders.component').then(m => m.MyOrdersComponent),
    canActivate: [authGuard],
  },
  {
    path: 'admin-dashboard',
    loadComponent: () =>
      import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [authGuard],
  },
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full',
  },
];
