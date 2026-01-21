using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CustomerSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CustomerFeature.Commands.UpdateCustomerCommand
{
    public class UpdateCustomerCommand : IRequest<Result<Customer>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<Customer>>
    {
        private readonly IRepositoryAsync<Customer> _repositoryAsync;

        public UpdateCustomerCommandHandler(IRepositoryAsync<Customer> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Customer>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
            {
                return new Result<Customer>("Cliente no encontrado.");
            }

            // Verificar que el nuevo nombre no exista (excepto si es el mismo cliente)
            var existingCustomer = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCustomerByNameSpecification(request.Name),
                cancellationToken);

            if (existingCustomer != null && existingCustomer.Id != request.Id)
            {
                return new Result<Customer>("Ya existe un cliente con este nombre.");
            }

            customer.Name = request.Name;
            customer.Company = request.Company;
            customer.Email = request.Email;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Description = request.Description;

            await _repositoryAsync.UpdateAsync(customer, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Customer>(customer);
        }
    }
}
