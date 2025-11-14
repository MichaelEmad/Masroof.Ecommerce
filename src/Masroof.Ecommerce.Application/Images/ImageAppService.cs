using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Images;

public class ImageAppService : ApplicationService, IImageAppService
{
    private readonly IImageUploadService _imageUploadService;

    public ImageAppService(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string entityType)
    {
        // Validate entity type
        var validTypes = new[] { "products", "categories", "allergens" };
        if (!Array.Exists(validTypes, t => t.Equals(entityType, StringComparison.OrdinalIgnoreCase)))
        {
            throw new UserFriendlyException($"Invalid entity type. Allowed types: {string.Join(", ", validTypes)}");
        }

        var imageUrl = await _imageUploadService.UploadImageAsync(file, entityType.ToLowerInvariant());

        return new ImageUploadResultDto
        {
            ImageUrl = imageUrl,
            FileName = Path.GetFileName(imageUrl)
        };
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        await _imageUploadService.DeleteImageAsync(imageUrl);
    }
}
