using System.Threading.Tasks;
using Masroof.Ecommerce.Images;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace Masroof.Ecommerce.Controllers;

[Area("app")]
[RemoteService(Name = "Ecommerce")]
[Route("api/app/images")]
public class ImageController : EcommerceController
{
    private readonly IImageAppService _imageAppService;

    public ImageController(IImageAppService imageAppService)
    {
        _imageAppService = imageAppService;
    }

    [HttpPost]
    [Route("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ImageUploadResultDto> UploadImageAsync([FromForm] IFormFile file, [FromForm] string entityType)
    {
        return await _imageAppService.UploadImageAsync(file, entityType);
    }

    [HttpDelete]
    [Route("delete")]
    public async Task DeleteImageAsync([FromQuery] string? imageUrl)
    {
        await _imageAppService.DeleteImageAsync(imageUrl);
    }
}
