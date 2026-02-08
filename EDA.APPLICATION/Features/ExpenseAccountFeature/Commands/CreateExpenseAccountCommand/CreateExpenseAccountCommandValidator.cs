using FluentValidation;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.CreateExpenseAccountCommand
{
    public class CreateExpenseAccountCommandValidator : AbstractValidator<CreateExpenseAccountCommand>
    {
        public CreateExpenseAccountCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la cuenta de gastos es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");
        }
    }
}
