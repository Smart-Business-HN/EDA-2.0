using FluentValidation;
using System.Text.RegularExpressions;

namespace EDA.APPLICATION.Features.ProviderFeature.Commands.CreateProviderCommand
{
    public class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
    {
        private static readonly Regex RtnRegex = new Regex(@"^\d{14}$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new Regex(@"^\d{4}-\d{4}$", RegexOptions.Compiled);

        public CreateProviderCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del proveedor es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

            RuleFor(x => x.RTN)
                .NotEmpty().WithMessage("El RTN es requerido.")
                .MaximumLength(20).WithMessage("El RTN no puede exceder 20 caracteres.")
                .Must(rtn => RtnRegex.IsMatch(rtn ?? ""))
                .WithMessage("El RTN debe tener exactamente 14 digitos numericos.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("El telefono no puede exceder 20 caracteres.")
                .Must(phone => PhoneRegex.IsMatch(phone ?? ""))
                .WithMessage("El numero de telefono debe tener el formato ####-#### (ej: 8818-7765).")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El formato del correo electronico no es valido.")
                .MaximumLength(100).WithMessage("El correo no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.ContactPerson)
                .MaximumLength(100).WithMessage("El nombre del contacto no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.ContactPerson));

            RuleFor(x => x.ContactPhoneNumber)
                .MaximumLength(20).WithMessage("El telefono del contacto no puede exceder 20 caracteres.")
                .Must(phone => PhoneRegex.IsMatch(phone ?? ""))
                .WithMessage("El telefono de contacto debe tener el formato ####-#### (ej: 8818-7765).")
                .When(x => !string.IsNullOrEmpty(x.ContactPhoneNumber));

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("El formato del correo del contacto no es valido.")
                .MaximumLength(100).WithMessage("El correo del contacto no puede exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("La direccion no puede exceder 200 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.WebsiteUrl)
                .MaximumLength(200).WithMessage("La URL del sitio web no puede exceder 200 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("El usuario creador es requerido.");
        }
    }
}
