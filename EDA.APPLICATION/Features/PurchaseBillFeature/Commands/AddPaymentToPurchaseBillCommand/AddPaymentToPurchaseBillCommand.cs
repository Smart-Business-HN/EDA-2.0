using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.AddPaymentToPurchaseBillCommand
{
    public class AddPaymentToPurchaseBillCommand : IRequest<Result<PurchaseBill>>
    {
        public int PurchaseBillId { get; set; }
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = null!;
    }

    public class AddPaymentToPurchaseBillCommandHandler : IRequestHandler<AddPaymentToPurchaseBillCommand, Result<PurchaseBill>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _purchaseBillRepository;
        private readonly IRepositoryAsync<PurchaseBillPayment> _paymentRepository;

        public AddPaymentToPurchaseBillCommandHandler(
            IRepositoryAsync<PurchaseBill> purchaseBillRepository,
            IRepositoryAsync<PurchaseBillPayment> paymentRepository)
        {
            _purchaseBillRepository = purchaseBillRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result<PurchaseBill>> Handle(AddPaymentToPurchaseBillCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener factura de compra
            var purchaseBill = await _purchaseBillRepository.GetByIdAsync(request.PurchaseBillId, cancellationToken);
            if (purchaseBill == null)
            {
                return new Result<PurchaseBill>("La factura de compra no existe.");
            }

            // 2. Validar que no este anulada
            if (purchaseBill.StatusId == (int)PurchaseBillStatusEnum.Cancelled)
            {
                return new Result<PurchaseBill>("No se puede agregar pagos a una factura anulada.");
            }

            // 3. Validar que no este pagada
            if (purchaseBill.StatusId == (int)PurchaseBillStatusEnum.Paid)
            {
                return new Result<PurchaseBill>("La factura ya esta completamente pagada.");
            }

            // 4. Validar monto
            if (request.Amount <= 0)
            {
                return new Result<PurchaseBill>("El monto debe ser mayor a cero.");
            }

            if (request.Amount > purchaseBill.OutstandingAmount)
            {
                return new Result<PurchaseBill>($"El monto excede el saldo pendiente de L {purchaseBill.OutstandingAmount:N2}.");
            }

            // 5. Crear registro de pago
            var payment = new PurchaseBillPayment
            {
                PurchaseBillId = request.PurchaseBillId,
                PaymentTypeId = request.PaymentTypeId,
                Date = request.PaymentDate,
                Amount = request.Amount,
                CreationDate = DateTime.Now,
                CreatedBy = request.CreatedBy
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // 6. Actualizar saldo pendiente
            purchaseBill.OutstandingAmount -= request.Amount;

            // 7. Si el saldo pendiente es 0, marcar como pagada
            if (purchaseBill.OutstandingAmount <= 0)
            {
                purchaseBill.OutstandingAmount = 0;
                purchaseBill.StatusId = (int)PurchaseBillStatusEnum.Paid;
            }

            await _purchaseBillRepository.UpdateAsync(purchaseBill, cancellationToken);
            await _purchaseBillRepository.SaveChangesAsync(cancellationToken);

            return new Result<PurchaseBill>(purchaseBill);
        }
    }
}
