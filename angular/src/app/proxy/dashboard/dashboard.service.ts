import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { DashboardStatsDto } from './models';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getStats(): Observable<DashboardStatsDto> {
    return this.restService.request<void, DashboardStatsDto>({
      method: 'GET',
      url: '/api/app/dashboard/stats',
    },
    { apiName: this.apiName });
  }
}
