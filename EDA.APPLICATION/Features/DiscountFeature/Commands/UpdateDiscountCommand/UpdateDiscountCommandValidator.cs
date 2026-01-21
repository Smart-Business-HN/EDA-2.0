using FluentValidation;

namespace EDA.APPLICATION.Features.DiscountFeature.Commands.UpdateDiscountCommand
{
    public class UpdateDiscountCommandValidator : AbstractValidator<UpdateDiscountCommand>
    {
        public UpdateDiscountCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del descuento es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del descuento no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del descuento no puede exceder los 100 caracteres.");

            RuleFor(x => x.Percentage)
                .GreaterThanOrEqualTo(0).WithMessage("El porcentaje debe ser mayor o igual a 0.")
                .LessThanOrEqualTo(100).WithMessage("El porcentaje no puede ser mayor a 100.");
        }
    }
}
