using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CashRegisterSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CashRegisterFeature.Queries
{
    public class GetAllCashRegistersQuery : IRequest<Result<PaginatedResult<CashRegister>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllCashRegistersQueryHandler : IRequestHandler<GetAllCashRegistersQuery, Result<PaginatedResult<CashRegister>>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;

        public GetAllCashRegistersQueryHandler(IRepositoryAsync<CashRegister> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<CashRegister>>> Handle(GetAllCashRegistersQuery request, CancellationToken cancellationToken)
        {
            List<CashRegister> cashRegisters;
            int totalCount;

            if (request.GetAll)
            {
                cashRegisters = await _repositoryAsync.ListAsync(
                    new FilterCashRegistersSpecification(request.SearchTerm, request.IsActive),
                    cancellationToken);
                totalCount = cashRegisters.Count;

                var allResult = new PaginatedResult<CashRegister>(cashRegisters, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<CashRegister>>(allResult);
            }
            else
            {
                cashRegisters = await _repositoryAsync.ListAsync(
                    new FilterCashRegistersSpecification(request.SearchTerm, request.IsActive, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountCashRegistersSpecification(request.SearchTerm, request.IsActive),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<CashRegister>(cashRegisters, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<CashRegister>>(paginatedResult);
            }
        }
    }

    public class GetActiveCashRegistersQuery : IRequest<Result<List<CashRegister>>>
    {
    }

    public class GetActiveCashRegistersQueryHandler : IRequestHandler<GetActiveCashRegistersQuery, Result<List<CashRegister>>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;

        public GetActiveCashRegistersQueryHandler(IRepositoryAsync<CashRegister> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<List<CashRegister>>> Handle(GetActiveCashRegistersQuery request, CancellationToken cancellationToken)
        {
            var cashRegisters = await _repositoryAsync.ListAsync(
                new GetActiveCashRegistersSpecification(),
                cancellationToken);

            return new Result<List<CashRegister>>(cashRegisters);
        }
    }

    public class GetCashRegisterByIdQuery : IRequest<Result<CashRegister>>
    {
        public int Id { get; set; }
    }

    public class GetCashRegisterByIdQueryHandler : IRequestHandler<GetCashRegisterByIdQuery, Result<CashRegister>>
    {
        private readonly IRepositoryAsync<CashRegister> _repositoryAsync;

        public GetCashRegisterByIdQueryHandler(IRepositoryAsync<CashRegister> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<CashRegister>> Handle(GetCashRegisterByIdQuery request, CancellationToken cancellationToken)
        {
            var cashRegister = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCashRegisterByIdWithPrinterSpecification(request.Id),
                cancellationToken);

            if (cashRegister == null)
            {
                return new Result<CashRegister>("Caja registradora no encontrada.");
            }

            return new Result<CashRegister>(cashRegister);
        }
    }
}
