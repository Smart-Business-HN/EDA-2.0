using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PendingSaleSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Queries
{
    public class GetPendingSalesByUserIdQuery : IRequest<Result<List<PendingSale>>>
    {
        public int UserId { get; set; }
    }

    public class GetPendingSalesByUserIdQueryHandler : IRequestHandler<GetPendingSalesByUserIdQuery, Result<List<PendingSale>>>
    {
        private readonly IRepositoryAsync<PendingSale> _repositoryAsync;

        public GetPendingSalesByUserIdQueryHandler(IRepositoryAsync<PendingSale> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<List<PendingSale>>> Handle(GetPendingSalesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var pendingSales = await _repositoryAsync.ListAsync(
                new GetPendingSalesByUserIdSpecification(request.UserId),
                cancellationToken);

            return new Result<List<PendingSale>>(pendingSales);
        }
    }
}
