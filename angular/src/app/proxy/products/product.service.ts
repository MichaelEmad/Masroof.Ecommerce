import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import type { ProductDto, CreateUpdateProductDto } from './models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getList = () =>
    this.restService.request<any, PagedResultDto<ProductDto>>({
      method: 'GET',
      url: '/api/app/product',
    });

  get = (id: string) =>
    this.restService.request<any, ProductDto>({
      method: 'GET',
      url: `/api/app/product/${id}`,
    });

  getFeaturedProducts = () =>
    this.restService.request<any, ProductDto[]>({
      method: 'GET',
      url: '/api/app/product/featured-products',
    });

  getProductsByCategory = (categoryId: string) =>
    this.restService.request<any, ProductDto[]>({
      method: 'GET',
      url: `/api/app/product/products-by-category/${categoryId}`,
    });

  getPublicProducts = () =>
    this.restService.request<any, PagedResultDto<ProductDto>>({
      method: 'GET',
      url: '/api/app/product/public-products',
    });

  create = (input: CreateUpdateProductDto) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      url: '/api/app/product',
      body: input,
    });

  update = (id: string, input: CreateUpdateProductDto) =>
    this.restService.request<any, ProductDto>({
      method: 'PUT',
      url: `/api/app/product/${id}`,
      body: input,
    });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product/${id}`,
    });

  incrementViewCount = (id: string) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product/increment-view-count/${id}`,
    });
}
