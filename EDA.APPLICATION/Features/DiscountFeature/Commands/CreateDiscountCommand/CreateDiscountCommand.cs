using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.DiscountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.DiscountFeature.Commands.CreateDiscountCommand
{
    public class CreateDiscountCommand : IRequest<Result<Discount>>
    {
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Result<Discount>>
    {
        private readonly IRepositoryAsync<Discount> _repositoryAsync;

        public CreateDiscountCommandHandler(IRepositoryAsync<Discount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Discount>> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingDiscount = await _repositoryAsync.FirstOrDefaultAsync(
                new GetDiscountByNameSpecification(request.Name),
                cancellationToken);

            if (existingDiscount != null)
            {
                return new Result<Discount>("Ya existe un descuento con este nombre.");
            }

            var newDiscount = new Discount
            {
                Name = request.Name,
                Percentage = request.Percentage
            };

            await _repositoryAsync.AddAsync(newDiscount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Discount>(newDiscount);
        }
    }
}
