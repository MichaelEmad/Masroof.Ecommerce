using Volo.Abp.Modularity;

namespace Masroof.Ecommerce;

public abstract class EcommerceApplicationTestBase<TStartupModule> : EcommerceTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
