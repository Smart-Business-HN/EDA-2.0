namespace EDA.APPLICATION.DTOs
{
    /// <summary>
    /// Represents a discovered printer on the system
    /// </summary>
    public class DiscoveredPrinter
    {
        /// <summary>
        /// The system name of the printer (use this for printing)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Display-friendly name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Type of printer connection
        /// </summary>
        public PrinterConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Current status of the printer
        /// </summary>
        public PrinterStatusType Status { get; set; }

        /// <summary>
        /// Port name (USB001, LPT1, IP_xxx, etc.)
        /// </summary>
        public string? PortName { get; set; }

        /// <summary>
        /// Driver name
        /// </summary>
        public string? DriverName { get; set; }

        /// <summary>
        /// Network path for shared printers (\\server\printer)
        /// </summary>
        public string? NetworkPath { get; set; }

        /// <summary>
        /// Whether the printer is set as default
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Whether the printer is currently online/available
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Returns a display string with connection type in Spanish
        /// </summary>
        public string ConnectionTypeDisplay => ConnectionType switch
        {
            PrinterConnectionType.LocalUsb => "USB Local",
            PrinterConnectionType.LocalParallel => "Puerto Paralelo",
            PrinterConnectionType.NetworkShared => "Red Compartida",
            PrinterConnectionType.NetworkTcpIp => "Red TCP/IP",
            PrinterConnectionType.Virtual => "Virtual",
            _ => "Desconocido"
        };

        /// <summary>
        /// Returns a display string with status in Spanish
        /// </summary>
        public string StatusDisplay => Status switch
        {
            PrinterStatusType.Ready => "Lista",
            PrinterStatusType.Offline => "Fuera de linea",
            PrinterStatusType.Busy => "Ocupada",
            PrinterStatusType.Error => "Error",
            PrinterStatusType.PaperOut => "Sin papel",
            PrinterStatusType.PaperJam => "Atasco de papel",
            _ => "Desconocido"
        };
    }

    /// <summary>
    /// Type of printer connection
    /// </summary>
    public enum PrinterConnectionType
    {
        Unknown = 0,
        LocalUsb = 1,
        LocalParallel = 2,
        NetworkShared = 3,
        NetworkTcpIp = 4,
        Virtual = 5
    }

    /// <summary>
    /// Status of the printer
    /// </summary>
    public enum PrinterStatusType
    {
        Unknown = 0,
        Ready = 1,
        Offline = 2,
        Busy = 3,
        Error = 4,
        PaperOut = 5,
        PaperJam = 6
    }
}
