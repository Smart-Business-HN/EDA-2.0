using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using System.Management;
using System.Printing;

namespace EDA.INFRAESTRUCTURE.Services
{
    /// <summary>
    /// Service for discovering available printers on the system using System.Printing and WMI
    /// </summary>
    public class PrinterDiscoveryService : IPrinterDiscoveryService
    {
        public Task<IReadOnlyList<DiscoveredPrinter>> DiscoverPrintersAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var printers = new List<DiscoveredPrinter>();

                try
                {
                    // Use System.Printing for enumeration
                    using var printServer = new LocalPrintServer();
                    var printQueues = printServer.GetPrintQueues(
                        new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

                    var defaultPrinter = printServer.DefaultPrintQueue?.Name;

                    foreach (var queue in printQueues)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            var printer = new DiscoveredPrinter
                            {
                                Name = queue.Name,
                                DisplayName = queue.Name,
                                DriverName = queue.QueueDriver?.Name,
                                IsDefault = queue.Name == defaultPrinter,
                                IsOnline = !queue.IsOffline
                            };

                            // Determine connection type from port name
                            var portName = queue.QueuePort?.Name ?? string.Empty;
                            printer.PortName = portName;
                            printer.ConnectionType = ClassifyPrinterType(portName, queue.Name);
                            printer.Status = MapQueueStatus(queue);

                            // Check for network path
                            if (queue.Name.StartsWith(@"\\"))
                            {
                                printer.NetworkPath = queue.Name;
                                printer.ConnectionType = PrinterConnectionType.NetworkShared;
                            }

                            printers.Add(printer);
                        }
                        catch
                        {
                            // Skip printers that cause errors
                        }
                        finally
                        {
                            queue.Dispose();
                        }
                    }
                }
                catch
                {
                    // Fallback to WMI if System.Printing fails
                    printers = DiscoverPrintersViaWmi(cancellationToken);
                }

                // Enrich with WMI data for better classification
                EnrichWithWmiData(printers, cancellationToken);

                return (IReadOnlyList<DiscoveredPrinter>)printers.AsReadOnly();
            }, cancellationToken);
        }

        public Task<PrinterStatusType> GetPrinterStatusAsync(string printerName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                try
                {
                    var escapedName = printerName.Replace("\\", "\\\\").Replace("'", "\\'");
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT PrinterStatus FROM Win32_Printer WHERE Name = '{escapedName}'");

                    foreach (var obj in searcher.Get())
                    {
                        var status = Convert.ToUInt32(obj["PrinterStatus"]);
                        return MapWmiStatus(status);
                    }
                }
                catch
                {
                    // Ignore errors
                }

                return PrinterStatusType.Unknown;
            }, cancellationToken);
        }

        private PrinterConnectionType ClassifyPrinterType(string portName, string printerName)
        {
            if (string.IsNullOrEmpty(portName))
                return PrinterConnectionType.Unknown;

            var port = portName.ToUpperInvariant();

            // USB printers
            if (port.StartsWith("USB") || port.Contains("DOT4"))
                return PrinterConnectionType.LocalUsb;

            // Parallel port
            if (port.StartsWith("LPT"))
                return PrinterConnectionType.LocalParallel;

            // Network TCP/IP printers
            if (port.StartsWith("IP_") || port.Contains("TCPMON") ||
                System.Net.IPAddress.TryParse(port.Split(':')[0], out _))
                return PrinterConnectionType.NetworkTcpIp;

            // Shared network printers
            if (printerName.StartsWith(@"\\") || port.StartsWith(@"\\"))
                return PrinterConnectionType.NetworkShared;

            // Virtual printers (PDF, XPS, OneNote, etc.)
            if (port.Contains("XPS") || port.Contains("PDF") ||
                port == "PORTPROMPT:" || port == "NUL:" || port == "FILE:" ||
                port.Contains("ONENOTE") || port.Contains("MICROSOFT"))
                return PrinterConnectionType.Virtual;

            return PrinterConnectionType.Unknown;
        }

        private PrinterStatusType MapQueueStatus(PrintQueue queue)
        {
            if (queue.IsOffline)
                return PrinterStatusType.Offline;
            if (queue.IsBusy || queue.IsPrinting)
                return PrinterStatusType.Busy;
            if (queue.HasPaperProblem)
                return PrinterStatusType.PaperOut;
            if (queue.IsPaperJammed)
                return PrinterStatusType.PaperJam;
            if (queue.IsInError)
                return PrinterStatusType.Error;

            return PrinterStatusType.Ready;
        }

        private PrinterStatusType MapWmiStatus(uint status)
        {
            return status switch
            {
                1 => PrinterStatusType.Ready,      // Other
                2 => PrinterStatusType.Unknown,    // Unknown
                3 => PrinterStatusType.Ready,      // Idle
                4 => PrinterStatusType.Busy,       // Printing
                5 => PrinterStatusType.Busy,       // Warmup
                6 => PrinterStatusType.Busy,       // Stopped printing
                7 => PrinterStatusType.Offline,    // Offline
                _ => PrinterStatusType.Unknown
            };
        }

        private List<DiscoveredPrinter> DiscoverPrintersViaWmi(CancellationToken cancellationToken)
        {
            var printers = new List<DiscoveredPrinter>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                foreach (var obj in searcher.Get())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var name = obj["Name"]?.ToString() ?? string.Empty;
                    var portName = obj["PortName"]?.ToString() ?? string.Empty;
                    var isDefault = Convert.ToBoolean(obj["Default"]);
                    var isNetwork = Convert.ToBoolean(obj["Network"]);
                    var isLocal = Convert.ToBoolean(obj["Local"]);
                    var status = Convert.ToUInt32(obj["PrinterStatus"] ?? 0);

                    var printer = new DiscoveredPrinter
                    {
                        Name = name,
                        DisplayName = name,
                        PortName = portName,
                        DriverName = obj["DriverName"]?.ToString(),
                        IsDefault = isDefault,
                        IsOnline = status != 7, // 7 = Offline
                        Status = MapWmiStatus(status)
                    };

                    if (isNetwork)
                        printer.ConnectionType = PrinterConnectionType.NetworkShared;
                    else if (isLocal)
                        printer.ConnectionType = ClassifyPrinterType(portName, name);

                    printers.Add(printer);
                }
            }
            catch
            {
                // Return empty list on error
            }

            return printers;
        }

        private void EnrichWithWmiData(List<DiscoveredPrinter> printers, CancellationToken cancellationToken)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, Network, Local FROM Win32_Printer");
                var wmiData = new Dictionary<string, (bool IsNetwork, bool IsLocal)>();

                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString() ?? string.Empty;
                    var isNetwork = Convert.ToBoolean(obj["Network"]);
                    var isLocal = Convert.ToBoolean(obj["Local"]);
                    wmiData[name] = (isNetwork, isLocal);
                }

                foreach (var printer in printers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (wmiData.TryGetValue(printer.Name, out var data))
                    {
                        if (data.IsNetwork && printer.ConnectionType == PrinterConnectionType.Unknown)
                            printer.ConnectionType = PrinterConnectionType.NetworkShared;
                    }
                }
            }
            catch
            {
                // Ignore enrichment errors
            }
        }
    }
}
