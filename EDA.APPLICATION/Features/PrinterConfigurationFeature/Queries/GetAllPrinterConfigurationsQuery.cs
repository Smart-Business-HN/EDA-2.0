using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.PrinterConfigurationSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.PrinterConfigurationFeature.Queries
{
    public class GetAllPrinterConfigurationsQuery : IRequest<Result<PaginatedResult<PrinterConfiguration>>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllPrinterConfigurationsQueryHandler : IRequestHandler<GetAllPrinterConfigurationsQuery, Result<PaginatedResult<PrinterConfiguration>>>
    {
        private readonly IRepositoryAsync<PrinterConfiguration> _repositoryAsync;

        public GetAllPrinterConfigurationsQueryHandler(IRepositoryAsync<PrinterConfiguration> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<PrinterConfiguration>>> Handle(GetAllPrinterConfigurationsQuery request, CancellationToken cancellationToken)
        {
            List<PrinterConfiguration> configurations;
            int totalCount;

            if (request.GetAll)
            {
                configurations = await _repositoryAsync.ListAsync(
                    new FilterPrinterConfigurationsSpecification(request.SearchTerm, request.IsActive),
                    cancellationToken);
                totalCount = configurations.Count;

                var allResult = new PaginatedResult<PrinterConfiguration>(configurations, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<PrinterConfiguration>>(allResult);
            }
            else
            {
                configurations = await _repositoryAsync.ListAsync(
                    new FilterPrinterConfigurationsSpecification(request.SearchTerm, request.IsActive, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountPrinterConfigurationsSpecification(request.SearchTerm, request.IsActive),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<PrinterConfiguration>(configurations, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<PrinterConfiguration>>(paginatedResult);
            }
        }
    }
}
