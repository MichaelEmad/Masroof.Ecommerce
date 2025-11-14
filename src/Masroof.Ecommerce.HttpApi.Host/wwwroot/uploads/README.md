# Image Upload Folders

This directory contains uploaded images for the e-commerce application.

## Folder Structure

- `products/` - Product images
- `categories/` - Category images
- `allergens/` - Allergen icon images

## Upload Specifications

### Allowed File Types
- JPEG (.jpg, .jpeg)
- PNG (.png)
- GIF (.gif)
- WebP (.webp)

### File Size Limit
- Maximum file size: 5MB

### Image Naming
Images are automatically renamed using a GUID to prevent conflicts:
- Format: `{GUID}.{extension}`
- Example: `a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg`

## API Usage

### Upload Image
**Endpoint:** POST /api/app/images/upload
**Content-Type:** multipart/form-data
**Parameters:**
- file: The image file
- entityType: Entity type (products, categories, or allergens)

### Delete Image
**Endpoint:** DELETE /api/app/images/delete?imageUrl={imageUrl}

## Automatic Cleanup
Images are automatically deleted when:
- The associated entity is deleted
- An entity's image is replaced with a new one

## Security
- Only authenticated users can upload images
- File type and size validation enforced
- Images stored in separate folders per entity type
