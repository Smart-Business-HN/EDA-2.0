using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.SoldProductSpecification
{
    public sealed class GetSoldProductsByInvoiceIdSpecification : Specification<SoldProduct>
    {
        public GetSoldProductsByInvoiceIdSpecification(int invoiceId)
        {
            Query.Where(sp => sp.InvoiceId == invoiceId);
        }
    }
}
