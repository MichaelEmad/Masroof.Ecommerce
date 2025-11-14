using Volo.Abp.Modularity;

namespace Masroof.Ecommerce;

[DependsOn(
    typeof(EcommerceDomainModule),
    typeof(EcommerceTestBaseModule)
)]
public class EcommerceDomainTestModule : AbpModule
{

}
