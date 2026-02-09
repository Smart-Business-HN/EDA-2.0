using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.DeletePrinterConfigurationCommand
{
    public class DeletePrinterConfigurationCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeletePrinterConfigurationCommandHandler : IRequestHandler<DeletePrinterConfigurationCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<PrinterConfiguration> _repositoryAsync;
        private readonly IRepositoryAsync<CashRegister> _cashRegisterRepository;

        public DeletePrinterConfigurationCommandHandler(
            IRepositoryAsync<PrinterConfiguration> repositoryAsync,
            IRepositoryAsync<CashRegister> cashRegisterRepository)
        {
            _repositoryAsync = repositoryAsync;
            _cashRegisterRepository = cashRegisterRepository;
        }

        public async Task<Result<bool>> Handle(DeletePrinterConfigurationCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (configuration == null)
            {
                return new Result<bool>("Configuracion de impresora no encontrada.");
            }

            // Check if any cash register is using this printer configuration
            var cashRegisters = await _cashRegisterRepository.ListAsync(cancellationToken);
            var hasAssociatedCashRegisters = cashRegisters.Any(cr => cr.PrinterConfigurationId == request.Id);

            if (hasAssociatedCashRegisters)
            {
                return new Result<bool>("No se puede eliminar esta configuracion porque esta asociada a una o mas cajas registradoras.");
            }

            // Instead of hard delete, we'll soft delete by setting IsActive = false
            configuration.IsActive = false;
            await _repositoryAsync.UpdateAsync(configuration, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
