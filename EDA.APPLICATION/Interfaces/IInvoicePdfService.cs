using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IInvoicePdfService
    {
        /// <summary>
        /// Genera un PDF de factura a partir de los datos proporcionados.
        /// </summary>
        /// <param name="data">Datos de la factura para generar el PDF</param>
        /// <returns>Array de bytes con el contenido del PDF</returns>
        byte[] GenerateInvoicePdf(InvoicePdfData data);
    }
}
