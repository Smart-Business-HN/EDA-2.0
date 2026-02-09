using EDA.DOMAIN.Enums;

namespace EDA.DOMAIN.Entities
{
    public class PrinterConfiguration
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int PrinterType { get; set; } // PrinterTypeEnum
        public string? PrinterName { get; set; } // Nombre del sistema (ej: "EPSON TM-T20")
        public int FontSize { get; set; } = 8; // Default: 8pt para térmica, 11pt para matricial
        public int CopyStrategy { get; set; } // CopyStrategyEnum
        public int CopiesCount { get; set; } = 1; // Cantidad de copias
        public int PrintWidth { get; set; } = 80; // Ancho en mm (80mm, 58mm, etc.)
        public bool IsActive { get; set; } = true;
        public DateTime CreationDate { get; set; }

        // Helper methods
        public PrinterTypeEnum GetPrinterType() => (PrinterTypeEnum)PrinterType;
        public CopyStrategyEnum GetCopyStrategy() => (CopyStrategyEnum)CopyStrategy;

        public float GetFontSizeForPdf() => PrinterType == (int)PrinterTypeEnum.DotMatrix ? 11f : 8f;
        public int GetCopiesForPrint() => CopyStrategy == (int)CopyStrategyEnum.DoublePrint ? 2 : 1;
    }
}
