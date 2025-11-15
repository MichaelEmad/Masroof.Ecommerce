import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
import { EcommercePermissions } from './proxy/permissions/ecommerce-permissions';

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
        requiredPolicy: 'Ecommerce.Dashboard.Host || Ecommerce.Dashboard.Tenant',
      },
      {
        path: '/ecommerce',
        name: 'Ecommerce',
        iconClass: 'fas fa-shopping-cart',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/ecommerce/admin-dashboard',
        name: 'Admin Dashboard',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-tachometer-alt',
        order: 1,
        layout: eLayoutType.application,
        requiredPolicy: EcommercePermissions.Dashboard.Host,
      },
      {
        path: '/ecommerce/admin-products',
        name: 'Products Management',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-boxes',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: EcommercePermissions.Products.Default,
      },
      {
        path: '/ecommerce/admin-categories',
        name: 'Categories Management',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-tags',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: EcommercePermissions.Categories.Default,
      },
      {
        path: '/ecommerce/admin-orders',
        name: 'Orders Management',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-receipt',
        order: 4,
        layout: eLayoutType.application,
        requiredPolicy: EcommercePermissions.Orders.Default,
      },
      {
        path: '/ecommerce/coupon-management',
        name: 'Coupons Management',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-ticket-alt',
        order: 5,
        layout: eLayoutType.application,
        requiredPolicy: EcommercePermissions.Coupons.Default,
      },
      {
        path: '/ecommerce/products',
        name: 'Shop',
        parentName: 'Ecommerce',
        iconClass: 'fas fa-store',
        order: 10,
        layout: eLayoutType.application,
      },
  ]);
}
