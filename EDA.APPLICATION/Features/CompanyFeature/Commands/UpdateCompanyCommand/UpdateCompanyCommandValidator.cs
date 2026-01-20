using FluentValidation;

namespace EDA.APPLICATION.Features.CompanyFeature.Commands.UpdateCompanyCommand
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la empresa no puede estar vacío.")
                .MaximumLength(200).WithMessage("El nombre de la empresa no puede exceder los 200 caracteres.");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("El nombre del propietario no puede estar vacío.")
                .MaximumLength(200).WithMessage("El nombre del propietario no puede exceder los 200 caracteres.");

            RuleFor(x => x.Address1)
                .MaximumLength(500).WithMessage("La dirección 1 no puede exceder los 500 caracteres.");

            RuleFor(x => x.Address2)
                .MaximumLength(500).WithMessage("La dirección 2 no puede exceder los 500 caracteres.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber1)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("El teléfono 1 contiene caracteres no válidos.")
                .MaximumLength(20).WithMessage("El teléfono 1 no puede exceder los 20 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber1));

            RuleFor(x => x.PhoneNumber2)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("El teléfono 2 contiene caracteres no válidos.")
                .MaximumLength(20).WithMessage("El teléfono 2 no puede exceder los 20 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber2));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("La descripción no puede exceder los 1000 caracteres.");
        }
    }
}
