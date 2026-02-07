using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoicePaymentSpecification;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetPaymentMethodsReportQuery : IRequest<Result<PaymentMethodsReportData>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetPaymentMethodsReportQueryHandler : IRequestHandler<GetPaymentMethodsReportQuery, Result<PaymentMethodsReportData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoicePayment> _paymentRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetPaymentMethodsReportQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<InvoicePayment> paymentRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<PaymentMethodsReportData>> Handle(GetPaymentMethodsReportQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForPeriodReportSpecification(request.FromDate, request.ToDate),
                cancellationToken);

            var invoiceIds = invoices.Select(i => i.Id).ToList();

            List<InvoicePayment> payments = new();
            if (invoiceIds.Count > 0)
            {
                payments = await _paymentRepository.ListAsync(
                    new GetPaymentsForPeriodSpecification(invoiceIds),
                    cancellationToken);
            }

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var grandTotal = payments.Sum(p => p.Amount);

            var items = payments
                .GroupBy(p => new { p.PaymentTypeId, PaymentTypeName = p.PaymentType?.Name ?? "Desconocido" })
                .Select(g => new PaymentMethodItem
                {
                    PaymentTypeId = g.Key.PaymentTypeId,
                    PaymentTypeName = g.Key.PaymentTypeName,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount),
                    Percentage = grandTotal > 0 ? Math.Round(g.Sum(p => p.Amount) / grandTotal * 100, 2) : 0
                })
                .OrderByDescending(i => i.TotalAmount)
                .ToList();

            return new Result<PaymentMethodsReportData>(new PaymentMethodsReportData
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Items = items,
                GrandTotal = grandTotal,
                TotalTransactions = payments.Count,
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }
    }
}
