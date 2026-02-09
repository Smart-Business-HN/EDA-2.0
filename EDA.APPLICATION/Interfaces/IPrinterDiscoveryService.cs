using EDA.APPLICATION.DTOs;

namespace EDA.APPLICATION.Interfaces
{
    /// <summary>
    /// Service for discovering available printers on the system
    /// </summary>
    public interface IPrinterDiscoveryService
    {
        /// <summary>
        /// Discovers all available printers on the system (local and network)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered printers</returns>
        Task<IReadOnlyList<DiscoveredPrinter>> DiscoverPrintersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current status of a specific printer by name
        /// </summary>
        /// <param name="printerName">The system name of the printer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Current printer status</returns>
        Task<PrinterStatusType> GetPrinterStatusAsync(string printerName, CancellationToken cancellationToken = default);
    }
}
