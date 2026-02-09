using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CashRegisterSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CashRegisterFeature.Commands.CreateCashRegisterCommand
{
    public class CreateCashRegisterCommand : IRequest<Result<CashRegister>>
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int PrinterConfigurationId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateCashRegisterCommandHandler : IRequestHandler<CreateCashRegisterCommand, Result<CashRegister>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;
        private readonly IRepositoryAsync<PrinterConfiguration> _printerConfigRepository;

        public CreateCashRegisterCommandHandler(
            IRepositoryAsync<CashRegister> repositoryAsync,
            IRepositoryAsync<PrinterConfiguration> printerConfigRepository)
        {
            _repositoryAsync = repositoryAsync;
            _printerConfigRepository = printerConfigRepository;
        }

        public async Task<Result<CashRegister>> Handle(CreateCashRegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if code already exists
            var existingByCode = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCashRegisterByCodeSpecification(request.Code),
                cancellationToken);

            if (existingByCode != null)
            {
                return new Result<CashRegister>("Ya existe una caja registradora con este codigo.");
            }

            // Check if name already exists
            var existingByName = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCashRegisterByNameSpecification(request.Name),
                cancellationToken);

            if (existingByName != null)
            {
                return new Result<CashRegister>("Ya existe una caja registradora con este nombre.");
            }

            // Verify printer configuration exists
            var printerConfig = await _printerConfigRepository.GetByIdAsync(request.PrinterConfigurationId, cancellationToken);
            if (printerConfig == null)
            {
                return new Result<CashRegister>("La configuracion de impresora seleccionada no existe.");
            }

            var newCashRegister = new CashRegister
            {
                Name = request.Name,
                Code = request.Code.ToUpper(),
                PrinterConfigurationId = request.PrinterConfigurationId,
                IsActive = request.IsActive,
                CreationDate = DateTime.Now
            };

            await _repositoryAsync.AddAsync(newCashRegister, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<CashRegister>(newCashRegister);
        }
    }
}
