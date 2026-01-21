using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.PaymentTypeSpecification
{
    public sealed class FilterPaymentTypesSpecification : Specification<PaymentType>
    {
        public FilterPaymentTypesSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(paymentType => paymentType.Name.Contains(searchTerm));
            }

            Query.OrderBy(paymentType => paymentType.Name);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountPaymentTypesSpecification : Specification<PaymentType>
    {
        public CountPaymentTypesSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(paymentType => paymentType.Name.Contains(searchTerm));
            }
        }
    }

    public sealed class GetPaymentTypeByNameSpecification : Specification<PaymentType>
    {
        public GetPaymentTypeByNameSpecification(string name)
        {
            Query.Where(paymentType => paymentType.Name == name);
        }
    }
}
