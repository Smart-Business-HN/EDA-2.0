using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.TaxFeature.Commands.DeleteTaxCommand
{
    public class DeleteTaxCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteTaxCommandHandler : IRequestHandler<DeleteTaxCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Tax> _repositoryAsync;

        public DeleteTaxCommandHandler(IRepositoryAsync<Tax> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteTaxCommand request, CancellationToken cancellationToken)
        {
            var tax = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (tax == null)
            {
                return new Result<bool>("Impuesto no encontrado.");
            }

            await _repositoryAsync.DeleteAsync(tax, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
