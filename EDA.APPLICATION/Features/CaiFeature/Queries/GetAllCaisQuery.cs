using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CaiSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CaiFeature.Queries
{
    public class GetAllCaisQuery : IRequest<Result<PaginatedResult<Cai>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllCaisQueryHandler : IRequestHandler<GetAllCaisQuery, Result<PaginatedResult<Cai>>>
    {
        private readonly IRepositoryAsync<Cai> _repositoryAsync;

        public GetAllCaisQueryHandler(IRepositoryAsync<Cai> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Cai>>> Handle(GetAllCaisQuery request, CancellationToken cancellationToken)
        {
            List<Cai> cais;
            int totalCount;

            if (request.GetAll)
            {
                cais = await _repositoryAsync.ListAsync(
                    new FilterCaisSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = cais.Count;

                var allResult = new PaginatedResult<Cai>(cais, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Cai>>(allResult);
            }
            else
            {
                cais = await _repositoryAsync.ListAsync(
                    new FilterCaisSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountCaisSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Cai>(cais, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Cai>>(paginatedResult);
            }
        }
    }
}
