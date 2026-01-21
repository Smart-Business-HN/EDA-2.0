using FluentValidation;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Commands.UpdatePaymentTypeCommand
{
    public class UpdatePaymentTypeCommandValidator : AbstractValidator<UpdatePaymentTypeCommand>
    {
        public UpdatePaymentTypeCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del tipo de pago es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del tipo de pago no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del tipo de pago no puede exceder los 100 caracteres.");
        }
    }
}
