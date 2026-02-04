using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetExpiringProductsReportQuery : IRequest<Result<ExpiringProductsReportData>>
    {
        public int DaysThreshold { get; set; } = 30;
    }

    public class GetExpiringProductsReportQueryHandler : IRequestHandler<GetExpiringProductsReportQuery, Result<ExpiringProductsReportData>>
    {
        private readonly IRepositoryAsync<Product> _productRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetExpiringProductsReportQueryHandler(
            IRepositoryAsync<Product> productRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<ExpiringProductsReportData>> Handle(GetExpiringProductsReportQuery request, CancellationToken cancellationToken)
        {
            var thresholdDate = DateTime.Today.AddDays(request.DaysThreshold);

            var products = await _productRepository.ListAsync(
                new GetExpiringProductsSpecification(thresholdDate),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var items = products.Select(p =>
            {
                var daysUntil = p.ExpirationDate.HasValue
                    ? (int)(p.ExpirationDate.Value.Date - DateTime.Today).TotalDays
                    : 0;

                return new ExpiringProductItem
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Barcode = p.Barcode,
                    FamilyName = p.Family?.Name ?? "-",
                    ExpirationDate = p.ExpirationDate,
                    DaysUntilExpiration = daysUntil,
                    CurrentStock = p.Stock,
                    UnitPrice = p.Price,
                    TotalValue = p.Stock * p.Price,
                    Status = GetExpirationStatus(daysUntil)
                };
            }).ToList();

            return new Result<ExpiringProductsReportData>(new ExpiringProductsReportData
            {
                GeneratedAt = DateTime.Now,
                DaysThreshold = request.DaysThreshold,
                Products = items,
                TotalExpired = items.Count(i => i.DaysUntilExpiration < 0),
                TotalExpiring = items.Count(i => i.DaysUntilExpiration >= 0),
                TotalValueAtRisk = items.Sum(i => i.TotalValue),
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }

        private static string GetExpirationStatus(int daysUntilExpiration)
        {
            if (daysUntilExpiration < 0) return "Vencido";
            if (daysUntilExpiration <= 7) return "Critico";
            return "Proximo";
        }
    }
}
