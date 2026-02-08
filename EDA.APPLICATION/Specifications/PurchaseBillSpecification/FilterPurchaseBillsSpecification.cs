using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.PurchaseBillSpecification
{
    public sealed class FilterPurchaseBillsSpecification : Specification<PurchaseBill>
    {
        public FilterPurchaseBillsSpecification(
            string? searchTerm = null,
            int? providerId = null,
            int? expenseAccountId = null,
            int? statusId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            Query.Include(pb => pb.Provider)
                 .Include(pb => pb.ExpenseAccount);

            // Filtro por proveedor
            if (providerId.HasValue)
            {
                Query.Where(pb => pb.ProviderId == providerId.Value);
            }

            // Filtro por cuenta de gastos
            if (expenseAccountId.HasValue)
            {
                Query.Where(pb => pb.ExpenseAccountId == expenseAccountId.Value);
            }

            // Filtro por estado
            if (statusId.HasValue)
            {
                Query.Where(pb => pb.StatusId == statusId.Value);
            }

            // Filtro por rango de fechas
            if (fromDate.HasValue)
            {
                Query.Where(pb => pb.InvoiceDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                Query.Where(pb => pb.InvoiceDate < endDate);
            }

            // Busqueda general
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(pb =>
                    pb.PurchaseBillCode.Contains(searchTerm) ||
                    pb.InvoiceNumber.Contains(searchTerm) ||
                    pb.Cai.Contains(searchTerm) ||
                    (pb.Provider != null && pb.Provider.Name.Contains(searchTerm)));
            }

            // Ordenar por fecha descendente
            Query.OrderByDescending(pb => pb.CreationDate)
                 .ThenByDescending(pb => pb.Id);

            // Paginacion
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountPurchaseBillsSpecification : Specification<PurchaseBill>
    {
        public CountPurchaseBillsSpecification(
            string? searchTerm = null,
            int? providerId = null,
            int? expenseAccountId = null,
            int? statusId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // Filtro por proveedor
            if (providerId.HasValue)
            {
                Query.Where(pb => pb.ProviderId == providerId.Value);
            }

            // Filtro por cuenta de gastos
            if (expenseAccountId.HasValue)
            {
                Query.Where(pb => pb.ExpenseAccountId == expenseAccountId.Value);
            }

            // Filtro por estado
            if (statusId.HasValue)
            {
                Query.Where(pb => pb.StatusId == statusId.Value);
            }

            // Filtro por rango de fechas
            if (fromDate.HasValue)
            {
                Query.Where(pb => pb.InvoiceDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                Query.Where(pb => pb.InvoiceDate < endDate);
            }

            // Busqueda general
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(pb =>
                    pb.PurchaseBillCode.Contains(searchTerm) ||
                    pb.InvoiceNumber.Contains(searchTerm) ||
                    pb.Cai.Contains(searchTerm) ||
                    (pb.Provider != null && pb.Provider.Name.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetPurchaseBillByIdSpecification : Specification<PurchaseBill>, ISingleResultSpecification<PurchaseBill>
    {
        public GetPurchaseBillByIdSpecification(int id)
        {
            Query.Where(pb => pb.Id == id)
                 .Include(pb => pb.Provider)
                 .Include(pb => pb.ExpenseAccount)
                 .Include(pb => pb.PurchaseBillPayments!)
                    .ThenInclude(pbp => pbp.PaymentType);
        }
    }

    public sealed class GetLastPurchaseBillCodeSpecification : Specification<PurchaseBill>
    {
        public GetLastPurchaseBillCodeSpecification()
        {
            Query.OrderByDescending(pb => pb.Id)
                 .Take(1);
        }
    }
}
