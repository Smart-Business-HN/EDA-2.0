using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PrinterConfigurationSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.CreatePrinterConfigurationCommand
{
    public class CreatePrinterConfigurationCommand : IRequest<Result<PrinterConfiguration>>
    {
        public string Name { get; set; } = null!;
        public int PrinterType { get; set; }
        public string? PrinterName { get; set; }
        public int FontSize { get; set; } = 8;
        public int CopyStrategy { get; set; }
        public int CopiesCount { get; set; } = 1;
        public int PrintWidth { get; set; } = 80;
        public bool IsActive { get; set; } = true;
    }

    public class CreatePrinterConfigurationCommandHandler : IRequestHandler<CreatePrinterConfigurationCommand, Result<PrinterConfiguration>>
    {
        private readonly IRepositoryAsync<PrinterConfiguration> _repositoryAsync;

        public CreatePrinterConfigurationCommandHandler(IRepositoryAsync<PrinterConfiguration> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PrinterConfiguration>> Handle(CreatePrinterConfigurationCommand request, CancellationToken cancellationToken)
        {
            var existingConfig = await _repositoryAsync.FirstOrDefaultAsync(
                new GetPrinterConfigurationByNameSpecification(request.Name),
                cancellationToken);

            if (existingConfig != null)
            {
                return new Result<PrinterConfiguration>("Ya existe una configuracion de impresora con este nombre.");
            }

            var newConfiguration = new PrinterConfiguration
            {
                Name = request.Name,
                PrinterType = request.PrinterType,
                PrinterName = request.PrinterName,
                FontSize = request.FontSize,
                CopyStrategy = request.CopyStrategy,
                CopiesCount = request.CopiesCount,
                PrintWidth = request.PrintWidth,
                IsActive = request.IsActive,
                CreationDate = DateTime.Now
            };

            await _repositoryAsync.AddAsync(newConfiguration, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PrinterConfiguration>(newConfiguration);
        }
    }
}
