using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Images;

public class ImageUploadDto
{
    [Required]
    public string EntityType { get; set; } = string.Empty; // "product", "category", "allergen"
}

public class ImageUploadResultDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
