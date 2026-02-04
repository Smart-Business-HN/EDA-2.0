using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CaiSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CaiFeature.Queries
{
    public class GetActiveCaiQuery : IRequest<Result<Cai?>>
    {
    }

    public class GetActiveCaiQueryHandler : IRequestHandler<GetActiveCaiQuery, Result<Cai?>>
    {
        private readonly IRepositoryAsync<Cai> _repositoryAsync;

        public GetActiveCaiQueryHandler(IRepositoryAsync<Cai> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Cai?>> Handle(GetActiveCaiQuery request, CancellationToken cancellationToken)
        {
            var cai = await _repositoryAsync.FirstOrDefaultAsync(
                new GetActiveCaiSpecification(),
                cancellationToken);

            return new Result<Cai?>(cai);
        }
    }
}
