using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProductFeature.Commands.UpdateProductCommand
{
    public class UpdateProductCommand : IRequest<Result<Product>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int FamilyId { get; set; }
        public int TaxId { get; set; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Product>>
    {
        private readonly IRepositoryAsync<Product> _repositoryAsync;

        public UpdateProductCommandHandler(IRepositoryAsync<Product> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Product>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return new Result<Product>("Producto no encontrado.");
            }

            // Verificar que el nuevo nombre no exista (excepto si es el mismo producto)
            var existingProduct = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProductByNameSpecification(request.Name),
                cancellationToken);

            if (existingProduct != null && existingProduct.Id != request.Id)
            {
                return new Result<Product>("Ya existe un producto con este nombre.");
            }

            // Verificar que el código de barras no exista (si se proporciona)
            if (!string.IsNullOrWhiteSpace(request.Barcode))
            {
                var existingBarcode = await _repositoryAsync.FirstOrDefaultAsync(
                    new GetProductByBarcodeSpecification(request.Barcode),
                    cancellationToken);

                if (existingBarcode != null && existingBarcode.Id != request.Id)
                {
                    return new Result<Product>("Ya existe un producto con este código de barras.");
                }
            }

            product.Name = request.Name;
            product.Barcode = request.Barcode;
            product.Date = request.Date;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.FamilyId = request.FamilyId;
            product.TaxId = request.TaxId;

            await _repositoryAsync.UpdateAsync(product, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Product>(product);
        }
    }
}
