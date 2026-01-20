using FluentValidation;

namespace EDA.APPLICATION.Features.FamilyFeature.Commands.CreateFamilyCommand
{
    public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
    {
        public CreateFamilyCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la familia no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre de la familia no puede exceder los 100 caracteres.");
        }
    }
}
