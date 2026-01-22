using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    }
}
