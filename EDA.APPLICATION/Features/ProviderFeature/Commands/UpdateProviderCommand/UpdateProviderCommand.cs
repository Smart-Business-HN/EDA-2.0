using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProviderSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProviderFeature.Commands.UpdateProviderCommand
{
    public class UpdateProviderCommand : IRequest<Result<Provider>>
    {
        public int Id { get; set; }
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
        public string ModificatedBy { get; set; } = null!;
    }

    public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<Provider>>
    {
        private readonly IRepositoryAsync<Provider> _repositoryAsync;

        public UpdateProviderCommandHandler(IRepositoryAsync<Provider> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Provider>> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (provider == null)
            {
                return new Result<Provider>("Proveedor no encontrado.");
            }

            // Verificar que el nuevo nombre no exista (excepto si es el mismo proveedor)
            var existingByName = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProviderByNameSpecification(request.Name),
                cancellationToken);

            if (existingByName != null && existingByName.Id != request.Id)
            {
                return new Result<Provider>("Ya existe un proveedor con este nombre.");
            }

            // Verificar que el nuevo RTN no exista (excepto si es el mismo proveedor)
            var existingByRtn = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProviderByRtnSpecification(request.RTN),
                cancellationToken);

            if (existingByRtn != null && existingByRtn.Id != request.Id)
            {
                return new Result<Provider>("Ya existe un proveedor con este RTN.");
            }

            provider.Name = request.Name;
            provider.RTN = request.RTN;
            provider.PhoneNumber = request.PhoneNumber ?? string.Empty;
            provider.Email = request.Email ?? string.Empty;
            provider.ContactPerson = request.ContactPerson;
            provider.ContactPhoneNumber = request.ContactPhoneNumber;
            provider.ContactEmail = request.ContactEmail;
            provider.Address = request.Address;
            provider.WebsiteUrl = request.WebsiteUrl;
            provider.TypeProviderId = request.TypeProviderId;
            provider.ModificationDate = DateTime.Now;
            provider.ModificatedBy = request.ModificatedBy;

            await _repositoryAsync.UpdateAsync(provider, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Provider>(provider);
        }
    }
}
