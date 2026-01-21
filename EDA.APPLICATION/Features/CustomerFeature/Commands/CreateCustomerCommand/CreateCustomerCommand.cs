using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CustomerSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CustomerFeature.Commands.CreateCustomerCommand
{
    public class CreateCustomerCommand : IRequest<Result<Customer>>
    {
        public string Name { get; set; } = null!;
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Customer>>
    {
        private readonly IRepositoryAsync<Customer> _repositoryAsync;

        public CreateCustomerCommandHandler(IRepositoryAsync<Customer> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Customer>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingCustomer = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCustomerByNameSpecification(request.Name),
                cancellationToken);

            if (existingCustomer != null)
            {
                return new Result<Customer>("Ya existe un cliente con este nombre.");
            }

            var newCustomer = new Customer
            {
                Name = request.Name,
                Company = request.Company,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Description = request.Description
            };

            await _repositoryAsync.AddAsync(newCustomer, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Customer>(newCustomer);
        }
    }
}
