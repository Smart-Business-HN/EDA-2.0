using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.InvoiceSpecification
{
    public sealed class FilterInvoicesSpecification : Specification<Invoice>
    {
        public FilterInvoicesSpecification(
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? customerRtn = null,
            string? customerName = null,
            string? invoiceNumber = null,
            string? userName = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            Query.Include(i => i.Customer)
                 .Include(i => i.User)
                 .Include(i => i.Cai);

            // Filtro por rango de fechas
            if (fromDate.HasValue)
            {
                Query.Where(invoice => invoice.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Incluir todo el día final
                var endDate = toDate.Value.Date.AddDays(1);
                Query.Where(invoice => invoice.Date < endDate);
            }

            // Filtro por RTN del cliente
            if (!string.IsNullOrWhiteSpace(customerRtn))
            {
                Query.Where(invoice =>
                    invoice.Customer != null &&
                    invoice.Customer.RTN != null &&
                    invoice.Customer.RTN.Contains(customerRtn));
            }

            // Filtro por nombre del cliente
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                Query.Where(invoice =>
                    invoice.Customer != null &&
                    invoice.Customer.Name.Contains(customerName));
            }

            // Filtro por número de factura
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
            {
                Query.Where(invoice => invoice.InvoiceNumber.Contains(invoiceNumber));
            }

            // Filtro por nombre del usuario
            if (!string.IsNullOrWhiteSpace(userName))
            {
                Query.Where(invoice =>
                    invoice.User != null &&
                    (invoice.User.Name.Contains(userName) ||
                     invoice.User.LastName.Contains(userName)));
            }

            // Búsqueda general (searchTerm busca en todos los campos)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(invoice =>
                    invoice.InvoiceNumber.Contains(searchTerm) ||
                    (invoice.Customer != null && invoice.Customer.Name.Contains(searchTerm)) ||
                    (invoice.Customer != null && invoice.Customer.RTN != null && invoice.Customer.RTN.Contains(searchTerm)) ||
                    (invoice.User != null && invoice.User.Name.Contains(searchTerm)) ||
                    (invoice.User != null && invoice.User.LastName.Contains(searchTerm)));
            }

            // Ordenar por fecha descendente (más recientes primero)
            Query.OrderByDescending(invoice => invoice.Date)
                 .ThenByDescending(invoice => invoice.Id);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountInvoicesSpecification : Specification<Invoice>
    {
        public CountInvoicesSpecification(
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? customerRtn = null,
            string? customerName = null,
            string? invoiceNumber = null,
            string? userName = null)
        {
            // Filtro por rango de fechas
            if (fromDate.HasValue)
            {
                Query.Where(invoice => invoice.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date.AddDays(1);
                Query.Where(invoice => invoice.Date < endDate);
            }

            // Filtro por RTN del cliente
            if (!string.IsNullOrWhiteSpace(customerRtn))
            {
                Query.Where(invoice =>
                    invoice.Customer != null &&
                    invoice.Customer.RTN != null &&
                    invoice.Customer.RTN.Contains(customerRtn));
            }

            // Filtro por nombre del cliente
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                Query.Where(invoice =>
                    invoice.Customer != null &&
                    invoice.Customer.Name.Contains(customerName));
            }

            // Filtro por número de factura
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
            {
                Query.Where(invoice => invoice.InvoiceNumber.Contains(invoiceNumber));
            }

            // Filtro por nombre del usuario
            if (!string.IsNullOrWhiteSpace(userName))
            {
                Query.Where(invoice =>
                    invoice.User != null &&
                    (invoice.User.Name.Contains(userName) ||
                     invoice.User.LastName.Contains(userName)));
            }

            // Búsqueda general
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(invoice =>
                    invoice.InvoiceNumber.Contains(searchTerm) ||
                    (invoice.Customer != null && invoice.Customer.Name.Contains(searchTerm)) ||
                    (invoice.Customer != null && invoice.Customer.RTN != null && invoice.Customer.RTN.Contains(searchTerm)) ||
                    (invoice.User != null && invoice.User.Name.Contains(searchTerm)) ||
                    (invoice.User != null && invoice.User.LastName.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetInvoiceByIdSpecification : Specification<Invoice>, ISingleResultSpecification<Invoice>
    {
        public GetInvoiceByIdSpecification(int id)
        {
            Query.Where(invoice => invoice.Id == id)
                 .Include(i => i.Customer)
                 .Include(i => i.User)
                 .Include(i => i.Cai)
                 .Include(i => i.Discount)
                 .Include(i => i.SoldProducts!)
                    .ThenInclude(sp => sp.Product)
                 .Include(i => i.SoldProducts!)
                    .ThenInclude(sp => sp.Tax)
                 .Include(i => i.SoldProducts!)
                    .ThenInclude(sp => sp.Discount)
                 .Include(i => i.InvoicePayments!)
                    .ThenInclude(ip => ip.PaymentType);
        }
    }
}
