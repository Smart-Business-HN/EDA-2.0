using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.DiscountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.DiscountFeature.Commands.UpdateDiscountCommand
{
    public class UpdateDiscountCommand : IRequest<Result<Discount>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }

    public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, Result<Discount>>
    {
        private readonly IRepositoryAsync<Discount> _repositoryAsync;

        public UpdateDiscountCommandHandler(IRepositoryAsync<Discount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Discount>> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
        {
            var discount = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (discount == null)
            {
                return new Result<Discount>("Descuento no encontrado.");
            }

            // Verificar que el nombre no exista en otro descuento
            var existingDiscount = await _repositoryAsync.FirstOrDefaultAsync(
                new GetDiscountByNameSpecification(request.Name),
                cancellationToken);

            if (existingDiscount != null && existingDiscount.Id != request.Id)
            {
                return new Result<Discount>("Ya existe otro descuento con este nombre.");
            }

            discount.Name = request.Name;
            discount.Percentage = request.Percentage;

            await _repositoryAsync.UpdateAsync(discount, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Discount>(discount);
        }
    }
}
