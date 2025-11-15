using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Products;

[Authorize(EcommercePermissions.Products.Default)]
public class ProductImageAppService : ApplicationService, IProductImageAppService
{
    private readonly IRepository<ProductImage, Guid> _productImageRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IBlobContainer<ProductImageContainer> _blobContainer;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedContentTypes = {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public ProductImageAppService(
        IRepository<ProductImage, Guid> productImageRepository,
        IRepository<Product, Guid> productRepository,
        IBlobContainer<ProductImageContainer> blobContainer)
    {
        _productImageRepository = productImageRepository;
        _productRepository = productRepository;
        _blobContainer = blobContainer;
    }

    [Authorize(EcommercePermissions.Products.Edit)]
    public async Task<ProductImageDto> UploadImageAsync(UploadProductImageDto input)
    {
        // Validate product exists
        var product = await _productRepository.GetAsync(input.ProductId);

        // Decode Base64 content
        byte[] contentBytes;
        try
        {
            contentBytes = Convert.FromBase64String(input.Content);
        }
        catch (FormatException)
        {
            throw new UserFriendlyException("Invalid file content format");
        }

        // Validate file size
        if (contentBytes.Length > MaxFileSize)
        {
            throw new UserFriendlyException($"File size cannot exceed {MaxFileSize / 1024 / 1024}MB");
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(input.ContentType.ToLower()))
        {
            throw new UserFriendlyException($"Invalid file type. Allowed types: {string.Join(", ", AllowedContentTypes)}");
        }

        // Generate unique blob name
        var fileExtension = Path.GetExtension(input.FileName);
        var blobName = $"{input.ProductId}/{Guid.NewGuid()}{fileExtension}";

        // Save to blob storage
        await _blobContainer.SaveAsync(blobName, contentBytes, overrideExisting: true);

        // Get the blob URL (for now, we'll construct it, but in production you might use a CDN)
        var url = $"/api/app/product-image/blob/{blobName}";

        // Determine display order (add to the end)
        var existingImages = await _productImageRepository.GetListAsync(x => x.ProductId == input.ProductId);
        var maxDisplayOrder = existingImages.Any() ? existingImages.Max(x => x.DisplayOrder) : -1;

        // Create product image entity
        var productImage = new ProductImage(
            GuidGenerator.Create(),
            input.ProductId,
            blobName,
            input.FileName,
            input.ContentType,
            contentBytes.Length,
            url,
            maxDisplayOrder + 1,
            isPrimary: !existingImages.Any() // First image is primary by default
        );

        await _productImageRepository.InsertAsync(productImage);

        // If this is the first image, update the product's ImageUrl
        if (!existingImages.Any())
        {
            product.ImageUrl = url;
            await _productRepository.UpdateAsync(product);
        }

        return ObjectMapper.Map<ProductImage, ProductImageDto>(productImage);
    }

    [Authorize(EcommercePermissions.Products.Edit)]
    public async Task DeleteImageAsync(Guid id)
    {
        var productImage = await _productImageRepository.GetAsync(id);
        var productId = productImage.ProductId;

        // Delete from blob storage
        await _blobContainer.DeleteAsync(productImage.BlobName);

        // Delete from database
        await _productImageRepository.DeleteAsync(id);

        // If this was the primary image, set another image as primary
        if (productImage.IsPrimary)
        {
            var remainingImages = await _productImageRepository.GetListAsync(x => x.ProductId == productId);
            if (remainingImages.Any())
            {
                var newPrimary = remainingImages.OrderBy(x => x.DisplayOrder).First();
                newPrimary.SetAsPrimary();
                await _productImageRepository.UpdateAsync(newPrimary);

                // Update product's ImageUrl
                var product = await _productRepository.GetAsync(productId);
                product.ImageUrl = newPrimary.Url;
                await _productRepository.UpdateAsync(product);
            }
            else
            {
                // No images left, clear product's ImageUrl
                var product = await _productRepository.GetAsync(productId);
                product.ImageUrl = null;
                await _productRepository.UpdateAsync(product);
            }
        }
    }

    [Authorize(EcommercePermissions.Products.Edit)]
    public async Task SetPrimaryImageAsync(Guid id)
    {
        var productImage = await _productImageRepository.GetAsync(id);
        var productId = productImage.ProductId;

        // Get all images for this product
        var allImages = await _productImageRepository.GetListAsync(x => x.ProductId == productId);

        // Set all images as secondary
        foreach (var image in allImages)
        {
            if (image.IsPrimary)
            {
                image.SetAsSecondary();
                await _productImageRepository.UpdateAsync(image);
            }
        }

        // Set the selected image as primary
        productImage.SetAsPrimary();
        await _productImageRepository.UpdateAsync(productImage);

        // Update product's ImageUrl
        var product = await _productRepository.GetAsync(productId);
        product.ImageUrl = productImage.Url;
        await _productRepository.UpdateAsync(product);
    }

    [AllowAnonymous]
    public async Task<List<ProductImageDto>> GetProductImagesAsync(Guid productId)
    {
        var images = await _productImageRepository.GetListAsync(x => x.ProductId == productId);
        var orderedImages = images.OrderBy(x => x.DisplayOrder).ToList();

        return ObjectMapper.Map<List<ProductImage>, List<ProductImageDto>>(orderedImages);
    }

    [Authorize(EcommercePermissions.Products.Edit)]
    public async Task UpdateDisplayOrderAsync(Guid imageId, int displayOrder)
    {
        var productImage = await _productImageRepository.GetAsync(imageId);

        // Update display order
        var allImages = await _productImageRepository.GetListAsync(x => x.ProductId == productImage.ProductId);

        // Simple approach: just update this image's order
        // In a more complex scenario, you might want to reorder all images
        productImage.DisplayOrder = displayOrder;
        await _productImageRepository.UpdateAsync(productImage);
    }

    /// <summary>
    /// Gets the blob content for download/display
    /// This method should be exposed via HTTP controller
    /// </summary>
    [AllowAnonymous]
    public async Task<byte[]> GetBlobAsync(string blobName)
    {
        return await _blobContainer.GetAllBytesOrNullAsync(blobName)
            ?? throw new UserFriendlyException("Image not found");
    }
}

/// <summary>
/// Blob container for product images
/// </summary>
[BlobContainerName(EcommerceConsts.ProductImageContainerName)]
public class ProductImageContainer
{
}
