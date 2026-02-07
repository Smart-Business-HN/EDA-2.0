using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.AddPaymentToInvoiceCommand
{
    public class AddPaymentToInvoiceCommand : IRequest<Result<Invoice>>
    {
        public int InvoiceId { get; set; }
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class AddPaymentToInvoiceCommandHandler : IRequestHandler<AddPaymentToInvoiceCommand, Result<Invoice>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<InvoicePayment> _paymentRepository;

        public AddPaymentToInvoiceCommandHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<InvoicePayment> paymentRepository)
        {
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<Invoice>> Handle(AddPaymentToInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener factura
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null)
            {
                return new Result<Invoice>("La factura no existe.");
            }

            // 2. Validar que la factura no esté anulada
            if (invoice.Status == (int)InvoiceStatusEnum.Cancelled)
            {
                return new Result<Invoice>("No se puede agregar pagos a una factura anulada.");
            }

            // 3. Validar que la factura no esté completamente pagada
            if (invoice.Status == (int)InvoiceStatusEnum.Paid)
            {
                return new Result<Invoice>("La factura ya está completamente pagada.");
            }

            // 4. Validar monto
            if (request.Amount <= 0)
            {
                return new Result<Invoice>("El monto debe ser mayor a cero.");
            }

            if (request.Amount > invoice.OutstandingAmount)
            {
                return new Result<Invoice>($"El monto excede el saldo pendiente de L {invoice.OutstandingAmount:N2}.");
            }

            // 5. Crear registro de pago
            var payment = new InvoicePayment
            {
                InvoiceId = request.InvoiceId,
                PaymentTypeId = request.PaymentTypeId,
                Amount = request.Amount
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // 6. Actualizar saldo pendiente
            invoice.OutstandingAmount -= request.Amount;

            // 7. Si el saldo pendiente es 0, marcar como pagada
            if (invoice.OutstandingAmount <= 0)
            {
                invoice.OutstandingAmount = 0;
                invoice.Status = (int)InvoiceStatusEnum.Paid;
            }

            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            return new Result<Invoice>(invoice);
        }
    }
}
