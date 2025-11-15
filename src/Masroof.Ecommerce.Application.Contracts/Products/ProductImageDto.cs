using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Products;

public class ProductImageDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
