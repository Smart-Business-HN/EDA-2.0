using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;
using System.Globalization;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetMonthlyClosingReportQuery : IRequest<Result<MonthlyClosingReportData>>
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class GetMonthlyClosingReportQueryHandler : IRequestHandler<GetMonthlyClosingReportQuery, Result<MonthlyClosingReportData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetMonthlyClosingReportQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _invoiceRepository = invoiceRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<MonthlyClosingReportData>> Handle(GetMonthlyClosingReportQuery request, CancellationToken cancellationToken)
        {
            var periodStart = new DateTime(request.Year, request.Month, 1);
            var periodEnd = periodStart.AddMonths(1);

            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForPeriodReportSpecification(periodStart, periodEnd),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var culture = new CultureInfo("es-HN");
            var monthName = culture.DateTimeFormat.GetMonthName(request.Month);

            var totalTaxedAt15 = invoices.Sum(i => i.TaxedAt15Percent);
            var totalTaxesAt15 = invoices.Sum(i => i.TaxesAt15Percent);
            var totalTaxedAt18 = invoices.Sum(i => i.TaxedAt18Percent);
            var totalTaxesAt18 = invoices.Sum(i => i.TaxesAt18Percent);
            var totalExempt = invoices.Sum(i => i.Exempt);
            var grandTotalTaxes = totalTaxesAt15 + totalTaxesAt18;

            return new Result<MonthlyClosingReportData>(new MonthlyClosingReportData
            {
                Year = request.Year,
                Month = request.Month,
                MonthName = char.ToUpper(monthName[0]) + monthName.Substring(1),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd.AddDays(-1),
                GeneratedAt = DateTime.Now,
                TotalInvoices = invoices.Count,
                TotalSubtotal = invoices.Sum(i => i.Subtotal),
                TotalDiscounts = (decimal)invoices.Sum(i => i.TotalDiscounts),
                TotalSales = invoices.Sum(i => i.Total),
                TotalTaxedAt15 = totalTaxedAt15,
                TotalTaxesAt15 = totalTaxesAt15,
                TotalTaxedAt18 = totalTaxedAt18,
                TotalTaxesAt18 = totalTaxesAt18,
                TotalExempt = totalExempt,
                GrandTotalTaxes = grandTotalTaxes,
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }
    }
}
