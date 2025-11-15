import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { CustomerDto, CreateUpdateCustomerDto } from './models';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getMyProfile = () =>
    this.restService.request<any, CustomerDto>({
      method: 'GET',
      url: '/api/app/customer/my-profile',
    });

  updateMyProfile = (input: CreateUpdateCustomerDto) =>
    this.restService.request<any, CustomerDto>({
      method: 'PUT',
      url: '/api/app/customer/my-profile',
      body: input,
    });

  get = (id: string) =>
    this.restService.request<any, CustomerDto>({
      method: 'GET',
      url: `/api/app/customer/${id}`,
    });

  create = (input: CreateUpdateCustomerDto) =>
    this.restService.request<any, CustomerDto>({
      method: 'POST',
      url: '/api/app/customer',
      body: input,
    });

  update = (id: string, input: CreateUpdateCustomerDto) =>
    this.restService.request<any, CustomerDto>({
      method: 'PUT',
      url: `/api/app/customer/${id}`,
      body: input,
    });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/customer/${id}`,
    });
}
