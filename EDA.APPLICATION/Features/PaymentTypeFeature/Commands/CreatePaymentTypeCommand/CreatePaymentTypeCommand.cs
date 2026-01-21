using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PaymentTypeSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Commands.CreatePaymentTypeCommand
{
    public class CreatePaymentTypeCommand : IRequest<Result<PaymentType>>
    {
        public string Name { get; set; } = null!;
    }

    public class CreatePaymentTypeCommandHandler : IRequestHandler<CreatePaymentTypeCommand, Result<PaymentType>>
    {
        private readonly IRepositoryAsync<PaymentType> _repositoryAsync;

        public CreatePaymentTypeCommandHandler(IRepositoryAsync<PaymentType> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaymentType>> Handle(CreatePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingPaymentType = await _repositoryAsync.FirstOrDefaultAsync(
                new GetPaymentTypeByNameSpecification(request.Name),
                cancellationToken);

            if (existingPaymentType != null)
            {
                return new Result<PaymentType>("Ya existe un tipo de pago con este nombre.");
            }

            var newPaymentType = new PaymentType
            {
                Name = request.Name
            };

            await _repositoryAsync.AddAsync(newPaymentType, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PaymentType>(newPaymentType);
        }
    }
}
