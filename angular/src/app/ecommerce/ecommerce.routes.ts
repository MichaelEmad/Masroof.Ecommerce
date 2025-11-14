import { Routes } from '@angular/router';

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
    path: '',
    redirectTo: 'products',
    pathMatch: 'full',
  },
];
