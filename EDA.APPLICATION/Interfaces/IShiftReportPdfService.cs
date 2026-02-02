using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IShiftReportPdfService
    {
        byte[] GenerateShiftReportPdf(ShiftReportData data);
    }
}
