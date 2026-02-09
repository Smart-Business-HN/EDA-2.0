using EDA.APPLICATION.Interfaces;
using EDA.APPLICATION.Repository;
using EDA.INFRAESTRUCTURE.Repository;
using EDA.INFRAESTRUCTURE.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EDA.INFRAESTRUCTURE
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Servicio de configuracion de base de datos (siempre disponible)
            services.AddSingleton<IDatabaseConfigService, DatabaseConfigService>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Solo registrar DbContext si hay una conexion configurada
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDbContext<DatabaseContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddScoped(typeof(IRepositoryAsync<>), typeof(CustomRepositoryAsync<>));

                // Servicios de Dashboard
                services.AddScoped<IDashboardService, DashboardService>();
            }

            // Servicios de PDF (siempre disponibles)
            services.AddTransient<IInvoicePdfService, InvoicePdfService>();
            services.AddTransient<IShiftReportPdfService, ShiftReportPdfService>();
            services.AddTransient<IReportPdfService, ReportPdfService>();
            services.AddTransient<ICustomerStatementPdfService, CustomerStatementPdfService>();

            // Servicio de actualizaciones
            services.AddSingleton<IUpdateCheckerService, UpdateCheckerService>();

            // Servicio de descubrimiento de impresoras
            services.AddSingleton<IPrinterDiscoveryService, PrinterDiscoveryService>();

            return services;
        }
    }
}
