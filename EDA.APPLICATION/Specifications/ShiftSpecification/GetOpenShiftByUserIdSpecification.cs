using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.ShiftSpecification
{
    public sealed class GetOpenShiftByUserIdSpecification : Specification<Shift>
    {
        public GetOpenShiftByUserIdSpecification(int userId)
        {
            Query.Where(s => s.UserId == userId && s.IsOpen);
        }
    }
}
