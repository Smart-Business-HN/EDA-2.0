using EDA.APPLICATION;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using EDA.INFRAESTRUCTURE;
using EDA_2._0.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EDA_2._0
{
    public partial class App : Application
    {
        private static MainWindow? _mainWindow;
        private static IServiceProvider? _services;
        private static IConfiguration? _configuration;
        private static bool _isDatabaseConfigured;

        public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not configured");
        public static IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration not loaded");
        public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("MainWindow not initialized");
        public static User? CurrentUser { get; set; }
        public static Shift? CurrentShift { get; set; }
        public static bool IsDatabaseConfigured => _isDatabaseConfigured;

        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var basePath = AppContext.BaseDirectory;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton(_configuration);
            services.AddApplicationServices(_configuration);
            services.AddInfrastructureServices(_configuration);

            _services = services.BuildServiceProvider();

            // Verificar si la base de datos esta configurada
            var dbConfigService = _services.GetRequiredService<IDatabaseConfigService>();
            _isDatabaseConfigured = dbConfigService.IsConfigured();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _mainWindow = new MainWindow();

            if (_isDatabaseConfigured)
            {
                try
                {
                    var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? "";

                    // Paso 1: Iniciar LocalDB si está configurado
                    if (connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase))
                    {
                        StartLocalDB();
                        // Esperar a que LocalDB se inicie completamente (puede tardar la primera vez)
                        await Task.Delay(3000);
                    }

                    // Paso 2: Asegurar que la base de datos existe
                    var dbConfigService = Services.GetRequiredService<IDatabaseConfigService>();
                    var dbCreated = await dbConfigService.EnsureDatabaseExistsAsync(connectionString);

                    if (!dbCreated)
                    {
                        System.Diagnostics.Debug.WriteLine("Could not create database");
                        _mainWindow.NavigateToPage(typeof(DatabaseSetupPage));
                        _mainWindow.Activate();
                        return;
                    }

                    // Paso 3: Aplicar migraciones pendientes automaticamente
                    using (var scope = Services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        await dbContext.Database.MigrateAsync();
                    }

                    // Mostrar pagina de login
                    _mainWindow.NavigateToPage(typeof(LoginPage));
                }
                catch (Exception ex)
                {
                    // Si falla la conexión, mostrar wizard de configuración
                    System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                    _mainWindow.NavigateToPage(typeof(DatabaseSetupPage));
                }
            }
            else
            {
                // Mostrar wizard de configuracion de base de datos
                _mainWindow.NavigateToPage(typeof(DatabaseSetupPage));
            }

            _mainWindow.Activate();
        }

        private static void StartLocalDB()
        {
            try
            {
                // Buscar SqlLocalDB.exe en diferentes versiones de SQL Server
                var sqlLocalDbPath = FindSqlLocalDBPath();

                if (!string.IsNullOrEmpty(sqlLocalDbPath) && File.Exists(sqlLocalDbPath))
                {
                    // Crear instancia si no existe
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sqlLocalDbPath,
                        Arguments = "create MSSQLLocalDB",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    })?.WaitForExit(10000);

                    // Iniciar instancia
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sqlLocalDbPath,
                        Arguments = "start MSSQLLocalDB",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    })?.WaitForExit(10000);
                }
            }
            catch { /* LocalDB might already be running or not installed */ }
        }

        private static string? FindSqlLocalDBPath()
        {
            // Versiones de SQL Server a buscar (de más nueva a más antigua)
            string[] versions = { "170", "160", "150", "140", "130" };
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            foreach (var version in versions)
            {
                var path = Path.Combine(programFiles, "Microsoft SQL Server", version, "Tools", "Binn", "SqlLocalDB.exe");
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
