using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Masroof.Ecommerce.Customers;

public class Customer : FullAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public string? CustomerNotes { get; set; }

    protected Customer()
    {
    }

    public Customer(
        Guid id,
        Guid userId,
        string firstName,
        string lastName,
        string email
    ) : base(id)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsEmailVerified = false;
        IsPhoneVerified = false;
        TotalOrders = 0;
        TotalSpent = 0;
    }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }

    public void VerifyPhone()
    {
        IsPhoneVerified = true;
    }

    public void RecordLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }

    public void IncrementOrderStats(decimal orderTotal)
    {
        TotalOrders++;
        TotalSpent += orderTotal;
    }

    public bool IsVipCustomer()
    {
        return TotalSpent >= 1000 || TotalOrders >= 10;
    }
}
