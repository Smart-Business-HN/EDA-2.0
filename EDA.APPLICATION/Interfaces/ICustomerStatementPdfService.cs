using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface ICustomerStatementPdfService
    {
        byte[] GenerateCustomerStatementPdf(CustomerStatementPdfData data);
    }
}
