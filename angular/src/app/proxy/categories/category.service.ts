import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { CategoryDto } from '../products/models';

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
}
