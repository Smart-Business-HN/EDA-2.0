using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.CaiSpecification
{
    public sealed class FilterCaisSpecification : Specification<Cai>
    {
        public FilterCaisSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(cai =>
                    cai.Name.Contains(searchTerm) ||
                    cai.Code.Contains(searchTerm) ||
                    cai.Prefix.Contains(searchTerm));
            }

            Query.OrderByDescending(cai => cai.IsActive)
                 .ThenByDescending(cai => cai.FromDate);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountCaisSpecification : Specification<Cai>
    {
        public CountCaisSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(cai =>
                    cai.Name.Contains(searchTerm) ||
                    cai.Code.Contains(searchTerm) ||
                    cai.Prefix.Contains(searchTerm));
            }
        }
    }

    public sealed class GetCaiByCodeSpecification : Specification<Cai>
    {
        public GetCaiByCodeSpecification(string code)
        {
            Query.Where(cai => cai.Code == code);
        }
    }

    public sealed class GetCaiByPrefixSpecification : Specification<Cai>
    {
        public GetCaiByPrefixSpecification(string prefix)
        {
            Query.Where(cai => cai.Prefix == prefix);
        }
    }

    public sealed class GetActiveCaiSpecification : Specification<Cai>
    {
        public GetActiveCaiSpecification()
        {
            Query.Where(cai => cai.IsActive && cai.PendingInvoices > 0);
        }
    }
}
