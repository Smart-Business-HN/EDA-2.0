using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ShiftSpecification
{
    public sealed class FilterShiftsForSummarySpecification : Specification<Shift>
    {
        public FilterShiftsForSummarySpecification(DateTime fromDate, DateTime toDate, int? userId = null)
        {
            Query.Include(s => s.User)
                 .Where(s => s.StartTime >= fromDate && s.StartTime < toDate);

            if (userId.HasValue)
            {
                Query.Where(s => s.UserId == userId.Value);
            }

            Query.OrderByDescending(s => s.StartTime);
        }
    }
}
