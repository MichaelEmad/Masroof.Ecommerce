using Masroof.Ecommerce.Tests.Customers;
using Xunit;

namespace Masroof.Ecommerce.EntityFrameworkCore.Customers;

[Collection(EcommerceTestConsts.CollectionDefinitionName)]
public class EfCoreCustomerUserCreatedEventHandlerTests : CustomerUserCreatedEventHandlerTests<EcommerceEntityFrameworkCoreTestModule>
{
    // All tests are inherited from CustomerUserCreatedEventHandlerTests
}
