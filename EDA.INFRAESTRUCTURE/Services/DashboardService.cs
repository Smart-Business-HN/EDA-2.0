using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DatabaseContext _context;

        public DashboardService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<DashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);
            var last7Days = today.AddDays(-6);
            var last12Months = today.AddMonths(-11);

            var invoices = _context.Set<Invoice>();
            var soldProducts = _context.Set<SoldProduct>();
            var invoicePayments = _context.Set<InvoicePayment>();

            // Resumen de ventas
            var todaySales = await invoices
                .Where(i => i.Date.Date == today)
                .SumAsync(i => i.Total, cancellationToken);

            var weekSales = await invoices
                .Where(i => i.Date.Date >= startOfWeek)
                .SumAsync(i => i.Total, cancellationToken);

            var monthSales = await invoices
                .Where(i => i.Date.Date >= startOfMonth)
                .SumAsync(i => i.Total, cancellationToken);

            var yearSales = await invoices
                .Where(i => i.Date.Date >= startOfYear)
                .SumAsync(i => i.Total, cancellationToken);

            // Conteo de facturas
            var todayInvoices = await invoices.CountAsync(i => i.Date.Date == today, cancellationToken);
            var weekInvoices = await invoices.CountAsync(i => i.Date.Date >= startOfWeek, cancellationToken);
            var monthInvoices = await invoices.CountAsync(i => i.Date.Date >= startOfMonth, cancellationToken);
            var yearInvoices = await invoices.CountAsync(i => i.Date.Date >= startOfYear, cancellationToken);

            // Ventas últimos 7 días
            var last7DaysSales = await invoices
                .Where(i => i.Date.Date >= last7Days)
                .GroupBy(i => i.Date.Date)
                .Select(g => new DailySalesData
                {
                    Date = g.Key,
                    Total = g.Sum(i => i.Total),
                    InvoiceCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync(cancellationToken);

            // Completar los días que faltan con 0
            var allDays = Enumerable.Range(0, 7)
                .Select(i => last7Days.AddDays(i))
                .ToList();

            var dayNames = new[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };
            var completeLast7Days = allDays.Select(d =>
            {
                var existing = last7DaysSales.FirstOrDefault(s => s.Date == d);
                return new DailySalesData
                {
                    Date = d,
                    DayName = dayNames[(int)d.DayOfWeek],
                    Total = existing?.Total ?? 0,
                    InvoiceCount = existing?.InvoiceCount ?? 0
                };
            }).ToList();

            // Ventas últimos 12 meses
            var monthNames = new[] { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            var last12MonthsSales = await invoices
                .Where(i => i.Date >= last12Months.AddDays(-last12Months.Day + 1))
                .GroupBy(i => new { i.Date.Year, i.Date.Month })
                .Select(g => new MonthlySalesData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(i => i.Total),
                    InvoiceCount = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync(cancellationToken);

            // Completar meses con nombres
            foreach (var m in last12MonthsSales)
            {
                m.MonthName = monthNames[m.Month];
            }

            // Top 10 productos más vendidos (último mes)
            var topProducts = await (
                from sp in soldProducts
                join i in invoices on sp.InvoiceId equals i.Id
                join p in _context.Set<Product>() on sp.ProductId equals p.Id
                where i.Date >= startOfMonth
                group sp by new { sp.ProductId, p.Name } into g
                select new TopProductData
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(sp => sp.Quantity),
                    TotalRevenue = g.Sum(sp => sp.TotalLine)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToListAsync(cancellationToken);

            // Top familias (último mes)
            var topFamilies = await (
                from sp in soldProducts
                join i in invoices on sp.InvoiceId equals i.Id
                join p in _context.Set<Product>() on sp.ProductId equals p.Id
                join f in _context.Set<Family>() on p.FamilyId equals f.Id
                where i.Date >= startOfMonth
                group sp by new { p.FamilyId, f.Name } into g
                select new TopFamilyData
                {
                    FamilyId = g.Key.FamilyId,
                    FamilyName = g.Key.Name,
                    QuantitySold = g.Sum(sp => sp.Quantity),
                    TotalRevenue = g.Sum(sp => sp.TotalLine)
                })
                .OrderByDescending(f => f.TotalRevenue)
                .Take(5)
                .ToListAsync(cancellationToken);

            // Ventas por hora (último mes)
            var salesByHour = await invoices
                .Where(i => i.Date >= startOfMonth)
                .GroupBy(i => i.Date.Hour)
                .Select(g => new HourlySalesData
                {
                    Hour = g.Key,
                    Total = g.Sum(i => i.Total),
                    InvoiceCount = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToListAsync(cancellationToken);

            // Agregar etiquetas de hora
            foreach (var h in salesByHour)
            {
                h.HourLabel = $"{h.Hour:D2}:00";
            }

            // Métodos de pago (último mes)
            var paymentMethods = await invoicePayments
                .Include(ip => ip.PaymentType)
                .Include(ip => ip.Invoice)
                .Where(ip => ip.Invoice != null && ip.Invoice.Date >= startOfMonth)
                .GroupBy(ip => new { ip.PaymentTypeId, PaymentTypeName = ip.PaymentType!.Name })
                .Select(g => new PaymentMethodData
                {
                    PaymentTypeId = g.Key.PaymentTypeId,
                    PaymentTypeName = g.Key.PaymentTypeName,
                    TotalAmount = g.Sum(ip => ip.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(pm => pm.TotalAmount)
                .ToListAsync(cancellationToken);

            // Últimas 10 facturas
            var recentInvoices = await invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.Date)
                .Take(10)
                .Select(i => new RecentInvoiceData
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    Date = i.Date,
                    CustomerName = i.Customer != null ? i.Customer.Name : "Cliente",
                    Total = i.Total
                })
                .ToListAsync(cancellationToken);

            return new DashboardData
            {
                TodaySales = todaySales,
                WeekSales = weekSales,
                MonthSales = monthSales,
                YearSales = yearSales,
                TodayInvoices = todayInvoices,
                WeekInvoices = weekInvoices,
                MonthInvoices = monthInvoices,
                YearInvoices = yearInvoices,
                Last7DaysSales = completeLast7Days,
                Last12MonthsSales = last12MonthsSales,
                TopProducts = topProducts,
                TopFamilies = topFamilies,
                SalesByHour = salesByHour,
                PaymentMethods = paymentMethods,
                RecentInvoices = recentInvoices
            };
        }
    }
}
