import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ImageUploadResultDto {
  imageUrl: string;
  fileName: string;
}

@Injectable({
  providedIn: 'root'
})
export class ImageUploadService {
  private baseUrl = '/api/app/images';

  constructor(private http: HttpClient) {}

  uploadImage(file: File, entityType: 'products' | 'categories' | 'allergens'): Observable<ImageUploadResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);

    return this.http.post<ImageUploadResultDto>(`${this.baseUrl}/upload`, formData);
  }

  deleteImage(imageUrl: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/delete`, {
      params: { imageUrl }
    });
  }

  /**
   * Validates if the file is a valid image
   * @param file The file to validate
   * @returns true if valid, false otherwise
   */
  isValidImage(file: File): boolean {
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    const maxSize = 5 * 1024 * 1024; // 5MB

    if (!file) {
      return false;
    }

    if (!allowedTypes.includes(file.type)) {
      return false;
    }

    if (file.size > maxSize) {
      return false;
    }

    return true;
  }

  /**
   * Gets a validation error message for an invalid file
   * @param file The file to check
   * @returns Error message or null if valid
   */
  getValidationError(file: File): string | null {
    if (!file) {
      return 'Please select a file';
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      return 'Invalid file type. Allowed types: JPG, PNG, GIF, WebP';
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      return 'File size exceeds 5MB limit';
    }

    return null;
  }
}
