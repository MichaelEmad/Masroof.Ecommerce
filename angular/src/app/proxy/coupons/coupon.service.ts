import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { CouponDto, CreateUpdateCouponDto } from './models';

@Injectable({
  providedIn: 'root',
})
export class CouponService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getList(): Observable<CouponDto[]> {
    return this.restService.request<void, CouponDto[]>({
      method: 'GET',
      url: '/api/app/coupon',
    },
    { apiName: this.apiName });
  }

  get(id: string): Observable<CouponDto> {
    return this.restService.request<void, CouponDto>({
      method: 'GET',
      url: `/api/app/coupon/${id}`,
    },
    { apiName: this.apiName });
  }

  create(input: CreateUpdateCouponDto): Observable<CouponDto> {
    return this.restService.request<CreateUpdateCouponDto, CouponDto>({
      method: 'POST',
      url: '/api/app/coupon',
      body: input,
    },
    { apiName: this.apiName });
  }

  update(id: string, input: CreateUpdateCouponDto): Observable<CouponDto> {
    return this.restService.request<CreateUpdateCouponDto, CouponDto>({
      method: 'PUT',
      url: `/api/app/coupon/${id}`,
      body: input,
    },
    { apiName: this.apiName });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `/api/app/coupon/${id}`,
    },
    { apiName: this.apiName });
  }

  activate(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `/api/app/coupon/${id}/activate`,
    },
    { apiName: this.apiName });
  }

  deactivate(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `/api/app/coupon/${id}/deactivate`,
    },
    { apiName: this.apiName });
  }

  getActiveCoupons(): Observable<CouponDto[]> {
    return this.restService.request<void, CouponDto[]>({
      method: 'GET',
      url: '/api/app/coupon/active-coupons',
    },
    { apiName: this.apiName });
  }
}
