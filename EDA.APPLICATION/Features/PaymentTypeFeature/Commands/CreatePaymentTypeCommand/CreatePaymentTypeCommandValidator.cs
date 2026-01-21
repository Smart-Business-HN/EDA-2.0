using FluentValidation;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Commands.CreatePaymentTypeCommand
{
    public class CreatePaymentTypeCommandValidator : AbstractValidator<CreatePaymentTypeCommand>
    {
        public CreatePaymentTypeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del tipo de pago no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del tipo de pago no puede exceder los 100 caracteres.");
        }
    }
}
