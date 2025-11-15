import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { OrderDto, CreateOrderDto, UpdateOrderStatusDto, UpdateTrackingInfoDto } from '../products/models';

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

  // Admin methods
  getList = () =>
    this.restService.request<any, OrderDto[]>({
      method: 'GET',
      url: '/api/app/order',
    });

  get = (id: string) =>
    this.restService.request<any, OrderDto>({
      method: 'GET',
      url: `/api/app/order/${id}`,
    });

  updateStatus = (id: string, input: UpdateOrderStatusDto) =>
    this.restService.request<any, OrderDto>({
      method: 'PUT',
      url: `/api/app/order/${id}/status`,
      body: input,
    });

  updateTrackingInfo = (id: string, input: UpdateTrackingInfoDto) =>
    this.restService.request<any, OrderDto>({
      method: 'PUT',
      url: `/api/app/order/${id}/tracking`,
      body: input,
    });

  addAdminNotes = (id: string, notes: string) =>
    this.restService.request<any, OrderDto>({
      method: 'PUT',
      url: `/api/app/order/${id}/notes`,
      body: { notes },
    });

  downloadInvoice = (id: string) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      url: `/api/app/order/${id}/invoice`,
      responseType: 'blob',
    });
}
