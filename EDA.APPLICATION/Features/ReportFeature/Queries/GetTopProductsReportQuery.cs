using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Specifications.SoldProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetTopProductsReportQuery : IRequest<Result<TopProductsReportData>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TopN { get; set; } = 10;
        public string SortBy { get; set; } = "Revenue";
    }

    public class GetTopProductsReportQueryHandler : IRequestHandler<GetTopProductsReportQuery, Result<TopProductsReportData>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<SoldProduct> _soldProductRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetTopProductsReportQueryHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<SoldProduct> soldProductRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _invoiceRepository = invoiceRepository;
            _soldProductRepository = soldProductRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<TopProductsReportData>> Handle(GetTopProductsReportQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _invoiceRepository.ListAsync(
                new GetInvoicesForPeriodReportSpecification(request.FromDate, request.ToDate),
                cancellationToken);

            var invoiceIds = invoices.Select(i => i.Id).ToList();

            List<SoldProduct> soldProducts = new();
            if (invoiceIds.Count > 0)
            {
                soldProducts = await _soldProductRepository.ListAsync(
                    new GetSoldProductsForPeriodSpecification(invoiceIds),
                    cancellationToken);
            }

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var totalRevenue = soldProducts.Sum(sp => sp.TotalLine);
            var totalQuantity = soldProducts.Sum(sp => sp.Quantity);

            var grouped = soldProducts
                .GroupBy(sp => new
                {
                    ProductId = sp.ProductId,
                    ProductName = sp.Product?.Name ?? sp.Description ?? "Producto",
                    Barcode = sp.Product?.Barcode,
                    FamilyName = sp.Product?.Family?.Name ?? "-"
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    g.Key.Barcode,
                    g.Key.FamilyName,
                    QuantitySold = g.Sum(sp => sp.Quantity),
                    TotalRevenue = g.Sum(sp => sp.TotalLine),
                    AveragePrice = g.Sum(sp => sp.Quantity) > 0
                        ? g.Sum(sp => sp.TotalLine) / g.Sum(sp => sp.Quantity)
                        : 0
                });

            var ordered = request.SortBy == "Quantity"
                ? grouped.OrderByDescending(g => g.QuantitySold)
                : grouped.OrderByDescending(g => g.TotalRevenue);

            var items = ordered
                .Take(request.TopN)
                .Select((g, index) => new TopProductItem
                {
                    Rank = index + 1,
                    ProductId = g.ProductId,
                    ProductName = g.ProductName,
                    Barcode = g.Barcode,
                    FamilyName = g.FamilyName,
                    QuantitySold = g.QuantitySold,
                    TotalRevenue = g.TotalRevenue,
                    AveragePrice = g.AveragePrice,
                    PercentageOfTotal = totalRevenue > 0
                        ? Math.Round(g.TotalRevenue / totalRevenue * 100, 2)
                        : 0
                })
                .ToList();

            return new Result<TopProductsReportData>(new TopProductsReportData
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TopN = request.TopN,
                SortBy = request.SortBy,
                Products = items,
                TotalRevenue = totalRevenue,
                TotalQuantity = totalQuantity,
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }
    }
}
