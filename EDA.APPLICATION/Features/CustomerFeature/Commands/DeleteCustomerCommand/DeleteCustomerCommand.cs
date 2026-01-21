using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CustomerFeature.Commands.DeleteCustomerCommand
{
    public class DeleteCustomerCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Customer> _repositoryAsync;

        public DeleteCustomerCommandHandler(IRepositoryAsync<Customer> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
            {
                return new Result<bool>("Cliente no encontrado.");
            }

            await _repositoryAsync.DeleteAsync(customer, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
