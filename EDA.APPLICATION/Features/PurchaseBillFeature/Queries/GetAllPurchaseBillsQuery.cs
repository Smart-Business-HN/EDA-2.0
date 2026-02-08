using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PurchaseBillSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Queries
{
    public class GetAllPurchaseBillsQuery : IRequest<Result<PaginatedResult<PurchaseBill>>>
    {
        public string? SearchTerm { get; set; }
        public int? ProviderId { get; set; }
        public int? ExpenseAccountId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllPurchaseBillsQueryHandler : IRequestHandler<GetAllPurchaseBillsQuery, Result<PaginatedResult<PurchaseBill>>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _repositoryAsync;

        public GetAllPurchaseBillsQueryHandler(IRepositoryAsync<PurchaseBill> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<PurchaseBill>>> Handle(GetAllPurchaseBillsQuery request, CancellationToken cancellationToken)
        {
            var purchaseBills = await _repositoryAsync.ListAsync(
                new FilterPurchaseBillsSpecification(
                    request.SearchTerm,
                    request.ProviderId,
                    request.ExpenseAccountId,
                    request.StatusId,
                    request.FromDate,
                    request.ToDate,
                    request.PageNumber,
                    request.PageSize),
                cancellationToken);

            var totalCount = await _repositoryAsync.CountAsync(
                new CountPurchaseBillsSpecification(
                    request.SearchTerm,
                    request.ProviderId,
                    request.ExpenseAccountId,
                    request.StatusId,
                    request.FromDate,
                    request.ToDate),
                cancellationToken);

            var paginatedResult = new PaginatedResult<PurchaseBill>(purchaseBills, totalCount, request.PageNumber, request.PageSize);
            return new Result<PaginatedResult<PurchaseBill>>(paginatedResult);
        }
    }
}
