using System;

namespace EDA.APPLICATION.DTOs
{
    public class ShiftReportData
    {
        public string UserName { get; set; } = null!;
        public string ShiftType { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal Difference { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }
}
