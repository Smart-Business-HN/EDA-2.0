using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.PrinterConfigurationSpecification
{
    public sealed class FilterPrinterConfigurationsSpecification : Specification<PrinterConfiguration>
    {
        public FilterPrinterConfigurationsSpecification(string? searchTerm, bool? isActive = null, int? pageNumber = null, int? pageSize = null)
        {
            if (isActive.HasValue)
            {
                Query.Where(p => p.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.PrinterName != null && p.PrinterName.Contains(searchTerm)));
            }

            Query.OrderBy(p => p.Name);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountPrinterConfigurationsSpecification : Specification<PrinterConfiguration>
    {
        public CountPrinterConfigurationsSpecification(string? searchTerm, bool? isActive = null)
        {
            if (isActive.HasValue)
            {
                Query.Where(p => p.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.PrinterName != null && p.PrinterName.Contains(searchTerm)));
            }
        }
    }

    public sealed class GetPrinterConfigurationByNameSpecification : Specification<PrinterConfiguration>
    {
        public GetPrinterConfigurationByNameSpecification(string name)
        {
            Query.Where(p => p.Name == name);
        }
    }

    public sealed class GetActivePrinterConfigurationsSpecification : Specification<PrinterConfiguration>
    {
        public GetActivePrinterConfigurationsSpecification()
        {
            Query.Where(p => p.IsActive)
                 .OrderBy(p => p.Name);
        }
    }
}
