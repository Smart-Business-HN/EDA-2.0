using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.InvoiceSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Queries.GetInvoiceByIdQuery
{
    public class GetInvoiceByIdQuery : IRequest<Result<Invoice>>
    {
        public int Id { get; set; }
    }

    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<Invoice>>
    {
        private readonly IRepositoryAsync<Invoice> _repositoryAsync;

        public GetInvoiceByIdQueryHandler(IRepositoryAsync<Invoice> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Invoice>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _repositoryAsync.FirstOrDefaultAsync(
                new GetInvoiceByIdSpecification(request.Id),
                cancellationToken);

            if (invoice == null)
            {
                return new Result<Invoice>("Factura no encontrada");
            }

            return new Result<Invoice>(invoice);
        }
    }
}
