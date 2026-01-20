using FluentValidation;

namespace EDA.APPLICATION.Features.FamilyFeature.Commands.UpdateFamilyCommand
{
    public class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
    {
        public UpdateFamilyCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la familia es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la familia no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre de la familia no puede exceder los 100 caracteres.");
        }
    }
}
