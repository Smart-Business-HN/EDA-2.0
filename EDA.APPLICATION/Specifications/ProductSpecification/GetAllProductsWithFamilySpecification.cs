using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ProductSpecification
{
    public sealed class GetAllProductsWithFamilySpecification : Specification<Product>
    {
        public GetAllProductsWithFamilySpecification()
        {
            Query.Include(p => p.Family)
                 .OrderBy(p => p.Family!.Name)
                 .ThenBy(p => p.Name);
        }
    }
}
