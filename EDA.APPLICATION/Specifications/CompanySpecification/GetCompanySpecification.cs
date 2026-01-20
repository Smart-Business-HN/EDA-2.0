using Ardalis.Specification;
using EDA.DOMAIN.Entities;

namespace EDA.APPLICATION.Specifications.CompanySpecification
{
    public sealed class GetCompanySpecification : Specification<Company>
    {
        public GetCompanySpecification()
        {
            Query.Take(1);
        }
    }
}
