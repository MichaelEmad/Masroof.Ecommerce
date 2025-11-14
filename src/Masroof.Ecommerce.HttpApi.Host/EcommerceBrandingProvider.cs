using Microsoft.Extensions.Localization;
using Masroof.Ecommerce.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Masroof.Ecommerce;

[Dependency(ReplaceServices = true)]
public class EcommerceBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EcommerceResource> _localizer;

    public EcommerceBrandingProvider(IStringLocalizer<EcommerceResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
