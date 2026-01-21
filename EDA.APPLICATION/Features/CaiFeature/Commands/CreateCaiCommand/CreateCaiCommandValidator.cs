using FluentValidation;
using System.Text.RegularExpressions;

namespace EDA.APPLICATION.Features.CaiFeature.Commands.CreateCaiCommand
{
    public class CreateCaiCommandValidator : AbstractValidator<CreateCaiCommand>
    {
        // Regex para Code: XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX (6 grupos de 6 hex + 1 grupo de 2 hex)
        // Ejemplo: "489D01-9282C4-A74690-B28A5F-07F128-D5"
        private static readonly Regex CodeRegex = new Regex(
            @"^[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{2}$",
            RegexOptions.Compiled);

        // Regex para Prefix: XXX-XXX-XX- (3 dígitos - 3 dígitos - 2 dígitos - )
        // Ejemplo: "000-002-01-"
        private static readonly Regex PrefixRegex = new Regex(
            @"^\d{3}-\d{3}-\d{2}-$",
            RegexOptions.Compiled);

        public CreateCaiCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del CAI no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre del CAI no puede exceder los 100 caracteres.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("El código del CAI no puede estar vacío.")
                .MaximumLength(100).WithMessage("El código del CAI no puede exceder los 100 caracteres.")
                .Must(BeValidCode).WithMessage("El código del CAI debe tener el formato: XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX (ejemplo: 489D01-9282C4-A74690-B28A5F-07F128-D5)");

            RuleFor(x => x.Prefix)
                .NotEmpty().WithMessage("El prefijo del CAI no puede estar vacío.")
                .MaximumLength(20).WithMessage("El prefijo del CAI no puede exceder los 20 caracteres.")
                .Must(BeValidPrefix).WithMessage("El prefijo debe tener el formato: XXX-XXX-XX- (ejemplo: 000-002-01-)");

            RuleFor(x => x.FromDate)
                .NotEmpty().WithMessage("La fecha de inicio es requerida.");

            RuleFor(x => x.ToDate)
                .NotEmpty().WithMessage("La fecha de fin es requerida.")
                .GreaterThan(x => x.FromDate).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

            RuleFor(x => x.InitialCorrelative)
                .GreaterThan(0).WithMessage("El correlativo inicial debe ser mayor a 0.");

            RuleFor(x => x.FinalCorrelative)
                .GreaterThan(0).WithMessage("El correlativo final debe ser mayor a 0.")
                .GreaterThan(x => x.InitialCorrelative).WithMessage("El correlativo final debe ser mayor al correlativo inicial.");
        }

        private bool BeValidCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return CodeRegex.IsMatch(code.ToUpperInvariant());
        }

        private bool BeValidPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return false;

            return PrefixRegex.IsMatch(prefix);
        }
    }
}
