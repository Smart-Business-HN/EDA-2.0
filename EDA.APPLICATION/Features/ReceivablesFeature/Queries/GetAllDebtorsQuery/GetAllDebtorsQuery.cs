using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ReceivablesSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ReceivablesFeature.Queries.GetAllDebtorsQuery
{
    public class GetAllDebtorsQuery : IRequest<Result<PaginatedResult<DebtorSummary>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDebtorsQueryHandler : IRequestHandler<GetAllDebtorsQuery, Result<PaginatedResult<DebtorSummary>>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;

        public GetAllDebtorsQueryHandler(IRepositoryAsync<Invoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Result<PaginatedResult<DebtorSummary>>> Handle(
            GetAllDebtorsQuery request,
            CancellationToken cancellationToken)
        {
            // Obtener todas las facturas pendientes
            var invoices = await _invoiceRepository.ListAsync(
                new GetDebtorsSpecification(request.SearchTerm),
                cancellationToken);

            var today = DateTime.Today;

            // Agrupar por cliente
            var debtorGroups = invoices
                .Where(i => i.Customer != null)
                .GroupBy(i => i.CustomerId)
                .Select(g =>
                {
                    var customer = g.First().Customer!;
                    var customerInvoices = g.ToList();

                    return new DebtorSummary
                    {
                        CustomerId = customer.Id,
                        CustomerName = customer.Name,
                        CustomerRtn = customer.RTN,
                        TotalOwed = customerInvoices.Sum(i => i.OutstandingAmount),
                        PendingInvoicesCount = customerInvoices.Count,
                        OverdueAmount = customerInvoices
                            .Where(i => i.DueDate.HasValue && i.DueDate.Value.Date < today)
                            .Sum(i => i.OutstandingAmount)
                    };
                })
                .OrderByDescending(d => d.TotalOwed)
                .ToList();

            // Paginacion
            var totalCount = debtorGroups.Count;
            var pagedItems = debtorGroups
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedResult = new PaginatedResult<DebtorSummary>(
                pagedItems, totalCount, request.PageNumber, request.PageSize);

            return new Result<PaginatedResult<DebtorSummary>>(paginatedResult);
        }
    }
}
