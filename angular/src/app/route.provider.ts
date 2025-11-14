import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/dashboard',
        name: '::Menu:Dashboard',
        iconClass: 'fas fa-chart-line',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Dashboard.Host  || Ecommerce.Dashboard.Tenant',
      },
      {
        path: '/products',
        name: '::Menu:Products',
        iconClass: 'fas fa-shopping-bag',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/cart',
        name: '::Menu:Cart',
        iconClass: 'fas fa-shopping-cart',
        order: 4,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Cart',
      },
      {
        path: '/orders',
        name: '::Menu:MyOrders',
        iconClass: 'fas fa-receipt',
        order: 5,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Orders',
      },
      {
        path: '/admin/categories',
        name: '::Menu:Categories',
        iconClass: 'fas fa-list',
        parentName: '::Menu:Administration',
        order: 1,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Categories',
      },
      {
        path: '/admin/allergens',
        name: '::Menu:Allergens',
        iconClass: 'fas fa-exclamation-triangle',
        parentName: '::Menu:Administration',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Allergens',
      },
      {
        path: '/admin/products',
        name: '::Menu:AdminProducts',
        iconClass: 'fas fa-boxes',
        parentName: '::Menu:Administration',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Products',
      },
      {
        path: '/admin/orders',
        name: '::Menu:AllOrders',
        iconClass: 'fas fa-file-invoice',
        parentName: '::Menu:Administration',
        order: 4,
        layout: eLayoutType.application,
        requiredPolicy: 'Ecommerce.Orders.ManageAll',
      },
  ]);
}
