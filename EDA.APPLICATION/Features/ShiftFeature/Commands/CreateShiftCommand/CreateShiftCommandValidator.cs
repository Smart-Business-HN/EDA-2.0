using FluentValidation;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.CreateShiftCommand
{
    public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
    {
        public CreateShiftCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Debe seleccionar un usuario.");

            RuleFor(x => x.ShiftType)
                .NotEmpty().WithMessage("Debe seleccionar un tipo de turno.");

            RuleFor(x => x.InitialAmount)
                .GreaterThanOrEqualTo(0).WithMessage("El monto inicial debe ser mayor o igual a 0.");
        }
    }
}
