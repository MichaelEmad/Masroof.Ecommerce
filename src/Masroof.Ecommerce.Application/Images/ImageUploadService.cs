using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Masroof.Ecommerce.Images;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task DeleteImageAsync(string imageUrl);
    bool IsValidImage(IFormFile file);
}

public class ImageUploadService : IImageUploadService, ITransientDependency
{
    private readonly string _uploadsFolder = "wwwroot/uploads";
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        if (file.Length > _maxFileSize)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Array.Exists(_allowedExtensions, ext => ext == extension))
        {
            return false;
        }

        return true;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (!IsValidImage(file))
        {
            throw new UserFriendlyException("Invalid image file. Allowed formats: JPG, PNG, GIF, WEBP. Max size: 5MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var folderPath = Path.Combine(_uploadsFolder, folder);

        // Ensure directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/uploads/{folder}/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return Task.CompletedTask;
        }

        try
        {
            // Remove leading slash if present
            var relativePath = imageUrl.TrimStart('/');
            var filePath = Path.Combine("wwwroot", relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore errors during deletion
        }

        return Task.CompletedTask;
    }
}
