using FluentValidation;

namespace EDA.APPLICATION.Features.TaxFeature.Commands.UpdateTaxCommand
{
    public class UpdateTaxCommandValidator : AbstractValidator<UpdateTaxCommand>
    {
        public UpdateTaxCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del impuesto es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del impuesto no puede estar vacío.")
                .MaximumLength(50).WithMessage("El nombre del impuesto no puede exceder los 50 caracteres.");

            RuleFor(x => x.Percentage)
                .GreaterThanOrEqualTo(0).WithMessage("El porcentaje debe ser mayor o igual a 0.")
                .LessThanOrEqualTo(100).WithMessage("El porcentaje no puede ser mayor a 100.");
        }
    }
}
