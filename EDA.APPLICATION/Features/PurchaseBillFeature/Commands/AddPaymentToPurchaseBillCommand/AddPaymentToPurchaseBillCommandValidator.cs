using FluentValidation;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.AddPaymentToPurchaseBillCommand
{
    public class AddPaymentToPurchaseBillCommandValidator : AbstractValidator<AddPaymentToPurchaseBillCommand>
    {
        public AddPaymentToPurchaseBillCommandValidator()
        {
            RuleFor(x => x.PurchaseBillId)
                .GreaterThan(0).WithMessage("El ID de la factura es invalido.");

            RuleFor(x => x.PaymentTypeId)
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de pago.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("El usuario es requerido.");
        }
    }
}
