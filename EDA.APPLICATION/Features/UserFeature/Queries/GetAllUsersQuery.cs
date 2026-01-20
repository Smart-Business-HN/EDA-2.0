using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.UserSpecifications;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.UserFeature.Queries
{
    public class GetAllUsersQuery : IRequest<Result<PaginatedResult<User>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedResult<User>>>
    {
        private readonly IRepositoryAsync<User> _repositoryAsync;

        public GetAllUsersQueryHandler(IRepositoryAsync<User> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _repositoryAsync.ListAsync(
                new FilterUsersSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                cancellationToken);

            var totalCount = await _repositoryAsync.CountAsync(
                new CountUsersSpecification(request.SearchTerm),
                cancellationToken);

            var paginatedResult = new PaginatedResult<User>(
                users,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return new Result<PaginatedResult<User>>(paginatedResult);
        }
    }
}
