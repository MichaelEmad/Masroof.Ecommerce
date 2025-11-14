import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { OrderDto, CreateOrderDto } from '../products/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getMyOrders = () =>
    this.restService.request<any, OrderDto[]>({
      method: 'GET',
      url: '/api/app/order/my-orders',
    });

  getMyOrder = (id: string) =>
    this.restService.request<any, OrderDto>({
      method: 'GET',
      url: `/api/app/order/my-order/${id}`,
    });

  create = (input: CreateOrderDto) =>
    this.restService.request<any, OrderDto>({
      method: 'POST',
      url: '/api/app/order',
      body: input,
    });

  cancel = (id: string) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/order/cancel/${id}`,
    });
}
