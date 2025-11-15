import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { CategoryDto, CreateUpdateCategoryDto } from '../products/models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  getRootCategories = () =>
    this.restService.request<any, CategoryDto[]>({
      method: 'GET',
      url: '/api/app/category/root-categories',
    });

  getActiveCategories = () =>
    this.restService.request<any, CategoryDto[]>({
      method: 'GET',
      url: '/api/app/category/active-categories',
    });

  getSubCategories = (parentId: string) =>
    this.restService.request<any, CategoryDto[]>({
      method: 'GET',
      url: `/api/app/category/sub-categories/${parentId}`,
    });

  // Admin methods
  getList = () =>
    this.restService.request<any, CategoryDto[]>({
      method: 'GET',
      url: '/api/app/category',
    });

  get = (id: string) =>
    this.restService.request<any, CategoryDto>({
      method: 'GET',
      url: `/api/app/category/${id}`,
    });

  create = (input: CreateUpdateCategoryDto) =>
    this.restService.request<any, CategoryDto>({
      method: 'POST',
      url: '/api/app/category',
      body: input,
    });

  update = (id: string, input: CreateUpdateCategoryDto) =>
    this.restService.request<any, CategoryDto>({
      method: 'PUT',
      url: `/api/app/category/${id}`,
      body: input,
    });

  delete = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/category/${id}`,
    });
}
