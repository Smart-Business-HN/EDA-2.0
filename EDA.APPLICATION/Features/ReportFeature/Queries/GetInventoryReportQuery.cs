using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetInventoryReportQuery : IRequest<Result<InventoryReportData>>
    {
    }

    public class GetInventoryReportQueryHandler : IRequestHandler<GetInventoryReportQuery, Result<InventoryReportData>>
    {
        private readonly IRepositoryAsync<Product> _productRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetInventoryReportQueryHandler(
            IRepositoryAsync<Product> productRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<InventoryReportData>> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.ListAsync(
                new GetAllProductsWithFamilySpecification(),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var familyGroups = products
                .GroupBy(p => new { p.FamilyId, FamilyName = p.Family?.Name ?? "Sin Familia" })
                .OrderBy(g => g.Key.FamilyName)
                .Select(g => new InventoryFamilyGroup
                {
                    FamilyId = g.Key.FamilyId,
                    FamilyName = g.Key.FamilyName,
                    Products = g.Select(p => new InventoryProductItem
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Barcode = p.Barcode,
                        Stock = p.Stock,
                        Price = p.Price,
                        TotalValue = p.Stock * p.Price
                    }).ToList(),
                    TotalProducts = g.Count(),
                    TotalUnits = g.Sum(p => p.Stock),
                    TotalValue = g.Sum(p => p.Stock * p.Price)
                })
                .ToList();

            return new Result<InventoryReportData>(new InventoryReportData
            {
                GeneratedAt = DateTime.Now,
                FamilyGroups = familyGroups,
                TotalProducts = products.Count,
                TotalUnits = products.Sum(p => p.Stock),
                TotalInventoryValue = products.Sum(p => p.Stock * p.Price),
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }
    }
}
