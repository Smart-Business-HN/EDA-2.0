using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ShiftSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CashRegisterFeature.Commands.DeleteCashRegisterCommand
{
    public class DeleteCashRegisterCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteCashRegisterCommandHandler : IRequestHandler<DeleteCashRegisterCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;
        private readonly IRepositoryAsync<Shift> _shiftRepository;

        public DeleteCashRegisterCommandHandler(
            IRepositoryAsync<CashRegister> repositoryAsync,
            IRepositoryAsync<Shift> shiftRepository)
        {
            _repositoryAsync = repositoryAsync;
            _shiftRepository = shiftRepository;
        }

        public async Task<Result<bool>> Handle(DeleteCashRegisterCommand request, CancellationToken cancellationToken)
        {
            var cashRegister = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (cashRegister == null)
            {
                return new Result<bool>("Caja registradora no encontrada.");
            }

            // Check if any shift is using this cash register
            var shifts = await _shiftRepository.ListAsync(cancellationToken);
            var hasAssociatedShifts = shifts.Any(s => s.CashRegisterId == request.Id);

            if (hasAssociatedShifts)
            {
                return new Result<bool>("No se puede eliminar esta caja porque tiene turnos asociados.");
            }

            // Soft delete by setting IsActive = false
            cashRegister.IsActive = false;
            await _repositoryAsync.UpdateAsync(cashRegister, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
