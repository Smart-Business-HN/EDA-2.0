using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PrinterConfigurationSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.UpdatePrinterConfigurationCommand
{
    public class UpdatePrinterConfigurationCommand : IRequest<Result<PrinterConfiguration>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int PrinterType { get; set; }
        public string? PrinterName { get; set; }
        public int FontSize { get; set; }
        public int CopyStrategy { get; set; }
        public int CopiesCount { get; set; }
        public int PrintWidth { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdatePrinterConfigurationCommandHandler : IRequestHandler<UpdatePrinterConfigurationCommand, Result<PrinterConfiguration>>
    {
        private readonly IRepositoryAsync<PrinterConfiguration> _repositoryAsync;

        public UpdatePrinterConfigurationCommandHandler(IRepositoryAsync<PrinterConfiguration> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PrinterConfiguration>> Handle(UpdatePrinterConfigurationCommand request, CancellationToken cancellationToken)
        {
            var configuration = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (configuration == null)
            {
                return new Result<PrinterConfiguration>("Configuracion de impresora no encontrada.");
            }

            // Check if name is being changed and if new name already exists
            if (configuration.Name != request.Name)
            {
                var existingConfig = await _repositoryAsync.FirstOrDefaultAsync(
                    new GetPrinterConfigurationByNameSpecification(request.Name),
                    cancellationToken);

                if (existingConfig != null && existingConfig.Id != request.Id)
                {
                    return new Result<PrinterConfiguration>("Ya existe una configuracion de impresora con este nombre.");
                }
            }

            configuration.Name = request.Name;
            configuration.PrinterType = request.PrinterType;
            configuration.PrinterName = request.PrinterName;
            configuration.FontSize = request.FontSize;
            configuration.CopyStrategy = request.CopyStrategy;
            configuration.CopiesCount = request.CopiesCount;
            configuration.PrintWidth = request.PrintWidth;
            configuration.IsActive = request.IsActive;

            await _repositoryAsync.UpdateAsync(configuration, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PrinterConfiguration>(configuration);
        }
    }
}
