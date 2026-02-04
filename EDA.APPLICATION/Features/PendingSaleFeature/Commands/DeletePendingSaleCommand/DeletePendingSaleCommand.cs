using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Commands.DeletePendingSaleCommand
{
    public class DeletePendingSaleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeletePendingSaleCommandHandler : IRequestHandler<DeletePendingSaleCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<PendingSale> _repositoryAsync;

        public DeletePendingSaleCommandHandler(IRepositoryAsync<PendingSale> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeletePendingSaleCommand request, CancellationToken cancellationToken)
        {
            var pendingSale = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (pendingSale == null)
            {
                return new Result<bool>("Venta pendiente no encontrada.");
            }

            await _repositoryAsync.DeleteAsync(pendingSale, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
