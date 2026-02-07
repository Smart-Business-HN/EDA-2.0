using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoicePaymentSpecification
{
    public sealed class GetPaymentsForPeriodSpecification : Specification<InvoicePayment>
    {
        public GetPaymentsForPeriodSpecification(List<int> invoiceIds)
        {
            Query.Where(p => invoiceIds.Contains(p.InvoiceId))
                 .Include(p => p.PaymentType);
        }
    }
}
