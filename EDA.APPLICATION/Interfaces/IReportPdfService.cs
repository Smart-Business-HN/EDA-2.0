using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IReportPdfService
    {
        byte[] GenerateSalesByPeriodReportPdf(SalesByPeriodReportData data);
        byte[] GeneratePaymentMethodsReportPdf(PaymentMethodsReportData data);
        byte[] GenerateTaxSummaryReportPdf(TaxSummaryReportData data);
        byte[] GenerateLowStockReportPdf(LowStockReportData data);
        byte[] GenerateExpiringProductsReportPdf(ExpiringProductsReportData data);
        byte[] GenerateTopProductsReportPdf(TopProductsReportData data);
        byte[] GenerateInventoryReportPdf(InventoryReportData data);
        byte[] GenerateMonthlyClosingReportPdf(MonthlyClosingReportData data);
    }
}
