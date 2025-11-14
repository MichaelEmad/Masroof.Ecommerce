using System;
using Volo.Abp.Application.Dtos;

namespace Masroof.Ecommerce.Addresses;

public class AddressDto : FullAuditedEntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public AddressType AddressType { get; set; }
    public string FormattedAddress { get; set; } = string.Empty;
}

public enum AddressType
{
    Shipping = 1,
    Billing = 2,
    Both = 3
}
