using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.UserSpecifications
{
    public sealed class FilterUsersSpecification : Specification<User>
    {
        public FilterUsersSpecification(string? searchTerm, int pageNumber, int pageSize)
        {
            Query.Include(user => user.Role);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(user =>
                    user.Name.Contains(searchTerm) ||
                    user.LastName.Contains(searchTerm));
            }

            Query.OrderBy(user => user.Name)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);
        }
    }

    public sealed class CountUsersSpecification : Specification<User>
    {
        public CountUsersSpecification(string? searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(user =>
                    user.Name.Contains(searchTerm) ||
                    user.LastName.Contains(searchTerm));
            }
        }
    }
}
