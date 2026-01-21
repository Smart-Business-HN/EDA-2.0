using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.CustomerSpecification
{
    public sealed class FilterCustomersSpecification : Specification<Customer>
    {
        public FilterCustomersSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(customer =>
                    customer.Name.Contains(searchTerm) ||
                    (customer.Company != null && customer.Company.Contains(searchTerm)) ||
                    (customer.Email != null && customer.Email.Contains(searchTerm)) ||
                    (customer.PhoneNumber != null && customer.PhoneNumber.Contains(searchTerm)));
            }

            Query.OrderBy(customer => customer.Name);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountCustomersSpecification : Specification<Customer>
    {
        public CountCustomersSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(customer =>
                    customer.Name.Contains(searchTerm) ||
                    (customer.Company != null && customer.Company.Contains(searchTerm)) ||
                    (customer.Email != null && customer.Email.Contains(searchTerm)) ||
                    (customer.PhoneNumber != null && customer.PhoneNumber.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetCustomerByNameSpecification : Specification<Customer>
    {
        public GetCustomerByNameSpecification(string name)
        {
            Query.Where(customer => customer.Name == name);
        }
    }
}
