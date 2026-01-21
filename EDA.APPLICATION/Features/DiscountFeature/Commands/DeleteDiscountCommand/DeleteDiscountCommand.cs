using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.DiscountFeature.Commands.DeleteDiscountCommand
{
    public class DeleteDiscountCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Discount> _repositoryAsync;

        public DeleteDiscountCommandHandler(IRepositoryAsync<Discount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
        {
            var discount = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (discount == null)
            {
                return new Result<bool>("Descuento no encontrado.");
            }

            await _repositoryAsync.DeleteAsync(discount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
