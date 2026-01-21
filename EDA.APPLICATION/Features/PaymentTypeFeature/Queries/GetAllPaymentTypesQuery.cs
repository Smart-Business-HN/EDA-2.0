using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PaymentTypeSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PaymentTypeFeature.Queries
{
    public class GetAllPaymentTypesQuery : IRequest<Result<PaginatedResult<PaymentType>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllPaymentTypesQueryHandler : IRequestHandler<GetAllPaymentTypesQuery, Result<PaginatedResult<PaymentType>>>
    {
        private readonly IRepositoryAsync<PaymentType> _repositoryAsync;

        public GetAllPaymentTypesQueryHandler(IRepositoryAsync<PaymentType> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<PaymentType>>> Handle(GetAllPaymentTypesQuery request, CancellationToken cancellationToken)
        {
            List<PaymentType> paymentTypes;
            int totalCount;

            if (request.GetAll)
            {
                // Obtener todos los tipos de pago sin paginación
                paymentTypes = await _repositoryAsync.ListAsync(
                    new FilterPaymentTypesSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = paymentTypes.Count;

                var allResult = new PaginatedResult<PaymentType>(paymentTypes, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<PaymentType>>(allResult);
            }
            else
            {
                // Obtener con paginación
                paymentTypes = await _repositoryAsync.ListAsync(
                    new FilterPaymentTypesSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountPaymentTypesSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<PaymentType>(paymentTypes, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<PaymentType>>(paginatedResult);
            }
        }
    }
}
