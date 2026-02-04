using Ardalis.Specification;
using EDA.DOMAIN.Entities;
using System.Collections.Generic;

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
