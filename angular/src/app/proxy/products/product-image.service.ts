import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import type { ProductImageDto, UploadProductImageDto } from './models';

@Injectable({ providedIn: 'root' })
export class ProductImageService {
  apiName = 'Default';

  constructor(private restService: RestService) {}

  uploadImage = (input: UploadProductImageDto) =>
    this.restService.request<any, ProductImageDto>({
      method: 'POST',
      url: '/api/app/product-image/upload-image',
      body: input,
    });

  deleteImage = (id: string) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-image/${id}`,
    });

  setPrimaryImage = (id: string) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product-image/${id}/set-primary`,
    });

  getProductImages = (productId: string) =>
    this.restService.request<any, ProductImageDto[]>({
      method: 'GET',
      url: `/api/app/product-image/product-images/${productId}`,
    });

  updateDisplayOrder = (imageId: string, displayOrder: number) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product-image/${imageId}/display-order`,
      params: { displayOrder },
    });
}
