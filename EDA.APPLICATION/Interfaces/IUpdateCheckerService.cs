using EDA.APPLICATION.DTOs;
using System.Threading.Tasks;

namespace EDA.APPLICATION.Interfaces
{
    public interface IUpdateCheckerService
    {
        Task<UpdateCheckResult> CheckForUpdatesAsync();
        string GetCurrentVersion();
    }
}
