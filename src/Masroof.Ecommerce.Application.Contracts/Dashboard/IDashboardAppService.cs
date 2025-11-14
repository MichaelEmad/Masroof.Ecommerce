using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Masroof.Ecommerce.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
