using Masroof.Ecommerce.Samples;
using Xunit;

namespace Masroof.Ecommerce.EntityFrameworkCore.Domains;

[Collection(EcommerceTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<EcommerceEntityFrameworkCoreTestModule>
{

}
