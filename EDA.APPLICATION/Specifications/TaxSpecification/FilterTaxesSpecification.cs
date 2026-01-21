using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.TaxSpecification
{
    public sealed class FilterTaxesSpecification : Specification<Tax>
    {
        public FilterTaxesSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(tax => tax.Name.Contains(searchTerm));
            }

            Query.OrderBy(tax => tax.Name);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountTaxesSpecification : Specification<Tax>
    {
        public CountTaxesSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(tax => tax.Name.Contains(searchTerm));
            }
        }
    }

    public sealed class GetTaxByNameSpecification : Specification<Tax>
    {
        public GetTaxByNameSpecification(string name)
        {
            Query.Where(tax => tax.Name == name);
        }
    }
}
