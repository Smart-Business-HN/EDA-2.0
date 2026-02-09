using EDA.APPLICATION.DTOs;
using System.Threading;

namespace EDA.APPLICATION.Interfaces
{
    public interface IUpdateCheckerService
    {
        Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
        string GetCurrentVersion();
    }
}
