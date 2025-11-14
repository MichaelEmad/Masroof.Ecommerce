using Masroof.Ecommerce.Localization;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce;

/* Inherit your application services from this class.
 */
public abstract class EcommerceAppService : ApplicationService
{
    protected EcommerceAppService()
    {
        LocalizationResource = typeof(EcommerceResource);
    }
}
