using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.DeletePurchaseBillCommand
{
    public class DeletePurchaseBillCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeletePurchaseBillCommandHandler : IRequestHandler<DeletePurchaseBillCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _repositoryAsync;

        public DeletePurchaseBillCommandHandler(IRepositoryAsync<PurchaseBill> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeletePurchaseBillCommand request, CancellationToken cancellationToken)
        {
            var purchaseBill = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (purchaseBill == null)
            {
                return new Result<bool>("Factura de compra no encontrada.");
            }

            // Ya esta anulada
            if (purchaseBill.StatusId == (int)PurchaseBillStatusEnum.Cancelled)
            {
                return new Result<bool>("La factura ya esta anulada.");
            }

            // Soft delete - cambiar estado a Cancelled
            purchaseBill.StatusId = (int)PurchaseBillStatusEnum.Cancelled;

            await _repositoryAsync.UpdateAsync(purchaseBill, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
