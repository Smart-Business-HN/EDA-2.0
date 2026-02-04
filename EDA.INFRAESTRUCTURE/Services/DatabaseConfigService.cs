using EDA.APPLICATION.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class DatabaseConfigService : IDatabaseConfigService
    {
        private readonly string _appSettingsPath;
        private readonly IConfiguration _configuration;

        public DatabaseConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        }

        public bool IsConfigured()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            return !string.IsNullOrWhiteSpace(connectionString);
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public void SaveConnectionString(string connectionString)
        {
            try
            {
                var json = File.ReadAllText(_appSettingsPath);
                var jsonNode = JsonNode.Parse(json);

                if (jsonNode == null)
                {
                    jsonNode = new JsonObject();
                }

                if (jsonNode["ConnectionStrings"] == null)
                {
                    jsonNode["ConnectionStrings"] = new JsonObject();
                }

                jsonNode["ConnectionStrings"]!["DefaultConnection"] = connectionString;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var updatedJson = jsonNode.ToJsonString(options);
                File.WriteAllText(_appSettingsPath, updatedJson);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al guardar configuracion: {ex.Message}", ex);
            }
        }

        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string BuildConnectionString(string server, string database, bool useWindowsAuth, string? username = null, string? password = null)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                TrustServerCertificate = true
            };

            if (useWindowsAuth)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = username ?? string.Empty;
                builder.Password = password ?? string.Empty;
            }

            return builder.ConnectionString;
        }
    }
}
