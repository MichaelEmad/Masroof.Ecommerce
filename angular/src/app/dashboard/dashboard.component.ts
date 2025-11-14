import { Component } from '@angular/core';
import { PermissionDirective } from '@abp/ng.core';
import { HostDashboardComponent } from './host-dashboard/host-dashboard.component';

@Component({
  selector: 'app-dashboard',
  template: `
    <app-host-dashboard *abpPermission="'Ecommerce.Dashboard.Host'" />
  `,
  imports: [HostDashboardComponent, PermissionDirective]
})
export class DashboardComponent {}
