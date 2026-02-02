using FluentValidation;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand
{
    public class UpdateShiftCommandValidator : AbstractValidator<UpdateShiftCommand>
    {
        public UpdateShiftCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Turno invalido.");

            RuleFor(x => x.FinalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("El monto final debe ser mayor o igual a 0.");
        }
    }
}
