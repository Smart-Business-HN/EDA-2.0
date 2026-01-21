using FluentValidation;

namespace EDA.APPLICATION.Features.CustomerFeature.Commands.CreateCustomerCommand
{
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del cliente no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del cliente no puede exceder los 100 caracteres.");

            RuleFor(x => x.Company)
                .MaximumLength(100).WithMessage("El nombre de la empresa no puede exceder los 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Company));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
                .MaximumLength(150).WithMessage("El correo electrónico no puede exceder los 150 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("El número de teléfono no puede exceder los 20 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
