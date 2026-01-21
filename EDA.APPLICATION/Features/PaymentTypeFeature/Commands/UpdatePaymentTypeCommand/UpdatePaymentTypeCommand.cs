using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PaymentTypeSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Commands.UpdatePaymentTypeCommand
{
    public class UpdatePaymentTypeCommand : IRequest<Result<PaymentType>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class UpdatePaymentTypeCommandHandler : IRequestHandler<UpdatePaymentTypeCommand, Result<PaymentType>>
    {
        private readonly IRepositoryAsync<PaymentType> _repositoryAsync;

        public UpdatePaymentTypeCommandHandler(IRepositoryAsync<PaymentType> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaymentType>> Handle(UpdatePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            // Proteger el registro con Id 1 (Efectivo) - no se puede modificar
            if (request.Id == 1)
            {
                return new Result<PaymentType>("El tipo de pago 'Efectivo' no puede ser modificado.");
            }

            var paymentType = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (paymentType == null)
            {
                return new Result<PaymentType>("Tipo de pago no encontrado.");
            }

            // Verificar que el nombre no exista en otro tipo de pago
            var existingPaymentType = await _repositoryAsync.FirstOrDefaultAsync(
                new GetPaymentTypeByNameSpecification(request.Name),
                cancellationToken);

            if (existingPaymentType != null && existingPaymentType.Id != request.Id)
            {
                return new Result<PaymentType>("Ya existe otro tipo de pago con este nombre.");
            }

            paymentType.Name = request.Name;

            await _repositoryAsync.UpdateAsync(paymentType, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<PaymentType>(paymentType);
        }
    }
}
