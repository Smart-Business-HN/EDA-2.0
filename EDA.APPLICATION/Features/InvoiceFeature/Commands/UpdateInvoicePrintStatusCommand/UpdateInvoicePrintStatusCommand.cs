using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.UpdateInvoicePrintStatusCommand
{
    public class UpdateInvoicePrintStatusCommand : IRequest<Result<bool>>
    {
        public int InvoiceId { get; set; }
        public bool IsPrinted { get; set; }
        public DateTime? PrintedAt { get; set; }
    }

    public class UpdateInvoicePrintStatusCommandHandler : IRequestHandler<UpdateInvoicePrintStatusCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Invoice> _repository;

        public UpdateInvoicePrintStatusCommandHandler(IRepositoryAsync<Invoice> repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(UpdateInvoicePrintStatusCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _repository.GetByIdAsync(request.InvoiceId, cancellationToken);

            if (invoice == null)
            {
                return new Result<bool>("Factura no encontrada.");
            }

            invoice.IsPrinted = request.IsPrinted;
            invoice.PrintedAt = request.PrintedAt;
            invoice.PrintCount++;

            await _repository.UpdateAsync(invoice, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
