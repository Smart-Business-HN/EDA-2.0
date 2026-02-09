using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoiceSpecification
{
    public sealed class GetUnprintedInvoicesForShiftSpecification : Specification<Invoice>
    {
        public GetUnprintedInvoicesForShiftSpecification(int userId, DateTime shiftStartTime)
        {
            Query.Where(i => i.UserId == userId
                          && i.Date >= shiftStartTime
                          && !i.IsPrinted)
                 .Include(i => i.Customer)
                 .OrderBy(i => i.Date);
        }
    }
}
