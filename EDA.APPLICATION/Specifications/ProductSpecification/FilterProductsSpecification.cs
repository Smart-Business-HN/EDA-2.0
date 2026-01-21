using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ProductSpecification
{
    public sealed class FilterProductsSpecification : Specification<Product>
    {
        public FilterProductsSpecification(string? searchTerm, int? pageNumber = null, int? pageSize = null)
        {
            Query.Include(p => p.Family)
                 .Include(p => p.Tax);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(product =>
                    product.Name.Contains(searchTerm) ||
                    (product.Barcode != null && product.Barcode.Contains(searchTerm)));
            }

            Query.OrderBy(product => product.Name);

            // Solo aplicar paginación si se proporcionan los parámetros
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountProductsSpecification : Specification<Product>
    {
        public CountProductsSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(product =>
                    product.Name.Contains(searchTerm) ||
                    (product.Barcode != null && product.Barcode.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetProductByNameSpecification : Specification<Product>
    {
        public GetProductByNameSpecification(string name)
        {
            Query.Where(product => product.Name == name);
        }
    }

    public sealed class GetProductByBarcodeSpecification : Specification<Product>
    {
        public GetProductByBarcodeSpecification(string barcode)
        {
            Query.Where(product => product.Barcode == barcode);
        }
    }
}
