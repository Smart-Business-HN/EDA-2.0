namespace EDA.DOMAIN.Enums
{
    public enum CopyStrategyEnum
    {
        CarbonCopy = 1,    // Matricial: 1 impresión (papel carbón)
        DoublePrint = 2,   // Térmica: 2 impresiones seguidas
        EndOfDay = 3,      // Imprimir al cerrar turno
        DigitalOnly = 4    // Solo digital, no imprimir
    }
}
