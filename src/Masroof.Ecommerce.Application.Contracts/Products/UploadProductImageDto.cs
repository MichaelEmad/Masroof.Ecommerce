using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Products;

public class UploadProductImageDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty; // Base64 encoded content
}
