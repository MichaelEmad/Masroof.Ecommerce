using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Images;

public interface IImageAppService : IApplicationService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string entityType);
    Task DeleteImageAsync(string? imageUrl);
}
