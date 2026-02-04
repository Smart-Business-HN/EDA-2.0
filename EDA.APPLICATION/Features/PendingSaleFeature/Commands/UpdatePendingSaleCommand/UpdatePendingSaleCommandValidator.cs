using FluentValidation;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Commands.UpdatePendingSaleCommand
{
    public class UpdatePendingSaleCommandValidator : AbstractValidator<UpdatePendingSaleCommand>
    {
        public UpdatePendingSaleCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID de venta invalido.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("El nombre de la venta es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.JsonData)
                .NotEmpty().WithMessage("Los datos de la venta son requeridos.");
        }
    }
}
