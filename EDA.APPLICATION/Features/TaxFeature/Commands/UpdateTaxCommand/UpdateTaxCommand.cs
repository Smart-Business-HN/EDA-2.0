using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.TaxSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.TaxFeature.Commands.UpdateTaxCommand
{
    public class UpdateTaxCommand : IRequest<Result<Tax>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class UpdateTaxCommandHandler : IRequestHandler<UpdateTaxCommand, Result<Tax>>
    {
        private readonly IRepositoryAsync<Tax> _repositoryAsync;

        public UpdateTaxCommandHandler(IRepositoryAsync<Tax> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Tax>> Handle(UpdateTaxCommand request, CancellationToken cancellationToken)
        {
            var tax = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (tax == null)
            {
                return new Result<Tax>("Impuesto no encontrado.");
            }

            var existingTax = await _repositoryAsync.FirstOrDefaultAsync(
                new GetTaxByNameSpecification(request.Name),
                cancellationToken);

            if (existingTax != null && existingTax.Id != request.Id)
            {
                return new Result<Tax>("Ya existe otro impuesto con este nombre.");
            }

            tax.Name = request.Name;
            tax.Percentage = request.Percentage;

            await _repositoryAsync.UpdateAsync(tax, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Tax>(tax);
        }
    }
}
