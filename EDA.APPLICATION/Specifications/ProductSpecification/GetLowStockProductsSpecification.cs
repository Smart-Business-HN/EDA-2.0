using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ProductSpecification
{
    public sealed class GetLowStockProductsSpecification : Specification<Product>
    {
        public GetLowStockProductsSpecification()
        {
            Query.Where(p => p.Stock <= p.MinStock)
                 .Include(p => p.Family)
                 .OrderBy(p => p.Stock);
        }
    }
}
