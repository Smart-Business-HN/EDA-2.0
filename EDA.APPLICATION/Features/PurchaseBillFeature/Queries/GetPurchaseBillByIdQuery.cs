using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PurchaseBillSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Queries
{
    public class GetPurchaseBillByIdQuery : IRequest<Result<PurchaseBill>>
    {
        public int Id { get; set; }
    }

    public class GetPurchaseBillByIdQueryHandler : IRequestHandler<GetPurchaseBillByIdQuery, Result<PurchaseBill>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _repositoryAsync;

        public GetPurchaseBillByIdQueryHandler(IRepositoryAsync<PurchaseBill> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PurchaseBill>> Handle(GetPurchaseBillByIdQuery request, CancellationToken cancellationToken)
        {
            var purchaseBill = await _repositoryAsync.FirstOrDefaultAsync(
                new GetPurchaseBillByIdSpecification(request.Id),
                cancellationToken);

            if (purchaseBill == null)
            {
                return new Result<PurchaseBill>("Factura de compra no encontrada.");
            }

            return new Result<PurchaseBill>(purchaseBill);
        }
    }
}
