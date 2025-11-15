using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.ShoppingCarts;
using Shouldly;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Masroof.Ecommerce.Tests.Customers;

/// <summary>
/// Tests for automatic Customer and ShoppingCart creation when a new user registers.
/// </summary>
public abstract class CustomerUserCreatedEventHandlerTests<TStartupModule> : EcommerceDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<ShoppingCart, Guid> _shoppingCartRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    protected CustomerUserCreatedEventHandlerTests()
    {
        _identityUserRepository = GetRequiredService<IIdentityUserRepository>();
        _identityUserManager = GetRequiredService<IdentityUserManager>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _shoppingCartRepository = GetRequiredService<IRepository<ShoppingCart, Guid>>();
        _localEventBus = GetRequiredService<ILocalEventBus>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Should_Create_Customer_When_New_User_Is_Created()
    {
        // Arrange
        var userName = "testuser" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";
        var displayName = "Test User";

        IdentityUser user;
        Customer customer;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );
            user.SetProperty("Name", displayName);

            await _identityUserManager.CreateAsync(user);

            // Publish the event (in real scenario, ABP does this automatically)
            await _localEventBus.PublishAsync(
                new EntityCreatedEventData<IdentityUser>(user)
            );
        });

        // Assert - Need to query in a new UOW
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            user.ShouldNotBeNull();

            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);
            customer.ShouldNotBeNull();
            customer.Email.ShouldBe(email);
            customer.FirstName.ShouldBe("Test");
            customer.LastName.ShouldBe("User");
            customer.IsEmailVerified.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Should_Create_ShoppingCart_When_Customer_Is_Created()
    {
        // Arrange
        var userName = "cartuser" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";

        IdentityUser user;
        Customer customer;
        ShoppingCart cart;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );

            await _identityUserManager.CreateAsync(user);

            // Publish the event
            await _localEventBus.PublishAsync(
                new EntityCreatedEventData<IdentityUser>(user)
            );
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);

            customer.ShouldNotBeNull();

            cart = await _shoppingCartRepository.FirstOrDefaultAsync(sc => sc.CustomerId == customer.Id);
            cart.ShouldNotBeNull();
            cart.Items.ShouldBeEmpty();
            cart.ExpiresAt.ShouldNotBeNull();
            cart.ExpiresAt.Value.ShouldBeGreaterThan(DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task Should_Extract_Names_From_Single_Word_Username()
    {
        // Arrange
        var userName = "john" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";

        IdentityUser user;
        Customer customer;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );

            await _identityUserManager.CreateAsync(user);
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);

            customer.ShouldNotBeNull();
            customer.FirstName.ShouldNotBeNullOrEmpty();
            customer.LastName.ShouldBe(string.Empty);
        });
    }

    [Fact]
    public async Task Should_Handle_Multiple_Names_Correctly()
    {
        // Arrange
        var userName = "multiname" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";
        var displayName = "John Michael Doe";

        IdentityUser user;
        Customer customer;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );
            user.SetProperty("Name", displayName);

            await _identityUserManager.CreateAsync(user);
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);

            customer.ShouldNotBeNull();
            customer.FirstName.ShouldBe("John");
            customer.LastName.ShouldBe("Michael Doe");
        });
    }

    [Fact]
    public async Task Should_Set_EmailVerified_When_User_Email_Is_Confirmed()
    {
        // Arrange
        var userName = "verifieduser" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";

        IdentityUser user;
        Customer customer;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );

            await _identityUserManager.CreateAsync(user);

            // Confirm the email using UserManager
            var token = await _identityUserManager.GenerateEmailConfirmationTokenAsync(user);
            await _identityUserManager.ConfirmEmailAsync(user, token);
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);

            customer.ShouldNotBeNull();
            customer.IsEmailVerified.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Should_Set_PhoneNumber_When_Available()
    {
        // Arrange
        var userName = "phoneuser" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";
        var phoneNumber = "+1234567890";

        IdentityUser user;
        Customer customer;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );

            user.SetPhoneNumber(phoneNumber, true);

            await _identityUserManager.CreateAsync(user);
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        // Assert
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            customer = await _customerRepository.FirstOrDefaultAsync(c => c.UserId == user.Id);

            customer.ShouldNotBeNull();
            customer.PhoneNumber.ShouldBe(phoneNumber);
            customer.IsPhoneVerified.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Should_Not_Create_Duplicate_Customer_When_Event_Fires_Multiple_Times()
    {
        // Arrange
        var userName = "dupeuser" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"{userName}@test.com";

        IdentityUser user;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                email,
                _currentTenant.Id
            );

            await _identityUserManager.CreateAsync(user);

            // Publish event twice
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
            await _localEventBus.PublishAsync(new EntityCreatedEventData<IdentityUser>(user));
        });

        // Assert - Should only have one customer
        await WithUnitOfWorkAsync(async () =>
        {
            user = await _identityUserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
            var customers = await _customerRepository.GetListAsync(c => c.UserId == user.Id);

            customers.Count.ShouldBe(1);
        });
    }
}
