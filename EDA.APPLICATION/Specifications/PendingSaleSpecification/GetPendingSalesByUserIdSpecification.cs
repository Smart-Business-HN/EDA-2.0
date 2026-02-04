using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.PendingSaleSpecification
{
    public sealed class GetPendingSalesByUserIdSpecification : Specification<PendingSale>
    {
        public GetPendingSalesByUserIdSpecification(int userId)
        {
            Query.Where(p => p.UserId == userId)
                 .OrderBy(p => p.CreatedAt);
        }
    }
}
