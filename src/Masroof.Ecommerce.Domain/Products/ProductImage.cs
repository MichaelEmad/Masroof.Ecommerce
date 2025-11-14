using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Products;

public class ProductImage : FullAuditedEntity<Guid>
{
    public Guid ProductId { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    protected ProductImage()
    {
    }

    public ProductImage(
        Guid id,
        Guid productId,
        string blobName,
        string fileName,
        string contentType,
        long sizeInBytes,
        string url,
        int displayOrder = 0,
        bool isPrimary = false
    ) : base(id)
    {
        ProductId = productId;
        BlobName = blobName;
        FileName = fileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        Url = url;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void SetAsSecondary()
    {
        IsPrimary = false;
    }
}
