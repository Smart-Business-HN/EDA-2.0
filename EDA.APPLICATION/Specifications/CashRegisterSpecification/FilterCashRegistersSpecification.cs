using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.CashRegisterSpecification
{
    public sealed class FilterCashRegistersSpecification : Specification<CashRegister>
    {
        public FilterCashRegistersSpecification(string? searchTerm, bool? isActive = null, int? pageNumber = null, int? pageSize = null)
        {
            Query.Include(cr => cr.PrinterConfiguration);

            if (isActive.HasValue)
            {
                Query.Where(cr => cr.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(cr =>
                    cr.Name.Contains(searchTerm) ||
                    cr.Code.Contains(searchTerm));
            }

            Query.OrderBy(cr => cr.Name);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountCashRegistersSpecification : Specification<CashRegister>
    {
        public CountCashRegistersSpecification(string? searchTerm, bool? isActive = null)
        {
            if (isActive.HasValue)
            {
                Query.Where(cr => cr.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(cr =>
                    cr.Name.Contains(searchTerm) ||
                    cr.Code.Contains(searchTerm));
            }
        }
    }

    public sealed class GetCashRegisterByCodeSpecification : Specification<CashRegister>
    {
        public GetCashRegisterByCodeSpecification(string code)
        {
            Query.Where(cr => cr.Code == code);
        }
    }

    public sealed class GetCashRegisterByNameSpecification : Specification<CashRegister>
    {
        public GetCashRegisterByNameSpecification(string name)
        {
            Query.Where(cr => cr.Name == name);
        }
    }

    public sealed class GetActiveCashRegistersSpecification : Specification<CashRegister>
    {
        public GetActiveCashRegistersSpecification()
        {
            Query.Where(cr => cr.IsActive)
                 .Include(cr => cr.PrinterConfiguration)
                 .OrderBy(cr => cr.Name);
        }
    }

    public sealed class GetCashRegisterByIdWithPrinterSpecification : Specification<CashRegister>
    {
        public GetCashRegisterByIdWithPrinterSpecification(int id)
        {
            Query.Where(cr => cr.Id == id)
                 .Include(cr => cr.PrinterConfiguration);
        }
    }
}
