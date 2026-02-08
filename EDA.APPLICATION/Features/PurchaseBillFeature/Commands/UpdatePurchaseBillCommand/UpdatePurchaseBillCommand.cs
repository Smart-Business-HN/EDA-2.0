using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.UpdatePurchaseBillCommand
{
    public class UpdatePurchaseBillCommand : IRequest<Result<PurchaseBill>>
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
        public string Cai { get; set; } = null!;
        public decimal Exempt { get; set; }
        public decimal Exonerated { get; set; }
        public decimal TaxedAt15Percent { get; set; }
        public decimal TaxedAt18Percent { get; set; }
        public decimal Taxes15Percent { get; set; }
        public decimal Taxes18Percent { get; set; }
        public decimal Total { get; set; }
        public int? CreditDays { get; set; }
        public DateTime? DueDate { get; set; }
        public int ExpenseAccountId { get; set; }
    }

    public class UpdatePurchaseBillCommandHandler : IRequestHandler<UpdatePurchaseBillCommand, Result<PurchaseBill>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _repositoryAsync;

        public UpdatePurchaseBillCommandHandler(IRepositoryAsync<PurchaseBill> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PurchaseBill>> Handle(UpdatePurchaseBillCommand request, CancellationToken cancellationToken)
        {
            var purchaseBill = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (purchaseBill == null)
            {
                return new Result<PurchaseBill>("Factura de compra no encontrada.");
            }

            // No permitir editar si esta anulada
            if (purchaseBill.StatusId == (int)PurchaseBillStatusEnum.Cancelled)
            {
                return new Result<PurchaseBill>("No se puede modificar una factura anulada.");
            }

            // Calcular diferencia en total para actualizar OutstandingAmount
            var totalDifference = request.Total - purchaseBill.Total;
            var newOutstanding = purchaseBill.OutstandingAmount + totalDifference;

            purchaseBill.ProviderId = request.ProviderId;
            purchaseBill.InvoiceNumber = request.InvoiceNumber;
            purchaseBill.InvoiceDate = request.InvoiceDate;
            purchaseBill.Cai = request.Cai;
            purchaseBill.Exempt = request.Exempt;
            purchaseBill.Exonerated = request.Exonerated;
            purchaseBill.TaxedAt15Percent = request.TaxedAt15Percent;
            purchaseBill.TaxedAt18Percent = request.TaxedAt18Percent;
            purchaseBill.Taxes15Percent = request.Taxes15Percent;
            purchaseBill.Taxes18Percent = request.Taxes18Percent;
            purchaseBill.Total = request.Total;
            purchaseBill.OutstandingAmount = newOutstanding < 0 ? 0 : newOutstanding;
            purchaseBill.CreditDays = request.CreditDays;
            purchaseBill.DueDate = request.DueDate;
            purchaseBill.ExpenseAccountId = request.ExpenseAccountId;

            // Si el outstanding es 0, marcar como pagada
            if (purchaseBill.OutstandingAmount <= 0)
            {
                purchaseBill.OutstandingAmount = 0;
                purchaseBill.StatusId = (int)PurchaseBillStatusEnum.Paid;
            }

            await _repositoryAsync.UpdateAsync(purchaseBill, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PurchaseBill>(purchaseBill);
        }
    }
}
