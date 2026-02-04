using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoicePaymentSpecification
{
    public sealed class GetPaymentsByInvoiceIdsSpecification : Specification<InvoicePayment>
    {
        public GetPaymentsByInvoiceIdsSpecification(List<int> invoiceIds)
        {
            Query.Where(p => invoiceIds.Contains(p.InvoiceId))
                 .Include(p => p.PaymentType);
        }
    }
}
