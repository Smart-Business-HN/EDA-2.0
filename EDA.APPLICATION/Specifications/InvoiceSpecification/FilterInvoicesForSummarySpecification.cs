using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoiceSpecification
{
    public sealed class FilterInvoicesForSummarySpecification : Specification<Invoice>
    {
        public FilterInvoicesForSummarySpecification(DateTime fromDate, DateTime toDate, int? userId = null)
        {
            Query.Where(i => i.Date >= fromDate && i.Date < toDate);

            if (userId.HasValue)
            {
                Query.Where(i => i.UserId == userId.Value);
            }
        }
    }
}
