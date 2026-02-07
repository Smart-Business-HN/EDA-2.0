using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IUpdateCheckerService
    {
        Task<UpdateCheckResult> CheckForUpdatesAsync();
        string GetCurrentVersion();
    }
}
