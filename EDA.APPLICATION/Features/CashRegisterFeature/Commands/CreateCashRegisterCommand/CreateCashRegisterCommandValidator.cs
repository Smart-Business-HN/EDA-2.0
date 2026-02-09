using FluentValidation;

namespace EDA.APPLICATION.Features.CashRegisterFeature.Commands.CreateCashRegisterCommand
{
    public class CreateCashRegisterCommandValidator : AbstractValidator<CreateCashRegisterCommand>
    {
        public CreateCashRegisterCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la caja es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El codigo de la caja es requerido.")
                .MaximumLength(20).WithMessage("El codigo no puede exceder 20 caracteres.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("El codigo solo puede contener letras y numeros.");

            RuleFor(x => x.PrinterConfigurationId)
                .GreaterThan(0).WithMessage("Debe seleccionar una configuracion de impresora.");
        }
    }
}
