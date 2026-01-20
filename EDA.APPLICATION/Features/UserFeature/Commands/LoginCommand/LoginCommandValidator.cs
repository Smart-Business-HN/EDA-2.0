using FluentValidation;

namespace EDA.APPLICATION.Features.UserFeature.Commands.LoginCommand
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator() {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario no puede estar vacío.")
                .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder los 50 caracteres.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña no puede estar vacía.")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder los 100 caracteres.");
        }
    }
}
