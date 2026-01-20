using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.UserSpecifications
{
    public sealed class GetUserUserNameSpecification : Specification<User>
    {
        public GetUserUserNameSpecification(string userName)
        {
            Query
                .Where(user => user.Name == userName)
                .Include(user => user.Role);
        }
    }
}
