using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ExpenseAccountSpecification
{
    public sealed class FilterExpenseAccountsSpecification : Specification<ExpenseAccount>
    {
        public FilterExpenseAccountsSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(e => e.Name.Contains(searchTerm));
            }

            Query.OrderBy(e => e.Name);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountExpenseAccountsSpecification : Specification<ExpenseAccount>
    {
        public CountExpenseAccountsSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(e => e.Name.Contains(searchTerm));
            }
        }
    }

    public sealed class GetExpenseAccountByNameSpecification : Specification<ExpenseAccount>
    {
        public GetExpenseAccountByNameSpecification(string name)
        {
            Query.Where(e => e.Name == name);
        }
    }
}
