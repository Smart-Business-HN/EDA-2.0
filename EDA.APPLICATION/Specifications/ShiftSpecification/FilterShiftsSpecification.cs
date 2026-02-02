using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ShiftSpecification
{
    public sealed class FilterShiftsSpecification : Specification<Shift>
    {
        public FilterShiftsSpecification(string? searchTerm, bool? isOpen = null, int? pageNumber = null, int? pageSize = null)
        {
            Query.Include(s => s.User);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(shift =>
                    (shift.User != null && shift.User.Name.Contains(searchTerm)) ||
                    (shift.User != null && shift.User.LastName.Contains(searchTerm)));
            }

            if (isOpen.HasValue)
            {
                Query.Where(shift => shift.IsOpen == isOpen.Value);
            }

            Query.OrderByDescending(shift => shift.StartTime);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                Query.Skip((pageNumber.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
        }
    }

    public sealed class CountShiftsSpecification : Specification<Shift>
    {
        public CountShiftsSpecification(string? searchTerm, bool? isOpen = null)
        {
            Query.Include(s => s.User);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(shift =>
                    (shift.User != null && shift.User.Name.Contains(searchTerm)) ||
                    (shift.User != null && shift.User.LastName.Contains(searchTerm)));
            }

            if (isOpen.HasValue)
            {
                Query.Where(shift => shift.IsOpen == isOpen.Value);
            }
        }
    }
}
