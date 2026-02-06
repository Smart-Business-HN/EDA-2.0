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

        public async Task<bool> TestServerConnectionAsync(string connectionString)
        {
            try
            {
                // Modificar el connection string para conectar a master en lugar de la DB especificada
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";

                using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnsureDatabaseExistsAsync(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return false;
            }

            // Conectar a master para crear la base de datos
            builder.InitialCatalog = "master";
            builder.ConnectTimeout = 30; // Aumentar timeout

            // Reintentar hasta 3 veces (LocalDB puede tardar en iniciarse)
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    using var connection = new SqlConnection(builder.ConnectionString);
                    await connection.OpenAsync();

                    // Verificar si la base de datos existe
                    var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = @dbName";
                    using var checkCmd = new SqlCommand(checkDbQuery, connection);
                    checkCmd.Parameters.AddWithValue("@dbName", databaseName);
                    var result = await checkCmd.ExecuteScalarAsync();

                    if (result == null)
                    {
                        // Crear la base de datos si no existe
                        var createDbQuery = $"CREATE DATABASE [{databaseName}]";
                        using var createCmd = new SqlCommand(createDbQuery, connection);
                        await createCmd.ExecuteNonQueryAsync();
                    }

                    return true;
                }
                catch
                {
                    if (attempt < 3)
                    {
                        // Esperar antes de reintentar
                        await Task.Delay(2000);
                    }
                }
            }

            return false;
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
