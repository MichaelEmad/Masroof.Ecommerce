import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { AddressDto, CreateUpdateAddressDto } from './models';

@Injectable({ providedIn: 'root' })
export class AddressService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getMyAddresses = () =>
    this.restService.request<any, AddressDto[]>({
      method: 'GET',
      url: '/api/app/address/my-addresses',
    });

  get = (id: string) =>
    this.restService.request<any, AddressDto>({
      method: 'GET',
      url: `/api/app/address/${id}`,
    });

  create = (input: CreateUpdateAddressDto) =>
    this.restService.request<any, AddressDto>({
      method: 'POST',
      url: '/api/app/address',
      body: input,
    });

  update = (id: string, input: CreateUpdateAddressDto) =>
    this.restService.request<any, AddressDto>({
      method: 'PUT',
      url: `/api/app/address/${id}`,
      body: input,
    });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/address/${id}`,
    });

  setAsDefault = (id: string) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/address/set-as-default/${id}`,
    });
}
