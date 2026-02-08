namespace EDA.APPLICATION.DTOs
{
    public class DashboardData
    {
        // Resumen general
        public decimal TodaySales { get; set; }
        public decimal WeekSales { get; set; }
        public decimal MonthSales { get; set; }
        public decimal YearSales { get; set; }
        public int TodayInvoices { get; set; }
        public int WeekInvoices { get; set; }
        public int MonthInvoices { get; set; }
        public int YearInvoices { get; set; }

        // Ventas por día (últimos 7 días)
        public List<DailySalesData> Last7DaysSales { get; set; } = new();

        // Ventas por mes (últimos 12 meses)
        public List<MonthlySalesData> Last12MonthsSales { get; set; } = new();

        // Productos más vendidos (top 10)
        public List<TopProductData> TopProducts { get; set; } = new();

        // Familias más vendidas
        public List<TopFamilyData> TopFamilies { get; set; } = new();

        // Ventas por hora del día (para ver horarios pico)
        public List<HourlySalesData> SalesByHour { get; set; } = new();

        // Métodos de pago más usados
        public List<PaymentMethodData> PaymentMethods { get; set; } = new();

        // Últimas facturas
        public List<RecentInvoiceData> RecentInvoices { get; set; } = new();

        // Cuentas por Cobrar - Totales
        public decimal TotalReceivables { get; set; }
        public int PendingInvoicesCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal DueNext7DaysAmount { get; set; }

        // Aging Report
        public List<AgingReportItem> AgingReport { get; set; } = new();

        // Top Facturas Vencidas
        public List<OverdueInvoiceItem> TopOverdueInvoices { get; set; } = new();
    }

    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class MonthlySalesData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class TopProductData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopFamilyData
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class HourlySalesData
    {
        public int Hour { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class PaymentMethodData
    {
        public int PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    public class RecentInvoiceData
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class AgingReportItem
    {
        public string Range { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class OverdueInvoiceItem
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}
