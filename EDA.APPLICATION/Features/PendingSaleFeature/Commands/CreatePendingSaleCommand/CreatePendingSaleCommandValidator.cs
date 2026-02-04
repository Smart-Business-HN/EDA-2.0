using FluentValidation;

namespace EDA.APPLICATION.Features.PendingSaleFeature.Commands.CreatePendingSaleCommand
{
    public class CreatePendingSaleCommandValidator : AbstractValidator<CreatePendingSaleCommand>
    {
        public CreatePendingSaleCommandValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("El nombre de la venta es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.JsonData)
                .NotEmpty().WithMessage("Los datos de la venta son requeridos.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Usuario invalido.");
        }
    }
}
