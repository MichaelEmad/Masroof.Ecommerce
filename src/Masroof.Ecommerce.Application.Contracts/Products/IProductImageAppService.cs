using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Products;

public interface IProductImageAppService : IApplicationService
{
    /// <summary>
    /// Uploads a new image for a product
    /// </summary>
    /// <param name="input">Upload image DTO containing file data</param>
    /// <returns>The created product image with URL</returns>
    Task<ProductImageDto> UploadImageAsync(UploadProductImageDto input);

    /// <summary>
    /// Deletes a product image
    /// </summary>
    /// <param name="id">Image ID to delete</param>
    Task DeleteImageAsync(Guid id);

    /// <summary>
    /// Sets an image as the primary image for a product
    /// </summary>
    /// <param name="id">Image ID to set as primary</param>
    Task SetPrimaryImageAsync(Guid id);

    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product images ordered by display order</returns>
    Task<List<ProductImageDto>> GetProductImagesAsync(Guid productId);

    /// <summary>
    /// Updates the display order of images
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <param name="displayOrder">New display order</param>
    Task UpdateDisplayOrderAsync(Guid imageId, int displayOrder);

    /// <summary>
    /// Gets the blob content for a specific image
    /// </summary>
    /// <param name="blobName">The blob name</param>
    /// <returns>The blob content as byte array</returns>
    Task<byte[]> GetBlobAsync(string blobName);
}
