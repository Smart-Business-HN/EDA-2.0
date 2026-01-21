using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.TaxSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.TaxFeature.Commands.CreateTaxCommand
{
    public class CreateTaxCommand : IRequest<Result<Tax>>
    {
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class CreateTaxCommandHandler : IRequestHandler<CreateTaxCommand, Result<Tax>>
    {
        private readonly IRepositoryAsync<Tax> _repositoryAsync;

        public CreateTaxCommandHandler(IRepositoryAsync<Tax> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Tax>> Handle(CreateTaxCommand request, CancellationToken cancellationToken)
        {
            var existingTax = await _repositoryAsync.FirstOrDefaultAsync(
                new GetTaxByNameSpecification(request.Name),
                cancellationToken);

            if (existingTax != null)
            {
                return new Result<Tax>("Ya existe un impuesto con este nombre.");
            }

            var newTax = new Tax
            {
                Name = request.Name,
                Percentage = request.Percentage
            };

            await _repositoryAsync.AddAsync(newTax, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Tax>(newTax);
        }
    }
}
