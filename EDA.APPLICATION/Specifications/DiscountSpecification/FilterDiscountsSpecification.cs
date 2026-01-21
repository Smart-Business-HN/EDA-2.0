using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.DiscountSpecification
{
    public sealed class FilterDiscountsSpecification : Specification<Discount>
    {
        public FilterDiscountsSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(discount => discount.Name.Contains(searchTerm));
            }

            Query.OrderBy(discount => discount.Name);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountDiscountsSpecification : Specification<Discount>
    {
        public CountDiscountsSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(discount => discount.Name.Contains(searchTerm));
            }
        }
    }

    public sealed class GetDiscountByNameSpecification : Specification<Discount>
    {
        public GetDiscountByNameSpecification(string name)
        {
            Query.Where(discount => discount.Name == name);
        }
    }
}
