using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand
{
    public class UpdateShiftCommand : IRequest<Result<Shift>>
    {
        public int Id { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, Result<Shift>>
    {
        private readonly IRepositoryAsync<Shift> _repositoryAsync;

        public UpdateShiftCommandHandler(IRepositoryAsync<Shift> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Shift>> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
        {
            var shift = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (shift == null)
            {
                return new Result<Shift>("Turno no encontrado.");
            }

            if (!shift.IsOpen)
            {
                return new Result<Shift>("El turno ya esta cerrado.");
            }

            shift.EndTime = DateTime.Now;
            shift.FinalAmount = request.FinalAmount;
            shift.Difference = request.FinalAmount - shift.InitialAmount;
            shift.IsOpen = false;

            await _repositoryAsync.UpdateAsync(shift, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Shift>(shift);
        }
    }
}
