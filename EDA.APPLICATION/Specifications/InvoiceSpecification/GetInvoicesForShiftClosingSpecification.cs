using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoiceSpecification
{
    public sealed class GetInvoicesForShiftClosingSpecification : Specification<Invoice>
    {
        public GetInvoicesForShiftClosingSpecification(int userId, DateTime shiftStartTime)
        {
            Query.Where(i => i.UserId == userId && i.Date >= shiftStartTime);
        }
    }
}
