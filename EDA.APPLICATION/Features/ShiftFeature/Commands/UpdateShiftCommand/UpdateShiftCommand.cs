using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand
{
    public class UpdateShiftCommand : IRequest<Result<Shift>>
    {
        public int Id { get; set; }
        public decimal FinalCashAmount { get; set; }
        public decimal FinalCardAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
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
            shift.FinalCashAmount = request.FinalCashAmount;
            shift.FinalCardAmount = request.FinalCardAmount;
            shift.FinalAmount = request.FinalCashAmount + request.FinalCardAmount + shift.InitialAmount;
            shift.ExpectedAmount = request.ExpectedAmount;
            shift.Difference = shift.ExpectedAmount - shift.FinalAmount;
            shift.IsOpen = false;

            await _repositoryAsync.UpdateAsync(shift, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Shift>(shift);
        }
    }
}
