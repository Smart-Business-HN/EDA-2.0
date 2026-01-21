using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Commands.DeletePaymentTypeCommand
{
    public class DeletePaymentTypeCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeletePaymentTypeCommandHandler : IRequestHandler<DeletePaymentTypeCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<PaymentType> _repositoryAsync;

        public DeletePaymentTypeCommandHandler(IRepositoryAsync<PaymentType> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeletePaymentTypeCommand request, CancellationToken cancellationToken)
        {
            // Proteger el registro con Id 1 (Efectivo) - no se puede eliminar
            if (request.Id == 1)
            {
                return new Result<bool>("El tipo de pago 'Efectivo' no puede ser eliminado.");
            }

            var paymentType = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (paymentType == null)
            {
                return new Result<bool>("Tipo de pago no encontrado.");
            }

            await _repositoryAsync.DeleteAsync(paymentType, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
