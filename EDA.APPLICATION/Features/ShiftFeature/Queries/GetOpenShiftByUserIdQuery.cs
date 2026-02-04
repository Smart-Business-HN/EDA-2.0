using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ShiftSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Queries
{
    public class GetOpenShiftByUserIdQuery : IRequest<Result<Shift?>>
    {
        public int UserId { get; set; }
    }

    public class GetOpenShiftByUserIdQueryHandler : IRequestHandler<GetOpenShiftByUserIdQuery, Result<Shift?>>
    {
        private readonly IRepositoryAsync<Shift> _repositoryAsync;

        public GetOpenShiftByUserIdQueryHandler(IRepositoryAsync<Shift> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Shift?>> Handle(GetOpenShiftByUserIdQuery request, CancellationToken cancellationToken)
        {
            var shift = await _repositoryAsync.FirstOrDefaultAsync(
                new GetOpenShiftByUserIdSpecification(request.UserId),
                cancellationToken);

            return new Result<Shift?>(shift);
        }
    }
}
