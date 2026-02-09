using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CashRegisterSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CashRegisterFeature.Commands.UpdateCashRegisterCommand
{
    public class UpdateCashRegisterCommand : IRequest<Result<CashRegister>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int PrinterConfigurationId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateCashRegisterCommandHandler : IRequestHandler<UpdateCashRegisterCommand, Result<CashRegister>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;
        private readonly IRepositoryAsync<PrinterConfiguration> _printerConfigRepository;

        public UpdateCashRegisterCommandHandler(
            IRepositoryAsync<CashRegister> repositoryAsync,
            IRepositoryAsync<PrinterConfiguration> printerConfigRepository)
        {
            _repositoryAsync = repositoryAsync;
            _printerConfigRepository = printerConfigRepository;
        }

        public async Task<Result<CashRegister>> Handle(UpdateCashRegisterCommand request, CancellationToken cancellationToken)
        {
            var cashRegister = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (cashRegister == null)
            {
                return new Result<CashRegister>("Caja registradora no encontrada.");
            }

            // Check if code is being changed and if new code already exists
            if (cashRegister.Code != request.Code.ToUpper())
            {
                var existingByCode = await _repositoryAsync.FirstOrDefaultAsync(
                    new GetCashRegisterByCodeSpecification(request.Code),
                    cancellationToken);

                if (existingByCode != null && existingByCode.Id != request.Id)
                {
                    return new Result<CashRegister>("Ya existe una caja registradora con este codigo.");
                }
            }

            // Check if name is being changed and if new name already exists
            if (cashRegister.Name != request.Name)
            {
                var existingByName = await _repositoryAsync.FirstOrDefaultAsync(
                    new GetCashRegisterByNameSpecification(request.Name),
                    cancellationToken);

                if (existingByName != null && existingByName.Id != request.Id)
                {
                    return new Result<CashRegister>("Ya existe una caja registradora con este nombre.");
                }
            }

            // Verify printer configuration exists
            var printerConfig = await _printerConfigRepository.GetByIdAsync(request.PrinterConfigurationId, cancellationToken);
            if (printerConfig == null)
            {
                return new Result<CashRegister>("La configuracion de impresora seleccionada no existe.");
            }

            cashRegister.Name = request.Name;
            cashRegister.Code = request.Code.ToUpper();
            cashRegister.PrinterConfigurationId = request.PrinterConfigurationId;
            cashRegister.IsActive = request.IsActive;

            await _repositoryAsync.UpdateAsync(cashRegister, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<CashRegister>(cashRegister);
        }
    }
}
