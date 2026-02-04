using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoiceSpecification
{
    public sealed class GetInvoicesForPeriodReportSpecification : Specification<Invoice>
    {
        public GetInvoicesForPeriodReportSpecification(DateTime fromDate, DateTime toDate)
        {
            Query.Where(i => i.Date >= fromDate && i.Date < toDate)
                 .OrderBy(i => i.Date);
        }
    }
}
