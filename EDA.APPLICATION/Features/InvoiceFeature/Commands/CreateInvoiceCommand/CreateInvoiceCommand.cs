using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.CreateInvoiceCommand
{
    // DTO para los items del carrito
    public class CreateInvoiceItemDto
    {
        public int ProductId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public int TaxId { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // DTO para los pagos
    public class CreateInvoicePaymentDto
    {
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreateInvoiceCommand : IRequest<Result<Invoice>>
    {
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public int CaiId { get; set; }
        public int UserId { get; set; }
        public int? DiscountId { get; set; }
        public decimal DiscountPercentage { get; set; }

        // Totales calculados desde UI
        public decimal Subtotal { get; set; }
        public double TotalDiscounts { get; set; }
        public double TotalTaxes { get; set; }
        public decimal Total { get; set; }

        // Manejo de efectivo
        public double? CashReceived { get; set; }
        public double? ChangeGiven { get; set; }

        // Credit
        public bool IsCredit { get; set; }
        public int? CreditDays { get; set; }

        // Cash Register
        public int? CashRegisterId { get; set; }

        // Items y Pagos
        public List<CreateInvoiceItemDto> Items { get; set; } = new();
        public List<CreateInvoicePaymentDto> Payments { get; set; } = new();
    }

    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<Invoice>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<Cai> _caiRepository;
        private readonly IRepositoryAsync<SoldProduct> _soldProductRepository;
        private readonly IRepositoryAsync<InvoicePayment> _paymentRepository;
        private readonly IRepositoryAsync<Product> _productRepository;

        public CreateInvoiceCommandHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<Cai> caiRepository,
            IRepositoryAsync<SoldProduct> soldProductRepository,
            IRepositoryAsync<InvoicePayment> paymentRepository,
            IRepositoryAsync<Product> productRepository)
        {
            _invoiceRepository = invoiceRepository;
            _caiRepository = caiRepository;
            _soldProductRepository = soldProductRepository;
            _paymentRepository = paymentRepository;
            _productRepository = productRepository;
        }

        public async Task<Result<Invoice>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener y validar CAI
            var cai = await _caiRepository.GetByIdAsync(request.CaiId, cancellationToken);
            if (cai == null)
            {
                return new Result<Invoice>("El CAI especificado no existe.");
            }

            if (!cai.IsActive)
            {
                return new Result<Invoice>("El CAI no está activo.");
            }

            if (cai.PendingInvoices <= 0)
            {
                return new Result<Invoice>("El CAI no tiene facturas pendientes disponibles.");
            }

            if (cai.CurrentCorrelative > cai.FinalCorrelative)
            {
                return new Result<Invoice>("El CAI ha alcanzado su correlativo máximo.");
            }

            if (DateTime.Now > cai.ToDate)
            {
                return new Result<Invoice>("El CAI ha expirado.");
            }

            // 2. Generar número de factura
            // Formato: PREFIX + 8-digit zero-padded correlative
            string invoiceNumber = $"{cai.Prefix}{cai.CurrentCorrelative:D8}";

            // 3. Calcular desglose de impuestos por tipo
            decimal taxedAt15 = 0, taxesAt15 = 0;
            decimal taxedAt18 = 0, taxesAt18 = 0;
            decimal exempt = 0;

            foreach (var item in request.Items)
            {
                decimal itemSubtotal = item.Quantity * item.UnitPrice;
                decimal itemAfterDiscount = itemSubtotal - (itemSubtotal * request.DiscountPercentage / 100m);

                // Tax IDs: 1 = Exento (0%), 2 = ISV 15%, 3 = ISV 18%
                switch (item.TaxId)
                {
                    case 1: // Exento
                        exempt += itemAfterDiscount;
                        break;
                    case 2: // 15%
                        taxedAt15 += itemAfterDiscount;
                        taxesAt15 += itemAfterDiscount * 0.15m;
                        break;
                    case 3: // 18%
                        taxedAt18 += itemAfterDiscount;
                        taxesAt18 += itemAfterDiscount * 0.18m;
                        break;
                    default:
                        // Usar porcentaje proporcionado para flexibilidad
                        if (item.TaxPercentage == 0)
                            exempt += itemAfterDiscount;
                        else if (item.TaxPercentage == 15)
                        {
                            taxedAt15 += itemAfterDiscount;
                            taxesAt15 += itemAfterDiscount * 0.15m;
                        }
                        else if (item.TaxPercentage == 18)
                        {
                            taxedAt18 += itemAfterDiscount;
                            taxesAt18 += itemAfterDiscount * 0.18m;
                        }
                        break;
                }
            }

            // 4. Calcular campos de credito
            decimal totalPayments = request.Payments.Sum(p => p.Amount);
            int status;
            decimal outstandingAmount;
            int? creditDays = null;
            DateTime? dueDate = null;

            if (request.IsCredit)
            {
                status = (int)InvoiceStatusEnum.Created;
                outstandingAmount = request.Total - totalPayments;
                creditDays = request.CreditDays;
                dueDate = request.Date.AddDays(request.CreditDays ?? 30);
            }
            else
            {
                status = (int)InvoiceStatusEnum.Paid;
                outstandingAmount = 0;
            }

            // 5. Crear entidad Invoice
            var invoice = new Invoice
            {
                Date = request.Date,
                CustomerId = request.CustomerId,
                CaiId = request.CaiId,
                InvoiceNumber = invoiceNumber,
                UserId = request.UserId,
                DiscountId = request.DiscountId,
                Subtotal = request.Subtotal,
                TotalDiscounts = request.TotalDiscounts,
                TotalTaxes = request.TotalTaxes,
                Total = request.Total,
                CashReceived = request.CashReceived,
                ChangeGiven = request.ChangeGiven,
                TaxedAt15Percent = taxedAt15,
                TaxesAt15Percent = taxesAt15,
                TaxedAt18Percent = taxedAt18,
                TaxesAt18Percent = taxesAt18,
                Exempt = exempt,
                Status = status,
                OutstandingAmount = outstandingAmount,
                CreditDays = creditDays,
                DueDate = dueDate,
                CashRegisterId = request.CashRegisterId,
                IsPrinted = false,
                PrintCount = 0
            };

            // 5. Agregar factura a la base de datos
            await _invoiceRepository.AddAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            // 6. Crear registros SoldProduct
            foreach (var item in request.Items)
            {
                decimal itemSubtotal = item.Quantity * item.UnitPrice;
                decimal discountAmount = itemSubtotal * request.DiscountPercentage / 100m;
                decimal itemAfterDiscount = itemSubtotal - discountAmount;
                decimal taxAmount = itemAfterDiscount * item.TaxPercentage / 100m;
                decimal totalLine = itemAfterDiscount + taxAmount;

                var soldProduct = new SoldProduct
                {
                    InvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    TaxId = item.TaxId,
                    TaxAmount = (double)taxAmount,
                    DiscountId = request.DiscountId,
                    DiscountAmount = (double)discountAmount,
                    UnitPrice = item.UnitPrice,
                    TotalLine = totalLine
                };

                await _soldProductRepository.AddAsync(soldProduct, cancellationToken);
            }
            await _soldProductRepository.SaveChangesAsync(cancellationToken);

            // 6.5. Reducir stock de productos vendidos
            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    if (product.Stock < 0) product.Stock = 0;
                    await _productRepository.UpdateAsync(product, cancellationToken);
                }
            }
            await _productRepository.SaveChangesAsync(cancellationToken);

            // 7. Crear registros InvoicePayment
            foreach (var payment in request.Payments)
            {
                var invoicePayment = new InvoicePayment
                {
                    InvoiceId = invoice.Id,
                    PaymentTypeId = payment.PaymentTypeId,
                    Amount = payment.Amount
                };

                await _paymentRepository.AddAsync(invoicePayment, cancellationToken);
            }
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // 8. Actualizar CAI: CurrentCorrelative y PendingInvoices
            cai.CurrentCorrelative++;
            cai.PendingInvoices--;

            await _caiRepository.UpdateAsync(cai, cancellationToken);
            await _caiRepository.SaveChangesAsync(cancellationToken);

            return new Result<Invoice>(invoice);
        }
    }
}
