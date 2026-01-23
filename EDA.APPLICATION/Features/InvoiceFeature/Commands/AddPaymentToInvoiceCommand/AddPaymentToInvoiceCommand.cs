using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.AddPaymentToInvoiceCommand
{
    public class AddPaymentToInvoiceCommand : IRequest<Result<InvoicePayment>>
    {
        public int InvoiceId { get; set; }
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class AddPaymentToInvoiceCommandHandler : IRequestHandler<AddPaymentToInvoiceCommand, Result<InvoicePayment>>
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

        public async Task<Result<InvoicePayment>> Handle(AddPaymentToInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener la factura
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

            if (invoice == null)
            {
                return new Result<InvoicePayment>("La factura no existe.");
            }

            // 2. Validar que la factura no esté anulada
            if (invoice.StatusId == (int)InvoiceStatusEnum.Anulada)
            {
                return new Result<InvoicePayment>("No se pueden agregar pagos a una factura anulada.");
            }

            // 3. Validar que la factura no esté ya pagada
            if (invoice.StatusId == (int)InvoiceStatusEnum.Pagada)
            {
                return new Result<InvoicePayment>("La factura ya está pagada.");
            }

            // 4. Validar monto positivo
            if (request.Amount <= 0)
            {
                return new Result<InvoicePayment>("El monto del pago debe ser mayor a cero.");
            }

            // 5. Crear el pago
            var payment = new InvoicePayment
            {
                InvoiceId = request.InvoiceId,
                PaymentTypeId = request.PaymentTypeId,
                Amount = request.Amount
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // 6. Obtener todos los pagos de la factura para calcular el total pagado
            var allPayments = await _paymentRepository.ListAsync(cancellationToken);
            var invoicePayments = allPayments.Where(p => p.InvoiceId == request.InvoiceId);
            var totalPaid = invoicePayments.Sum(p => p.Amount);

            // 7. Si el total pagado >= total de la factura, cambiar estado a Pagada
            if (totalPaid >= invoice.Total)
            {
                invoice.StatusId = (int)InvoiceStatusEnum.Pagada;
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
                await _invoiceRepository.SaveChangesAsync(cancellationToken);
            }

            return new Result<InvoicePayment>(payment);
        }
    }
}
