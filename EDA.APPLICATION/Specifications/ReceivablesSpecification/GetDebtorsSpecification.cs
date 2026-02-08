using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ReceivablesSpecification
{
    /// <summary>
    /// Specification para obtener facturas pendientes (Status=1, OutstandingAmount > 0)
    /// agrupables por cliente
    /// </summary>
    public sealed class GetDebtorsSpecification : Specification<Invoice>
    {
        public GetDebtorsSpecification(string? searchTerm = null)
        {
            Query.Include(i => i.Customer)
                 .Where(i => i.Status == 1 && i.OutstandingAmount > 0);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(i =>
                    i.Customer != null &&
                    (i.Customer.Name.Contains(searchTerm) ||
                     (i.Customer.RTN != null && i.Customer.RTN.Contains(searchTerm))));
            }
        }
    }

    /// <summary>
    /// Specification para obtener facturas pendientes de un cliente especifico
    /// </summary>
    public sealed class GetPendingInvoicesByCustomerSpecification : Specification<Invoice>
    {
        public GetPendingInvoicesByCustomerSpecification(int customerId)
        {
            Query.Include(i => i.Customer)
                 .Where(i => i.CustomerId == customerId &&
                            i.Status == 1 &&
                            i.OutstandingAmount > 0)
                 .OrderBy(i => i.DueDate)
                 .ThenBy(i => i.Date);
        }
    }
}
