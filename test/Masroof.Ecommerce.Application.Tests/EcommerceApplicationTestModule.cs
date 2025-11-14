using Volo.Abp.Modularity;

namespace Masroof.Ecommerce;

[DependsOn(
    typeof(EcommerceApplicationModule),
    typeof(EcommerceDomainTestModule)
)]
public class EcommerceApplicationTestModule : AbpModule
{

}
