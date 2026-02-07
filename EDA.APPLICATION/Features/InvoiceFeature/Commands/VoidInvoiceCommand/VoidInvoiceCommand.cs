using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.SoldProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;

namespace EDA.APPLICATION.Features.InvoiceFeature.Commands.VoidInvoiceCommand
{
    public class VoidInvoiceCommand : IRequest<Result<Invoice>>
    {
        public int InvoiceId { get; set; }
    }

    public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, Result<Invoice>>
    {
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;
        private readonly IRepositoryAsync<SoldProduct> _soldProductRepository;
        private readonly IRepositoryAsync<Product> _productRepository;

        public VoidInvoiceCommandHandler(
            IRepositoryAsync<Invoice> invoiceRepository,
            IRepositoryAsync<SoldProduct> soldProductRepository,
            IRepositoryAsync<Product> productRepository)
        {
            _invoiceRepository = invoiceRepository;
            _soldProductRepository = soldProductRepository;
            _productRepository = productRepository;
        }

        public async Task<Result<Invoice>> Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Obtener factura
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice == null)
            {
                return new Result<Invoice>("La factura no existe.");
            }

            // 2. Validar que la factura no esté ya anulada
            if (invoice.Status == (int)InvoiceStatusEnum.Cancelled)
            {
                return new Result<Invoice>("La factura ya está anulada.");
            }

            // 3. Obtener productos vendidos para restaurar stock
            var soldProducts = await _soldProductRepository.ListAsync(
                new GetSoldProductsByInvoiceIdSpecification(request.InvoiceId),
                cancellationToken);

            // 4. Restaurar stock de productos
            foreach (var soldProduct in soldProducts)
            {
                var product = await _productRepository.GetByIdAsync(soldProduct.ProductId, cancellationToken);
                if (product != null)
                {
                    product.Stock += soldProduct.Quantity;
                    await _productRepository.UpdateAsync(product, cancellationToken);
                }
            }
            await _productRepository.SaveChangesAsync(cancellationToken);

            // 5. Marcar factura como anulada
            invoice.Status = (int)InvoiceStatusEnum.Cancelled;
            invoice.OutstandingAmount = 0;

            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            return new Result<Invoice>(invoice);
        }
    }
}
