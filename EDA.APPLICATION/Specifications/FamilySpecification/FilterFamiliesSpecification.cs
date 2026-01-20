using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.FamilySpecification
{
    public sealed class FilterFamiliesSpecification : Specification<Family>
    {
        public FilterFamiliesSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(family => family.Name.Contains(searchTerm));
            }

            Query.OrderBy(family => family.Name);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountFamiliesSpecification : Specification<Family>
    {
        public CountFamiliesSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(family => family.Name.Contains(searchTerm));
            }
        }
    }

    public sealed class GetFamilyByNameSpecification : Specification<Family>
    {
        public GetFamilyByNameSpecification(string name)
        {
            Query.Where(family => family.Name == name);
        }
    }
}
