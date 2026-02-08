using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ExpenseAccountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Queries
{
    public class GetAllExpenseAccountsQuery : IRequest<Result<PaginatedResult<ExpenseAccount>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllExpenseAccountsQueryHandler : IRequestHandler<GetAllExpenseAccountsQuery, Result<PaginatedResult<ExpenseAccount>>>
    {
        private readonly IRepositoryAsync<ExpenseAccount> _repositoryAsync;

        public GetAllExpenseAccountsQueryHandler(IRepositoryAsync<ExpenseAccount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<ExpenseAccount>>> Handle(GetAllExpenseAccountsQuery request, CancellationToken cancellationToken)
        {
            List<ExpenseAccount> expenseAccounts;
            int totalCount;

            if (request.GetAll)
            {
                expenseAccounts = await _repositoryAsync.ListAsync(
                    new FilterExpenseAccountsSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = expenseAccounts.Count;

                var allResult = new PaginatedResult<ExpenseAccount>(expenseAccounts, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<ExpenseAccount>>(allResult);
            }
            else
            {
                expenseAccounts = await _repositoryAsync.ListAsync(
                    new FilterExpenseAccountsSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountExpenseAccountsSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<ExpenseAccount>(expenseAccounts, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<ExpenseAccount>>(paginatedResult);
            }
        }
    }
}
