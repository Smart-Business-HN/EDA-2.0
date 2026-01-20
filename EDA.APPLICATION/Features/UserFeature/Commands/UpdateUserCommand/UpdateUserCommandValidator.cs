using FluentValidation;

namespace EDA.APPLICATION.Features.UserFeature.Commands.UpdateUserCommand
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del usuario es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de usuario no puede estar vacío.")
                .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder los 50 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido no puede estar vacío.")
                .MaximumLength(100).WithMessage("El apellido no puede exceder los 100 caracteres.");

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder los 100 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Password));

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Debe seleccionar un rol válido.");
        }
    }
}
