using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.VoidInvoiceCommand
{
    public class VoidInvoiceCommand : IRequest<Result<bool>>
    {
        public int InvoiceId { get; set; }
    }

    public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;

        public VoidInvoiceCommandHandler(IRepositoryAsync<Invoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Result<bool>> Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

            if (invoice == null)
            {
                return new Result<bool>("La factura no existe.");
            }

            if (invoice.StatusId == (int)InvoiceStatusEnum.Anulada)
            {
                return new Result<bool>("La factura ya se encuentra anulada.");
            }

            invoice.StatusId = (int)InvoiceStatusEnum.Anulada;

            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
