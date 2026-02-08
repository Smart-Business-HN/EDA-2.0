using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProviderSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProviderFeature.Commands.CreateProviderCommand
{
    public class CreateProviderCommand : IRequest<Result<Provider>>
    {
        public string Name { get; set; } = null!;
        public string RTN { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
        public string? Address { get; set; }
        public string? WebsiteUrl { get; set; }
        public int TypeProviderId { get; set; } = 1;
        public string CreatedBy { get; set; } = null!;
    }

    public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<Provider>>
    {
        private readonly IRepositoryAsync<Provider> _repositoryAsync;

        public CreateProviderCommandHandler(IRepositoryAsync<Provider> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Provider>> Handle(CreateProviderCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingByName = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProviderByNameSpecification(request.Name),
                cancellationToken);

            if (existingByName != null)
            {
                return new Result<Provider>("Ya existe un proveedor con este nombre.");
            }

            // Verificar que el RTN no exista
            var existingByRtn = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProviderByRtnSpecification(request.RTN),
                cancellationToken);

            if (existingByRtn != null)
            {
                return new Result<Provider>("Ya existe un proveedor con este RTN.");
            }

            var newProvider = new Provider
            {
                Name = request.Name,
                RTN = request.RTN,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                Email = request.Email ?? string.Empty,
                ContactPerson = request.ContactPerson,
                ContactPhoneNumber = request.ContactPhoneNumber,
                ContactEmail = request.ContactEmail,
                Address = request.Address,
                WebsiteUrl = request.WebsiteUrl,
                TypeProviderId = request.TypeProviderId,
                CreationDate = DateTime.Now,
                CreatedBy = request.CreatedBy,
                IsActive = true
            };

            await _repositoryAsync.AddAsync(newProvider, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Provider>(newProvider);
        }
    }
}
