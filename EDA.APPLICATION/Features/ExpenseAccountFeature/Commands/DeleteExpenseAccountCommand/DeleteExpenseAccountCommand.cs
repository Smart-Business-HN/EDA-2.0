using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.DeleteExpenseAccountCommand
{
    public class DeleteExpenseAccountCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteExpenseAccountCommandHandler : IRequestHandler<DeleteExpenseAccountCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<ExpenseAccount> _repositoryAsync;

        public DeleteExpenseAccountCommandHandler(IRepositoryAsync<ExpenseAccount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteExpenseAccountCommand request, CancellationToken cancellationToken)
        {
            var expenseAccount = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (expenseAccount == null)
            {
                return new Result<bool>("Cuenta de gastos no encontrada.");
            }

            await _repositoryAsync.DeleteAsync(expenseAccount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
