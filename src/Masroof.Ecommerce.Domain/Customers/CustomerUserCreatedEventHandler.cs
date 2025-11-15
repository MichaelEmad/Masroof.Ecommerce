using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.ShoppingCarts;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Masroof.Ecommerce.Customers;

/// <summary>
/// Event handler that automatically creates a Customer entity and ShoppingCart
/// when a new user registers through ABP Identity.
/// </summary>
public class CustomerUserCreatedEventHandler : ILocalEventHandler<EntityCreatedEventData<IdentityUser>>, ITransientDependency
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<ShoppingCart, Guid> _shoppingCartRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<CustomerUserCreatedEventHandler> _logger;

    public CustomerUserCreatedEventHandler(
        IRepository<Customer, Guid> customerRepository,
        IRepository<ShoppingCart, Guid> shoppingCartRepository,
        IGuidGenerator guidGenerator,
        ILogger<CustomerUserCreatedEventHandler> logger)
    {
        _customerRepository = customerRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<IdentityUser> eventData)
    {
        var user = eventData.Entity;

        try
        {
            _logger.LogInformation("Creating Customer entity for new user: {UserId} ({UserName})", user.Id, user.UserName);

            // Check if Customer already exists (in case of duplicate events)
            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (existingCustomer != null)
            {
                _logger.LogWarning("Customer entity already exists for user: {UserId}", user.Id);
                return;
            }

            // Extract first name and last name from user name
            var (firstName, lastName) = ExtractNames(user.Name, user.UserName);

            // Create Customer entity
            var customerId = _guidGenerator.Create();
            var customer = new Customer(
                customerId,
                user.Id,
                firstName,
                lastName,
                user.Email ?? string.Empty
            );

            // Set phone number if available
            customer.PhoneNumber = user.PhoneNumber;

            // Mark email as verified if user's email is confirmed
            if (user.EmailConfirmed)
            {
                customer.VerifyEmail();
            }

            // Mark phone as verified if user's phone is confirmed
            if (user.PhoneNumberConfirmed)
            {
                customer.VerifyPhone();
            }

            // Insert the customer
            await _customerRepository.InsertAsync(customer, autoSave: true);

            _logger.LogInformation("Successfully created Customer entity for user: {UserId} with CustomerId: {CustomerId}", user.Id, customerId);

            // Create default ShoppingCart for the customer
            await CreateDefaultShoppingCartAsync(customerId, user.Id);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - we don't want to fail user registration
            _logger.LogError(
                ex,
                "Failed to create Customer entity for user: {UserId}. User registration will proceed, but customer profile must be created manually.",
                user.Id
            );
        }
    }

    private async Task CreateDefaultShoppingCartAsync(Guid customerId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Creating default ShoppingCart for customer: {CustomerId}", customerId);

            // Check if cart already exists
            var existingCart = await _shoppingCartRepository.FirstOrDefaultAsync(sc => sc.CustomerId == customerId);
            if (existingCart != null)
            {
                _logger.LogWarning("ShoppingCart already exists for customer: {CustomerId}", customerId);
                return;
            }

            var cartId = _guidGenerator.Create();
            var shoppingCart = new ShoppingCart(cartId, customerId);

            await _shoppingCartRepository.InsertAsync(shoppingCart, autoSave: true);

            _logger.LogInformation("Successfully created ShoppingCart for customer: {CustomerId} with CartId: {CartId}", customerId, cartId);
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            _logger.LogError(
                ex,
                "Failed to create ShoppingCart for customer: {CustomerId}. Cart can be created later when user first adds items.",
                customerId
            );
        }
    }

    /// <summary>
    /// Extracts first name and last name from user's display name or username.
    /// </summary>
    /// <param name="displayName">User's display name (e.g., "John Doe")</param>
    /// <param name="userName">User's username (fallback if displayName is null/empty)</param>
    /// <returns>Tuple of (FirstName, LastName)</returns>
    private (string firstName, string lastName) ExtractNames(string? displayName, string? userName)
    {
        // Use display name if available, otherwise fall back to username
        var fullName = !string.IsNullOrWhiteSpace(displayName) ? displayName : userName;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ("User", string.Empty);
        }

        // Split by space and extract first/last names
        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length == 0)
        {
            return ("User", string.Empty);
        }
        else if (nameParts.Length == 1)
        {
            // Single name - use as first name
            return (nameParts[0], string.Empty);
        }
        else if (nameParts.Length == 2)
        {
            // Two names - first and last
            return (nameParts[0], nameParts[1]);
        }
        else
        {
            // More than two names - first name is first part, last name is everything else
            var firstName = nameParts[0];
            var lastName = string.Join(" ", nameParts[1..]);
            return (firstName, lastName);
        }
    }
}
