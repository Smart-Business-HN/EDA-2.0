using FluentValidation;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.UpdateExpenseAccountCommand
{
    public class UpdateExpenseAccountCommandValidator : AbstractValidator<UpdateExpenseAccountCommand>
    {
        public UpdateExpenseAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la cuenta de gastos es inválido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la cuenta de gastos es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");
        }
    }
}
