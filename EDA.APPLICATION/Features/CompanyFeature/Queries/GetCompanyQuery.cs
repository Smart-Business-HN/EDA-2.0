using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CompanySpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CompanyFeature.Queries
{
    public class GetCompanyQuery : IRequest<Result<Company>>
    {
    }

    public class GetCompanyQueryHandler : IRequestHandler<GetCompanyQuery, Result<Company>>
    {
        private readonly IRepositoryAsync<Company> _repositoryAsync;

        public GetCompanyQueryHandler(IRepositoryAsync<Company> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Company>> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
        {
            var company = await _repositoryAsync.FirstOrDefaultAsync(new GetCompanySpecification(), cancellationToken);

            if (company == null)
            {
                return new Result<Company>(new Company());
            }

            return new Result<Company>(company);
        }
    }
}
