using FluentValidation;

namespace EDA.APPLICATION.Features.TaxFeature.Commands.CreateTaxCommand
{
    public class CreateTaxCommandValidator : AbstractValidator<CreateTaxCommand>
    {
        public CreateTaxCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del impuesto no puede estar vacío.")
                .MaximumLength(50).WithMessage("El nombre del impuesto no puede exceder los 50 caracteres.");

            RuleFor(x => x.Percentage)
                .GreaterThanOrEqualTo(0).WithMessage("El porcentaje debe ser mayor o igual a 0.")
                .LessThanOrEqualTo(100).WithMessage("El porcentaje no puede ser mayor a 100.");
        }
    }
}
