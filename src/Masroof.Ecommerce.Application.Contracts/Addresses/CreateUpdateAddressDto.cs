using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Addresses;

public class CreateUpdateAddressDto
{
    [Required]
    [StringLength(128)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(256)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    [Required]
    public AddressType AddressType { get; set; }
}
