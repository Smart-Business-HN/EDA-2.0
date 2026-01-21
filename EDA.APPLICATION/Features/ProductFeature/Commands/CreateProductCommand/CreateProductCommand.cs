using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProductFeature.Commands.CreateProductCommand
{
    public class CreateProductCommand : IRequest<Result<Product>>
    {
        public string Name { get; set; } = null!;
        public string? Barcode { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int FamilyId { get; set; }
        public int TaxId { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Product>>
    {
        private readonly IRepositoryAsync<Product> _repositoryAsync;

        public CreateProductCommandHandler(IRepositoryAsync<Product> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Product>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingProduct = await _repositoryAsync.FirstOrDefaultAsync(
                new GetProductByNameSpecification(request.Name),
                cancellationToken);

            if (existingProduct != null)
            {
                return new Result<Product>("Ya existe un producto con este nombre.");
            }

            // Verificar que el código de barras no exista (si se proporciona)
            if (!string.IsNullOrWhiteSpace(request.Barcode))
            {
                var existingBarcode = await _repositoryAsync.FirstOrDefaultAsync(
                    new GetProductByBarcodeSpecification(request.Barcode),
                    cancellationToken);

                if (existingBarcode != null)
                {
                    return new Result<Product>("Ya existe un producto con este código de barras.");
                }
            }

            var newProduct = new Product
            {
                Name = request.Name,
                Barcode = request.Barcode,
                Date = request.Date,
                Price = request.Price,
                Stock = request.Stock,
                FamilyId = request.FamilyId,
                TaxId = request.TaxId
            };

            await _repositoryAsync.AddAsync(newProduct, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Product>(newProduct);
        }
    }
}
