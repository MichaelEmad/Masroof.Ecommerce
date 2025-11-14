using System;
using System.ComponentModel.DataAnnotations;

namespace Masroof.Ecommerce.Customers;

public class CreateUpdateCustomerDto
{
    [Required]
    [StringLength(128)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }
}
