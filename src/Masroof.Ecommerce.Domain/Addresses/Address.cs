using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Addresses;

public class Address : FullAuditedAggregateRoot<Guid>
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

    protected Address()
    {
    }

    public Address(
        Guid id,
        Guid customerId,
        string fullName,
        string phoneNumber,
        string addressLine1,
        string city,
        string state,
        string postalCode,
        string country,
        AddressType addressType = AddressType.Shipping
    ) : base(id)
    {
        CustomerId = customerId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        AddressLine1 = addressLine1;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
        AddressType = addressType;
        IsDefault = false;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
    }

    public string GetFormattedAddress()
    {
        var line2 = string.IsNullOrWhiteSpace(AddressLine2) ? "" : $", {AddressLine2}";
        return $"{AddressLine1}{line2}, {City}, {State} {PostalCode}, {Country}";
    }
}

public enum AddressType
{
    Shipping = 1,
    Billing = 2,
    Both = 3
}
