using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.FamilySpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.FamilyFeature.Queries
{
    public class GetAllFamiliesQuery : IRequest<Result<PaginatedResult<Family>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllFamiliesQueryHandler : IRequestHandler<GetAllFamiliesQuery, Result<PaginatedResult<Family>>>
    {
        private readonly IRepositoryAsync<Family> _repositoryAsync;

        public GetAllFamiliesQueryHandler(IRepositoryAsync<Family> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Family>>> Handle(GetAllFamiliesQuery request, CancellationToken cancellationToken)
        {
            List<Family> families;
            int totalCount;

            if (request.GetAll)
            {
                // Obtener todas las familias sin paginación
                families = await _repositoryAsync.ListAsync(
                    new FilterFamiliesSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = families.Count;

                var allResult = new PaginatedResult<Family>(families, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Family>>(allResult);
            }
            else
            {
                // Obtener con paginación
                families = await _repositoryAsync.ListAsync(
                    new FilterFamiliesSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountFamiliesSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Family>(families, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Family>>(paginatedResult);
            }
        }
    }
}
