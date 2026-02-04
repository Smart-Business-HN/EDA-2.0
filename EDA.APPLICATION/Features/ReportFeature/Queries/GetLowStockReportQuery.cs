using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReportFeature.Queries
{
    public class GetLowStockReportQuery : IRequest<Result<LowStockReportData>>
    {
    }

    public class GetLowStockReportQueryHandler : IRequestHandler<GetLowStockReportQuery, Result<LowStockReportData>>
    {
        private readonly IRepositoryAsync<Product> _productRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public GetLowStockReportQueryHandler(
            IRepositoryAsync<Product> productRepository,
            IRepositoryAsync<Company> companyRepository)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
        }

        public async Task<Result<LowStockReportData>> Handle(GetLowStockReportQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.ListAsync(
                new GetLowStockProductsSpecification(),
                cancellationToken);

            var companies = await _companyRepository.ListAsync(cancellationToken);
            var company = companies.FirstOrDefault();

            var items = products.Select(p => new LowStockItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Barcode = p.Barcode,
                FamilyName = p.Family?.Name ?? "-",
                CurrentStock = p.Stock,
                MinStock = p.MinStock,
                MaxStock = p.MaxStock,
                SuggestedOrder = Math.Max(0, p.MaxStock - p.Stock),
                UnitPrice = p.Price,
                Status = GetStockStatus(p.Stock, p.MinStock)
            }).ToList();

            return new Result<LowStockReportData>(new LowStockReportData
            {
                GeneratedAt = DateTime.Now,
                Products = items,
                TotalProductsAtRisk = items.Count,
                TotalOutOfStock = items.Count(i => i.CurrentStock == 0),
                CompanyName = company?.Name ?? "Empresa",
                CompanyAddress = company?.Address1,
                CompanyRTN = company?.RTN
            });
        }

        private static string GetStockStatus(int currentStock, int minStock)
        {
            if (currentStock == 0) return "Sin Stock";
            if (currentStock <= minStock / 2) return "Critico";
            return "Bajo";
        }
    }
}
