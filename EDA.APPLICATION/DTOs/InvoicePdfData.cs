namespace EDA.APPLICATION.DTOs
{
    /// <summary>
    /// Datos completos para generar el PDF de una factura.
    /// </summary>
    public class InvoicePdfData
    {
        // Datos de la Empresa
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyOwner { get; set; } = string.Empty;
        public string? CompanyAddress1 { get; set; }
        public string? CompanyAddress2 { get; set; }
        public string? CompanyRtn { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }

        // Datos de la Factura
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Subtotal { get; set; }
        public double TotalDiscounts { get; set; }
        public double TotalTaxes { get; set; }
        public decimal Total { get; set; }
        public decimal TaxedAt15Percent { get; set; }
        public decimal TaxesAt15Percent { get; set; }
        public decimal TaxedAt18Percent { get; set; }
        public decimal TaxesAt18Percent { get; set; }
        public decimal Exempt { get; set; }
        public double? CashReceived { get; set; }
        public double? ChangeGiven { get; set; }

        // Datos del Cliente
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerRtn { get; set; }
        public string? CustomerAddress { get; set; }

        // Datos del CAI
        public string CaiNumber { get; set; } = string.Empty;
        public DateTime CaiFromDate { get; set; }
        public DateTime CaiToDate { get; set; }
        public string InitialCorrelative { get; set; } = string.Empty;
        public string FinalCorrelative { get; set; } = string.Empty;

        // Items de la factura
        public List<InvoicePdfItem> Items { get; set; } = new();

        // Pagos de la factura
        public List<InvoicePdfPayment> Payments { get; set; } = new();
    }

    /// <summary>
    /// Item/producto de la factura para el PDF.
    /// </summary>
    public class InvoicePdfItem
    {
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TotalLine { get; set; }
    }

    /// <summary>
    /// Pago de la factura para el PDF.
    /// </summary>
    public class InvoicePdfPayment
    {
        public string PaymentTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
