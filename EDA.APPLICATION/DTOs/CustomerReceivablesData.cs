namespace EDA.APPLICATION.DTOs
{
    /// <summary>
    /// DTO para listar clientes deudores en ReceivablesPage
    /// </summary>
    public class DebtorSummary
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerRtn { get; set; }
        public decimal TotalOwed { get; set; }
        public int PendingInvoicesCount { get; set; }
        public decimal OverdueAmount { get; set; }
    }

    /// <summary>
    /// DTO para el detalle de cuentas por cobrar de un cliente
    /// </summary>
    public class CustomerReceivablesDetail
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerRtn { get; set; }
        public string? CustomerCompany { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal TotalOwed { get; set; }
        public int PendingInvoicesCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal DueIn7DaysAmount { get; set; }
        public List<PendingInvoiceItem> PendingInvoices { get; set; } = new();
    }

    /// <summary>
    /// Item de factura pendiente
    /// </summary>
    public class PendingInvoiceItem
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Total { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int DaysOverdue { get; set; }
        public bool IsOverdue { get; set; }
    }

    /// <summary>
    /// Datos para el PDF de Estado de Cuenta
    /// </summary>
    public class CustomerStatementPdfData
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyRtn { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public byte[]? CompanyLogo { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerRtn { get; set; }
        public string? CustomerCompany { get; set; }
        public DateTime GeneratedAt { get; set; }
        public decimal TotalOwed { get; set; }
        public int PendingInvoicesCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public List<StatementInvoiceItem> Invoices { get; set; } = new();
    }

    /// <summary>
    /// Item de factura para el estado de cuenta PDF
    /// </summary>
    public class StatementInvoiceItem
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Total { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
}
