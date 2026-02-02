using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.DeleteShiftCommand
{
    public class DeleteShiftCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Shift> _repositoryAsync;

        public DeleteShiftCommandHandler(IRepositoryAsync<Shift> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
        {
            var shift = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (shift == null)
            {
                return new Result<bool>("Turno no encontrado.");
            }

            if (shift.IsOpen)
            {
                return new Result<bool>("No se puede eliminar un turno abierto. Cierre el turno primero.");
            }

            await _repositoryAsync.DeleteAsync(shift, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
