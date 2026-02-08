using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PurchaseBillSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.PurchaseBillFeature.Commands.CreatePurchaseBillCommand
{
    public class CreatePurchaseBillCommand : IRequest<Result<PurchaseBill>>
    {
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

    public class CreatePurchaseBillCommandHandler : IRequestHandler<CreatePurchaseBillCommand, Result<PurchaseBill>>
    {
        private readonly IRepositoryAsync<PurchaseBill> _repositoryAsync;

        public CreatePurchaseBillCommandHandler(IRepositoryAsync<PurchaseBill> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PurchaseBill>> Handle(CreatePurchaseBillCommand request, CancellationToken cancellationToken)
        {
            // Generar PurchaseBillCode (auto-increment, 8 digitos)
            var lastBill = await _repositoryAsync.FirstOrDefaultAsync(
                new GetLastPurchaseBillCodeSpecification(),
                cancellationToken);

            int nextNumber = 1;
            if (lastBill != null && int.TryParse(lastBill.PurchaseBillCode, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
            string purchaseBillCode = nextNumber.ToString("D8");

            var newPurchaseBill = new PurchaseBill
            {
                PurchaseBillCode = purchaseBillCode,
                ProviderId = request.ProviderId,
                InvoiceNumber = request.InvoiceNumber,
                InvoiceDate = request.InvoiceDate,
                CreationDate = DateTime.Now,
                Cai = request.Cai,
                StatusId = (int)PurchaseBillStatusEnum.Created,
                Exempt = request.Exempt,
                Exonerated = request.Exonerated,
                TaxedAt15Percent = request.TaxedAt15Percent,
                TaxedAt18Percent = request.TaxedAt18Percent,
                Taxes15Percent = request.Taxes15Percent,
                Taxes18Percent = request.Taxes18Percent,
                Total = request.Total,
                OutstandingAmount = request.Total,
                CreditDays = request.CreditDays,
                DueDate = request.DueDate,
                ExpenseAccountId = request.ExpenseAccountId
            };

            await _repositoryAsync.AddAsync(newPurchaseBill, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PurchaseBill>(newPurchaseBill);
        }
    }
}
