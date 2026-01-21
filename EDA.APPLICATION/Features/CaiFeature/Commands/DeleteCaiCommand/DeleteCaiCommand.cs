using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CaiFeature.Commands.DeleteCaiCommand
{
    public class DeleteCaiCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteCaiCommandHandler : IRequestHandler<DeleteCaiCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Cai> _repositoryAsync;

        public DeleteCaiCommandHandler(IRepositoryAsync<Cai> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteCaiCommand request, CancellationToken cancellationToken)
        {
            var cai = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (cai == null)
            {
                return new Result<bool>("CAI no encontrado.");
            }

            // Verificar si el CAI ya tiene facturas emitidas
            if (cai.CurrentCorrelative > cai.InitialCorrelative)
            {
                return new Result<bool>("No se puede eliminar el CAI porque ya tiene facturas emitidas. Considere desactivarlo en su lugar.");
            }

            await _repositoryAsync.DeleteAsync(cai, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
