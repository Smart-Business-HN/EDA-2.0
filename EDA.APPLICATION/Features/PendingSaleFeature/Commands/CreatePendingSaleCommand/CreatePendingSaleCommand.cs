using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Commands.CreatePendingSaleCommand
{
    public class CreatePendingSaleCommand : IRequest<Result<PendingSale>>
    {
        public string DisplayName { get; set; } = null!;
        public string JsonData { get; set; } = null!;
        public int UserId { get; set; }
    }

    public class CreatePendingSaleCommandHandler : IRequestHandler<CreatePendingSaleCommand, Result<PendingSale>>
    {
        private readonly IRepositoryAsync<PendingSale> _repositoryAsync;

        public CreatePendingSaleCommandHandler(IRepositoryAsync<PendingSale> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PendingSale>> Handle(CreatePendingSaleCommand request, CancellationToken cancellationToken)
        {
            var pendingSale = new PendingSale
            {
                DisplayName = request.DisplayName,
                JsonData = request.JsonData,
                UserId = request.UserId,
                CreatedAt = DateTime.Now
            };

            await _repositoryAsync.AddAsync(pendingSale, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PendingSale>(pendingSale);
        }
    }
}
