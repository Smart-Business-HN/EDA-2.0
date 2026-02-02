using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ShiftSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Queries
{
    public class GetAllShiftsQuery : IRequest<Result<PaginatedResult<Shift>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsOpen { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllShiftsQueryHandler : IRequestHandler<GetAllShiftsQuery, Result<PaginatedResult<Shift>>>
    {
        private readonly IRepositoryAsync<Shift> _repositoryAsync;

        public GetAllShiftsQueryHandler(IRepositoryAsync<Shift> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Shift>>> Handle(GetAllShiftsQuery request, CancellationToken cancellationToken)
        {
            List<Shift> shifts;
            int totalCount;

            if (request.GetAll)
            {
                shifts = await _repositoryAsync.ListAsync(
                    new FilterShiftsSpecification(request.SearchTerm, request.IsOpen),
                    cancellationToken);
                totalCount = shifts.Count;

                var allResult = new PaginatedResult<Shift>(shifts, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Shift>>(allResult);
            }
            else
            {
                shifts = await _repositoryAsync.ListAsync(
                    new FilterShiftsSpecification(request.SearchTerm, request.IsOpen, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountShiftsSpecification(request.SearchTerm, request.IsOpen),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Shift>(shifts, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Shift>>(paginatedResult);
            }
        }
    }
}
