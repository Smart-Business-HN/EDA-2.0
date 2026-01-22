using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CompanySpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CompanyFeature.Commands.UpdateCompanyCommand
{
    public class UpdateCompanyCommand : IRequest<Result<Company>>
    {
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public string? RTN { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? Description { get; set; }
        public byte[]? Logo { get; set; }
    }

    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result<Company>>
    {
        private readonly IRepositoryAsync<Company> _repositoryAsync;

        public UpdateCompanyCommandHandler(IRepositoryAsync<Company> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Company>> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var existingCompany = await _repositoryAsync.FirstOrDefaultAsync(new GetCompanySpecification(), cancellationToken);

            if (existingCompany == null)
            {
                var newCompany = new Company
                {
                    Name = request.Name,
                    Owner = request.Owner,
                    RTN = request.RTN,
                    Address1 = request.Address1,
                    Address2 = request.Address2,
                    Email = request.Email,
                    PhoneNumber1 = request.PhoneNumber1,
                    PhoneNumber2 = request.PhoneNumber2,
                    Description = request.Description,
                    Logo = request.Logo
                };

                await _repositoryAsync.AddAsync(newCompany, cancellationToken);
                await _repositoryAsync.SaveChangesAsync(cancellationToken);

                return new Result<Company>(newCompany);
            }
            else
            {
                existingCompany.Name = request.Name;
                existingCompany.Owner = request.Owner;
                existingCompany.RTN = request.RTN;
                existingCompany.Address1 = request.Address1;
                existingCompany.Address2 = request.Address2;
                existingCompany.Email = request.Email;
                existingCompany.PhoneNumber1 = request.PhoneNumber1;
                existingCompany.PhoneNumber2 = request.PhoneNumber2;
                existingCompany.Description = request.Description;
                existingCompany.Logo = request.Logo;

                await _repositoryAsync.UpdateAsync(existingCompany, cancellationToken);
                await _repositoryAsync.SaveChangesAsync(cancellationToken);

                return new Result<Company>(existingCompany);
            }
        }
    }
}
