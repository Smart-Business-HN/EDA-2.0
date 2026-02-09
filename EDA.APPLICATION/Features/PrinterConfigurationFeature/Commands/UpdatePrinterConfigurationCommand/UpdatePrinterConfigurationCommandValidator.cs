using FluentValidation;

namespace EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.UpdatePrinterConfigurationCommand
{
    public class UpdatePrinterConfigurationCommandValidator : AbstractValidator<UpdatePrinterConfigurationCommand>
    {
        public UpdatePrinterConfigurationCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la configuracion es requerido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la configuracion es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

            RuleFor(x => x.PrinterType)
                .InclusiveBetween(1, 2).WithMessage("El tipo de impresora debe ser Termica (1) o Matricial (2).");

            RuleFor(x => x.PrinterName)
                .MaximumLength(200).WithMessage("El nombre de la impresora no puede exceder 200 caracteres.");

            RuleFor(x => x.FontSize)
                .InclusiveBetween(6, 16).WithMessage("El tamano de fuente debe estar entre 6 y 16.");

            RuleFor(x => x.CopyStrategy)
                .InclusiveBetween(1, 4).WithMessage("La estrategia de copias debe ser valida (1-4).");

            RuleFor(x => x.CopiesCount)
                .InclusiveBetween(1, 5).WithMessage("El numero de copias debe estar entre 1 y 5.");

            RuleFor(x => x.PrintWidth)
                .InclusiveBetween(48, 210).WithMessage("El ancho de papel debe estar entre 48mm y 210mm.");
        }
    }
}
