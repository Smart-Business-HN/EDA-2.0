using EDA.APPLICATION;
using EDA.DOMAIN.Entities;
using EDA.INFRAESTRUCTURE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace EDA_2._0
{
    public partial class App : Application
    {
        private static MainWindow? _mainWindow;
        private static IServiceProvider? _services;
        private static IConfiguration? _configuration;

        public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not configured");
        public static IConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration not loaded");
        public static MainWindow MainWindow => _mainWindow ?? throw new InvalidOperationException("MainWindow not initialized");
        public static User? CurrentUser { get; set; }
        public static Shift? CurrentShift { get; set; }

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
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Aplicar migraciones pendientes automáticamente
            using (var scope = Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.Migrate();
            }

            _mainWindow = new MainWindow();
            _mainWindow.Activate();
        }
    }
}
