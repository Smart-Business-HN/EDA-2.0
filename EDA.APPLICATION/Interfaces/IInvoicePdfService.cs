using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    public interface IInvoicePdfService
    {
        /// <summary>
        /// Genera un PDF de factura en formato ticket (80mm) para impresoras térmicas.
        /// </summary>
        /// <param name="data">Datos de la factura para generar el PDF</param>
        /// <returns>Array de bytes con el contenido del PDF</returns>
        byte[] GenerateInvoicePdf(InvoicePdfData data);

        /// <summary>
        /// Genera un PDF de factura en formato carta (Letter 8.5" x 11").
        /// </summary>
        /// <param name="data">Datos de la factura para generar el PDF</param>
        /// <returns>Array de bytes con el contenido del PDF</returns>
        byte[] GenerateInvoiceLetterPdf(InvoicePdfData data);
    }
}
