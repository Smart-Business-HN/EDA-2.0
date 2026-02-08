using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ExpenseAccountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.UpdateExpenseAccountCommand
{
    public class UpdateExpenseAccountCommand : IRequest<Result<ExpenseAccount>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class UpdateExpenseAccountCommandHandler : IRequestHandler<UpdateExpenseAccountCommand, Result<ExpenseAccount>>
    {
        private readonly IRepositoryAsync<ExpenseAccount> _repositoryAsync;

        public UpdateExpenseAccountCommandHandler(IRepositoryAsync<ExpenseAccount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<ExpenseAccount>> Handle(UpdateExpenseAccountCommand request, CancellationToken cancellationToken)
        {
            var expenseAccount = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (expenseAccount == null)
            {
                return new Result<ExpenseAccount>("Cuenta de gastos no encontrada.");
            }

            // Verificar que el nombre no exista en otra cuenta
            var existingAccount = await _repositoryAsync.FirstOrDefaultAsync(
                new GetExpenseAccountByNameSpecification(request.Name),
                cancellationToken);

            if (existingAccount != null && existingAccount.Id != request.Id)
            {
                return new Result<ExpenseAccount>("Ya existe otra cuenta de gastos con este nombre.");
            }

            expenseAccount.Name = request.Name;

            await _repositoryAsync.UpdateAsync(expenseAccount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<ExpenseAccount>(expenseAccount);
        }
    }
}
