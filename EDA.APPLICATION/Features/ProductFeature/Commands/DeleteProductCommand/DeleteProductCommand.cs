using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProductFeature.Commands.DeleteProductCommand
{
    public class DeleteProductCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Product> _repositoryAsync;

        public DeleteProductCommandHandler(IRepositoryAsync<Product> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return new Result<bool>("Producto no encontrado.");
            }

            await _repositoryAsync.DeleteAsync(product, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
