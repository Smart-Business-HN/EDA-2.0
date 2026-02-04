using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ProductSpecification
{
    public sealed class GetExpiringProductsSpecification : Specification<Product>
    {
        public GetExpiringProductsSpecification(DateTime thresholdDate)
        {
            Query.Where(p => p.ExpirationDate.HasValue && p.ExpirationDate <= thresholdDate && p.Stock > 0)
                 .Include(p => p.Family)
                 .OrderBy(p => p.ExpirationDate);
        }
    }
}
