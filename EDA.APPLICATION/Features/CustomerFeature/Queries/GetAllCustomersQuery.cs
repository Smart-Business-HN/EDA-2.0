using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CustomerSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CustomerFeature.Queries
{
    public class GetAllCustomersQuery : IRequest<Result<PaginatedResult<Customer>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<PaginatedResult<Customer>>>
    {
        private readonly IRepositoryAsync<Customer> _repositoryAsync;

        public GetAllCustomersQueryHandler(IRepositoryAsync<Customer> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Customer>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            List<Customer> customers;
            int totalCount;

            if (request.GetAll)
            {
                customers = await _repositoryAsync.ListAsync(
                    new FilterCustomersSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = customers.Count;

                var allResult = new PaginatedResult<Customer>(customers, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Customer>>(allResult);
            }
            else
            {
                customers = await _repositoryAsync.ListAsync(
                    new FilterCustomersSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountCustomersSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Customer>(customers, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Customer>>(paginatedResult);
            }
        }
    }
}
