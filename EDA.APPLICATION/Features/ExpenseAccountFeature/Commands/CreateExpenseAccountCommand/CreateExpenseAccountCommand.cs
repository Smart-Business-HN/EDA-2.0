using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ExpenseAccountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.CreateExpenseAccountCommand
{
    public class CreateExpenseAccountCommand : IRequest<Result<ExpenseAccount>>
    {
        public string Name { get; set; } = null!;
    }

    public class CreateExpenseAccountCommandHandler : IRequestHandler<CreateExpenseAccountCommand, Result<ExpenseAccount>>
    {
        private readonly IRepositoryAsync<ExpenseAccount> _repositoryAsync;

        public CreateExpenseAccountCommandHandler(IRepositoryAsync<ExpenseAccount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<ExpenseAccount>> Handle(CreateExpenseAccountCommand request, CancellationToken cancellationToken)
        {
            var existingAccount = await _repositoryAsync.FirstOrDefaultAsync(
                new GetExpenseAccountByNameSpecification(request.Name),
                cancellationToken);

            if (existingAccount != null)
            {
                return new Result<ExpenseAccount>("Ya existe una cuenta de gastos con este nombre.");
            }

            var newExpenseAccount = new ExpenseAccount
            {
                Name = request.Name
            };

            await _repositoryAsync.AddAsync(newExpenseAccount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<ExpenseAccount>(newExpenseAccount);
        }
    }
}
