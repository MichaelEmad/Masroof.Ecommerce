# Image Upload Implementation Guide

This guide explains how to use the image upload feature in the e-commerce application.

## Backend API

### Upload Image

**Endpoint:** `POST /api/app/images/upload`

**Content-Type:** `multipart/form-data`

**Parameters:**
- `file` (required): The image file to upload
- `entityType` (required): One of `"products"`, `"categories"`, or `"allergens"`

**Response:**
```json
{
  "imageUrl": "/uploads/products/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg",
  "fileName": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:44370/api/app/images/upload" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "file=@/path/to/image.jpg" \
  -F "entityType=products"
```

### Delete Image

**Endpoint:** `DELETE /api/app/images/delete`

**Parameters:**
- `imageUrl` (query parameter, required): The URL of the image to delete

**Response:** `200 OK`

**cURL Example:**
```bash
curl -X DELETE "https://localhost:44370/api/app/images/delete?imageUrl=/uploads/products/a1b2c3d4.jpg" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Frontend Implementation (Angular)

### 1. Image Upload Service

A reusable service is provided at `angular/src/app/shared/services/image-upload.service.ts`:

```typescript
import { ImageUploadService } from './shared/services/image-upload.service';

// In your component
constructor(private imageUploadService: ImageUploadService) {}

uploadImage(file: File) {
  // Validate first
  const error = this.imageUploadService.getValidationError(file);
  if (error) {
    alert(error);
    return;
  }

  // Upload
  this.imageUploadService.uploadImage(file, 'products').subscribe(
    result => {
      console.log('Uploaded:', result.imageUrl);
      this.product.imageUrl = result.imageUrl;
    },
    error => {
      console.error('Upload failed:', error);
    }
  );
}
```

### 2. Component Example

```typescript
import { Component } from '@angular/core';
import { ImageUploadService } from '../shared/services/image-upload.service';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent {
  product = {
    name: '',
    imageUrl: ''
  };
  uploading = false;

  constructor(private imageUploadService: ImageUploadService) {}

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];

    if (!file) {
      return;
    }

    // Validate
    const error = this.imageUploadService.getValidationError(file);
    if (error) {
      alert(error);
      return;
    }

    // Upload
    this.uploading = true;
    this.imageUploadService.uploadImage(file, 'products').subscribe({
      next: (result) => {
        this.product.imageUrl = result.imageUrl;
        this.uploading = false;
        console.log('Image uploaded successfully');
      },
      error: (error) => {
        console.error('Upload failed:', error);
        this.uploading = false;
        alert('Upload failed. Please try again.');
      }
    });
  }

  removeImage(): void {
    if (this.product.imageUrl) {
      this.imageUploadService.deleteImage(this.product.imageUrl).subscribe({
        next: () => {
          this.product.imageUrl = '';
          console.log('Image deleted successfully');
        },
        error: (error) => {
          console.error('Delete failed:', error);
        }
      });
    }
  }
}
```

### 3. Template Example

```html
<div class="form-group">
  <label>Product Image</label>

  <!-- File Input -->
  <input
    type="file"
    class="form-control"
    accept="image/jpeg,image/png,image/gif,image/webp"
    (change)="onFileSelected($event)"
    [disabled]="uploading"
  />

  <!-- Upload Progress -->
  <div *ngIf="uploading" class="mt-2">
    <div class="spinner-border spinner-border-sm" role="status">
      <span class="sr-only">Uploading...</span>
    </div>
    <span class="ms-2">Uploading...</span>
  </div>

  <!-- Image Preview -->
  <div *ngIf="product.imageUrl && !uploading" class="mt-3">
    <img
      [src]="product.imageUrl"
      alt="Product Image"
      class="img-thumbnail"
      style="max-width: 200px; max-height: 200px;"
    />
    <button
      type="button"
      class="btn btn-danger btn-sm ms-2"
      (click)="removeImage()">
      <i class="fas fa-trash"></i> Remove
    </button>
  </div>

  <!-- Help Text -->
  <small class="form-text text-muted">
    Allowed formats: JPG, PNG, GIF, WebP. Max size: 5MB
  </small>
</div>
```

## Validation Rules

### File Type
Allowed formats:
- JPEG (`.jpg`, `.jpeg`)
- PNG (`.png`)
- GIF (`.gif`)
- WebP (`.webp`)

### File Size
Maximum file size: **5MB**

### Automatic Validation
The backend automatically validates:
1. File presence
2. File type
3. File size
4. Entity type

## Storage Structure

Images are organized in the following folder structure:

```
wwwroot/
  uploads/
    products/
      a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg
      b2c3d4e5-f6g7-8901-bcde-fg2345678901.png
    categories/
      c3d4e5f6-g7h8-9012-cdef-gh3456789012.jpg
    allergens/
      d4e5f6g7-h8i9-0123-defg-hi4567890123.png
```

### File Naming
- Files are automatically renamed using GUIDs
- Original filenames are discarded for security
- File extension is preserved

## Automatic Cleanup

Images are automatically deleted in the following scenarios:

1. **Entity Deletion**: When a Product or Allergen is deleted
2. **Image Replacement**: When updating an entity's image with a new one
3. **Manual Deletion**: Via the delete endpoint

## Security Features

1. **Authentication Required**: Only authenticated users can upload/delete images
2. **File Type Validation**: Only allowed image formats accepted
3. **File Size Limit**: Maximum 5MB enforced
4. **Secure Naming**: GUID-based names prevent path traversal attacks
5. **Organized Storage**: Separate folders per entity type

## Production Deployment

### Option 1: File System (Current Implementation)

**Pros:**
- Simple setup
- No additional costs
- Fast for small-scale applications

**Cons:**
- Not scalable for distributed systems
- Requires backup strategy
- Limited by disk space

**Recommendations:**
- Set up regular backups
- Monitor disk space
- Consider volume mounting for containers

### Option 2: Cloud Storage (Recommended for Production)

**Azure Blob Storage Example:**

1. Install packages:
```bash
cd src/Masroof.Ecommerce.Application
dotnet add package Volo.Abp.BlobStoring.Azure
```

2. Update `ImageUploadService.cs`:
```csharp
public class ImageUploadService : IImageUploadService
{
    private readonly IBlobContainer _blobContainer;

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        var blobName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using (var stream = file.OpenReadStream())
        {
            await _blobContainer.SaveAsync(blobName, stream);
        }

        return $"https://youraccount.blob.core.windows.net/images/{blobName}";
    }
}
```

3. Configure in `appsettings.json`:
```json
{
  "AbpBlobStoring": {
    "Containers": {
      "default": {
        "ProviderType": "Azure",
        "Azure": {
          "ConnectionString": "your-connection-string",
          "ContainerName": "images"
        }
      }
    }
  }
}
```

**AWS S3 Example:**

Similar approach using `Volo.Abp.BlobStoring.Aws` package.

## Troubleshooting

### Common Issues

**1. 403 Forbidden**
- Ensure user is authenticated
- Check if user has proper permissions

**2. 413 Payload Too Large**
- File exceeds 5MB limit
- Check if server has request size limits configured

**3. 415 Unsupported Media Type**
- File format not allowed
- Ensure Content-Type is `multipart/form-data`

**4. Image not displaying**
- Check if `wwwroot` folder is configured to serve static files
- Verify image URL is correct
- Check browser console for CORS errors

### Server Configuration

Ensure static files are enabled in `Program.cs`:

```csharp
app.UseStaticFiles();
```

For larger files, configure request size limits:

```csharp
services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

## Testing

### Manual Testing

1. Start the backend API
2. Login to get authentication token
3. Use Postman or cURL to test upload:
   ```bash
   curl -X POST "http://localhost:44370/api/app/images/upload" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -F "file=@test-image.jpg" \
     -F "entityType=products"
   ```
4. Verify image appears in `wwwroot/uploads/products/`
5. Test deletion:
   ```bash
   curl -X DELETE "http://localhost:44370/api/app/images/delete?imageUrl=/uploads/products/GUID.jpg" \
     -H "Authorization: Bearer YOUR_TOKEN"
   ```

### Automated Testing

Example unit test:

```csharp
public class ImageUploadService_Tests
{
    [Fact]
    public void IsValidImage_Should_Return_True_For_Valid_Jpeg()
    {
        var service = new ImageUploadService();
        var file = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

        var result = service.IsValidImage(file);

        Assert.True(result);
    }
}
```

## Support

For issues or questions:
1. Check the logs in `Logs/` folder
2. Review the API documentation at `/swagger`
3. Consult the main `ECOMMERCE_IMPLEMENTATION.md` document
