using FluentValidation;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand
{
    public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
    {
        public UpdateShiftCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Turno invalido.");

            RuleFor(x => x.FinalCashAmount)
                .GreaterThanOrEqualTo(0).WithMessage("El efectivo debe ser mayor o igual a 0.");

            RuleFor(x => x.FinalCardAmount)
                .GreaterThanOrEqualTo(0).WithMessage("El monto en tarjeta debe ser mayor o igual a 0.");
        }
    }
}
