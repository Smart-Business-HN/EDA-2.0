using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.RoleSpecifications;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.RoleFeature.Queries
{
    public class GetAllRolesQuery : IRequest<Result<List<Role>>>
    {
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<Role>>>
    {
        private readonly IRepositoryAsync<Role> _repositoryAsync;

        public GetAllRolesQueryHandler(IRepositoryAsync<Role> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<List<Role>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _repositoryAsync.ListAsync(new GetAllRolesSpecification(), cancellationToken);
            return new Result<List<Role>>(roles);
        }
    }
}
