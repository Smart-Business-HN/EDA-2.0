using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoicePaymentSpecification;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Queries
{
    public class GetShiftClosingDataQuery : IRequest<Result<ShiftClosingData>>
    {
        public int UserId { get; set; }
        public DateTime ShiftStartTime { get; set; }
        public decimal InitialAmount { get; set; }
    }

    public class GetShiftClosingDataQueryHandler : IRequestHandler<GetShiftClosingDataQuery, Result<ShiftClosingData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoicePayment> _paymentRepository;

        public GetShiftClosingDataQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<InvoicePayment> paymentRepository)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<ShiftClosingData>> Handle(GetShiftClosingDataQuery request, CancellationToken cancellationToken)
        {
            // Obtener facturas del turno
            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForShiftClosingSpecification(request.UserId, request.ShiftStartTime),
                cancellationToken);

            var invoiceIds = invoices.Select(i => i.Id).ToList();

            // Obtener pagos de esas facturas
            List<InvoicePayment> payments = new();
            if (invoiceIds.Count > 0)
            {
                payments = await _paymentRepository.ListAsync(
                    new GetPaymentsByInvoiceIdsSpecification(invoiceIds),
                    cancellationToken);
            }

            // Calcular montos esperados
            // PaymentTypeId: 1 = Efectivo, 2 = Transferencia, 3 = Tarjeta
            var expectedCash = payments.Where(p => p.PaymentTypeId == 1).Sum(p => p.Amount);
            var expectedCard = payments.Where(p => p.PaymentTypeId == 2 || p.PaymentTypeId == 3).Sum(p => p.Amount);
            var expectedTotal = request.InitialAmount + expectedCash + expectedCard;
            var totalSales = invoices.Sum(i => i.Total);

            // Obtener facturas no impresas del turno
            var unprintedInvoices = await _invoiceRepository.ListAsync(
                new GetUnprintedInvoicesForShiftSpecification(request.UserId, request.ShiftStartTime),
                cancellationToken);

            return new Result<ShiftClosingData>(new ShiftClosingData
            {
                ExpectedCash = expectedCash,
                ExpectedCard = expectedCard,
                ExpectedTotal = expectedTotal,
                TotalInvoices = invoices.Count,
                TotalSales = totalSales,
                UnprintedInvoices = unprintedInvoices
            });
        }
    }
}
