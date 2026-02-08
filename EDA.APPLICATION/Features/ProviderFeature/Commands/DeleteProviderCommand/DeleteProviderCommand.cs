using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProviderFeature.Commands.DeleteProviderCommand
{
    public class DeleteProviderCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string ModificatedBy { get; set; } = null!;
    }

    public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Provider> _repositoryAsync;

        public DeleteProviderCommandHandler(IRepositoryAsync<Provider> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
        {
            var provider = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (provider == null)
            {
                return new Result<bool>("Proveedor no encontrado.");
            }

            // Soft delete
            provider.IsActive = false;
            provider.ModificationDate = DateTime.Now;
            provider.ModificatedBy = request.ModificatedBy;

            await _repositoryAsync.UpdateAsync(provider, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
