using System.Threading.Tasks;

namespace EDA.APPLICATION.Interfaces
{
    public interface IDatabaseConfigService
    {
        bool IsConfigured();
        string GetConnectionString();
        void SaveConnectionString(string connectionString);
        Task<bool> TestConnectionAsync(string connectionString);
        string BuildConnectionString(string server, string database, bool useWindowsAuth, string? username = null, string? password = null);
    }
}
