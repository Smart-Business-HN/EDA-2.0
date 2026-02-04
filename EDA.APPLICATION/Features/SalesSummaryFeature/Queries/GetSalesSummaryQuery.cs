using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ShiftSpecification;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.SalesSummaryFeature.Queries
{
    public class GetSalesSummaryQuery : IRequest<Result<SalesSummaryData>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? UserId { get; set; }
    }

    public class GetSalesSummaryQueryHandler : IRequestHandler<GetSalesSummaryQuery, Result<SalesSummaryData>>
    {
        private readonly IRepositoryAsync<Shift> _shiftRepository;
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;

        public GetSalesSummaryQueryHandler(
            IRepositoryAsync<Shift> shiftRepository,
            IRepositoryAsync<Invoice> invoiceRepository)
        {
            _shiftRepository = shiftRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Result<SalesSummaryData>> Handle(GetSalesSummaryQuery request, CancellationToken cancellationToken)
        {
            // Obtener turnos en el rango
            var shifts = await _shiftRepository.ListAsync(
                new FilterShiftsForSummarySpecification(request.FromDate, request.ToDate, request.UserId),
                cancellationToken);

            // Obtener facturas en el rango
            var invoices = await _invoiceRepository.ListAsync(
                new FilterInvoicesForSummarySpecification(request.FromDate, request.ToDate, request.UserId),
                cancellationToken);

            // Construir detalle por turno
            var shiftDetails = new List<SalesSummaryShiftItem>();
            foreach (var shift in shifts)
            {
                var endTime = shift.EndTime ?? DateTime.Now;
                var shiftInvoices = invoices
                    .Where(i => i.UserId == shift.UserId && i.Date >= shift.StartTime && i.Date <= endTime)
                    .ToList();

                shiftDetails.Add(new SalesSummaryShiftItem
                {
                    UserName = shift.User?.Name ?? "-",
                    ShiftType = shift.ShiftType,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    InitialAmount = shift.InitialAmount,
                    FinalAmount = shift.FinalAmount,
                    Difference = shift.Difference,
                    InvoiceCount = shiftInvoices.Count,
                    TotalSales = shiftInvoices.Sum(i => i.Total)
                });
            }

            // Construir resumen por usuario
            var userSummary = shiftDetails
                .GroupBy(s => s.UserName)
                .Select(g => new SalesSummaryUserItem
                {
                    UserName = g.Key,
                    TotalShifts = g.Count(),
                    TotalInvoices = g.Sum(s => s.InvoiceCount),
                    TotalSales = g.Sum(s => s.TotalSales)
                })
                .OrderByDescending(u => u.TotalSales)
                .ToList();

            return new Result<SalesSummaryData>(new SalesSummaryData
            {
                UserSummaries = userSummary,
                ShiftDetails = shiftDetails,
                GrandTotalShifts = shifts.Count,
                GrandTotalInvoices = shiftDetails.Sum(s => s.InvoiceCount),
                GrandTotalSales = shiftDetails.Sum(s => s.TotalSales)
            });
        }
    }
}
