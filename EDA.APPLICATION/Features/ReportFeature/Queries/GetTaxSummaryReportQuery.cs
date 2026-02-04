using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetTaxSummaryReportQuery : IRequest<Result<TaxSummaryReportData>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetTaxSummaryReportQueryHandler : IRequestHandler<GetTaxSummaryReportQuery, Result<TaxSummaryReportData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetTaxSummaryReportQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _invoiceRepository = invoiceRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<TaxSummaryReportData>> Handle(GetTaxSummaryReportQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForPeriodReportSpecification(request.FromDate, request.ToDate),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var totalTaxedAt15 = invoices.Sum(i => i.TaxedAt15Percent);
            var totalTaxesAt15 = invoices.Sum(i => i.TaxesAt15Percent);
            var totalTaxedAt18 = invoices.Sum(i => i.TaxedAt18Percent);
            var totalTaxesAt18 = invoices.Sum(i => i.TaxesAt18Percent);
            var totalExempt = invoices.Sum(i => i.Exempt);
            var grandTotalTaxes = totalTaxesAt15 + totalTaxesAt18;
            var grandTotalSales = invoices.Sum(i => i.Total);

            return new Result<TaxSummaryReportData>(new TaxSummaryReportData
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalTaxedAt15 = totalTaxedAt15,
                TotalTaxesAt15 = totalTaxesAt15,
                TotalTaxedAt18 = totalTaxedAt18,
                TotalTaxesAt18 = totalTaxesAt18,
                TotalExempt = totalExempt,
                GrandTotalTaxes = grandTotalTaxes,
                GrandTotalSales = grandTotalSales,
                TotalInvoices = invoices.Count,
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }
    }
}
