using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;
using System.Globalization;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetSalesByPeriodReportQuery : IRequest<Result<SalesByPeriodReportData>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupingType { get; set; } = "Day";
    }

    public class GetSalesByPeriodReportQueryHandler : IRequestHandler<GetSalesByPeriodReportQuery, Result<SalesByPeriodReportData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetSalesByPeriodReportQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _invoiceRepository = invoiceRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<SalesByPeriodReportData>> Handle(GetSalesByPeriodReportQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForPeriodReportSpecification(request.FromDate, request.ToDate),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var groupedItems = GroupInvoicesByPeriod(invoices, request.GroupingType);

            var data = new SalesByPeriodReportData
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                GroupingType = request.GroupingType,
                Items = groupedItems,
                GrandTotal = invoices.Sum(i => i.Total),
                TotalInvoices = invoices.Count,
                TotalTaxes = (decimal)invoices.Sum(i => i.TotalTaxes),
                TotalDiscounts = (decimal)invoices.Sum(i => i.TotalDiscounts),
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            };

            return new Result<SalesByPeriodReportData>(data);
        }

        private List<SalesPeriodItem> GroupInvoicesByPeriod(List<Invoice> invoices, string groupingType)
        {
            var culture = new CultureInfo("es-HN");

            return groupingType switch
            {
                "Week" => invoices
                    .GroupBy(i => new { Year = i.Date.Year, Week = GetWeekOfYear(i.Date) })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                    .Select(g =>
                    {
                        var firstDay = FirstDateOfWeek(g.Key.Year, g.Key.Week);
                        var lastDay = firstDay.AddDays(6);
                        return new SalesPeriodItem
                        {
                            PeriodStart = firstDay,
                            PeriodEnd = lastDay,
                            PeriodLabel = $"Semana {g.Key.Week} ({firstDay:dd/MM} - {lastDay:dd/MM})",
                            InvoiceCount = g.Count(),
                            Subtotal = g.Sum(i => i.Subtotal),
                            TotalTaxes = (decimal)g.Sum(i => i.TotalTaxes),
                            TotalDiscounts = (decimal)g.Sum(i => i.TotalDiscounts),
                            Total = g.Sum(i => i.Total)
                        };
                    }).ToList(),

                "Month" => invoices
                    .GroupBy(i => new { i.Date.Year, i.Date.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g =>
                    {
                        var firstDay = new DateTime(g.Key.Year, g.Key.Month, 1);
                        var lastDay = firstDay.AddMonths(1).AddDays(-1);
                        return new SalesPeriodItem
                        {
                            PeriodStart = firstDay,
                            PeriodEnd = lastDay,
                            PeriodLabel = firstDay.ToString("MMMM yyyy", culture),
                            InvoiceCount = g.Count(),
                            Subtotal = g.Sum(i => i.Subtotal),
                            TotalTaxes = (decimal)g.Sum(i => i.TotalTaxes),
                            TotalDiscounts = (decimal)g.Sum(i => i.TotalDiscounts),
                            Total = g.Sum(i => i.Total)
                        };
                    }).ToList(),

                _ => invoices
                    .GroupBy(i => i.Date.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new SalesPeriodItem
                    {
                        PeriodStart = g.Key,
                        PeriodEnd = g.Key,
                        PeriodLabel = g.Key.ToString("dd/MM/yyyy"),
                        InvoiceCount = g.Count(),
                        Subtotal = g.Sum(i => i.Subtotal),
                        TotalTaxes = (decimal)g.Sum(i => i.TotalTaxes),
                        TotalDiscounts = (decimal)g.Sum(i => i.TotalDiscounts),
                        Total = g.Sum(i => i.Total)
                    }).ToList()
            };
        }

        private static int GetWeekOfYear(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            var firstMonday = jan1.AddDays(daysOffset);
            var firstWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (firstWeek <= 1)
                weekOfYear -= 1;
            return firstMonday.AddDays(weekOfYear * 7);
        }
    }
}
