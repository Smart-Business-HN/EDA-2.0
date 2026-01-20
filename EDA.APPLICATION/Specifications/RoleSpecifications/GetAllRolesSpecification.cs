using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.RoleSpecifications
{
    public sealed class GetAllRolesSpecification : Specification<Role>
    {
        public GetAllRolesSpecification()
        {
            Query.OrderBy(role => role.Name);
        }
    }
}
