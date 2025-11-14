using System.Threading.Tasks;

namespace Masroof.Ecommerce.Data;

public interface IEcommerceDbSchemaMigrator
{
    Task MigrateAsync();
}
