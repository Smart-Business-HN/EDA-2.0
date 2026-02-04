using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Commands.UpdatePendingSaleCommand
{
    public class UpdatePendingSaleCommand : IRequest<Result<PendingSale>>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string JsonData { get; set; } = null!;
    }

    public class UpdatePendingSaleCommandHandler : IRequestHandler<UpdatePendingSaleCommand, Result<PendingSale>>
    {
        private readonly IRepositoryAsync<PendingSale> _repositoryAsync;

        public UpdatePendingSaleCommandHandler(IRepositoryAsync<PendingSale> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PendingSale>> Handle(UpdatePendingSaleCommand request, CancellationToken cancellationToken)
        {
            var pendingSale = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (pendingSale == null)
            {
                return new Result<PendingSale>("Venta pendiente no encontrada.");
            }

            pendingSale.DisplayName = request.DisplayName;
            pendingSale.JsonData = request.JsonData;

            await _repositoryAsync.UpdateAsync(pendingSale, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PendingSale>(pendingSale);
        }
    }
}
