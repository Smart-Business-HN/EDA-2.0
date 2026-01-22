using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Queries.GetAllInvoicesQuery
{
    public class GetAllInvoicesQuery : IRequest<Result<PaginatedResult<Invoice>>>
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? CustomerRtn { get; set; }
        public string? CustomerName { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? UserName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, Result<PaginatedResult<Invoice>>>
    {
        private readonly IRepositoryAsync<Invoice> _repositoryAsync;

        public GetAllInvoicesQueryHandler(IRepositoryAsync<Invoice> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Invoice>>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _repositoryAsync.ListAsync(
                new FilterInvoicesSpecification(
                    request.SearchTerm,
                    request.FromDate,
                    request.ToDate,
                    request.CustomerRtn,
                    request.CustomerName,
                    request.InvoiceNumber,
                    request.UserName,
                    request.PageNumber,
                    request.PageSize),
                cancellationToken);

            var totalCount = await _repositoryAsync.CountAsync(
                new CountInvoicesSpecification(
                    request.SearchTerm,
                    request.FromDate,
                    request.ToDate,
                    request.CustomerRtn,
                    request.CustomerName,
                    request.InvoiceNumber,
                    request.UserName),
                cancellationToken);

            var paginatedResult = new PaginatedResult<Invoice>(invoices, totalCount, request.PageNumber, request.PageSize);
            return new Result<PaginatedResult<Invoice>>(paginatedResult);
        }
    }
}
