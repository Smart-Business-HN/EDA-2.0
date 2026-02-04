namespace EDA.APPLICATION.DTOs
{
    public class ShiftClosingData
    {
        public decimal ExpectedCash { get; set; }
        public decimal ExpectedCard { get; set; }
        public decimal ExpectedTotal { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
    }
}
