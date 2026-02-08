using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ProviderSpecification
{
    public sealed class FilterProvidersSpecification : Specification<Provider>
    {
        public FilterProvidersSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            Query.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.RTN.Contains(searchTerm) ||
                    (p.Email != null && p.Email.Contains(searchTerm)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)) ||
                    (p.ContactPerson != null && p.ContactPerson.Contains(searchTerm)));
            }

            Query.OrderBy(p => p.Name);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountProvidersSpecification : Specification<Provider>
    {
        public CountProvidersSpecification(string? searchTerm)
        {
            Query.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.RTN.Contains(searchTerm) ||
                    (p.Email != null && p.Email.Contains(searchTerm)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)) ||
                    (p.ContactPerson != null && p.ContactPerson.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetProviderByNameSpecification : Specification<Provider>
    {
        public GetProviderByNameSpecification(string name)
        {
            Query.Where(p => p.Name == name && p.IsActive);
        }
    }

    public sealed class GetProviderByRtnSpecification : Specification<Provider>
    {
        public GetProviderByRtnSpecification(string rtn)
        {
            Query.Where(p => p.RTN == rtn && p.IsActive);
        }
    }
}
