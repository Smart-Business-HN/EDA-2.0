using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.SoldProductSpecification
{
    public sealed class GetSoldProductsForPeriodSpecification : Specification<SoldProduct>
    {
        public GetSoldProductsForPeriodSpecification(List<int> invoiceIds)
        {
            Query.Where(sp => invoiceIds.Contains(sp.InvoiceId))
                 .Include(sp => sp.Product)
                    .ThenInclude(p => p!.Family);
        }
    }
}
