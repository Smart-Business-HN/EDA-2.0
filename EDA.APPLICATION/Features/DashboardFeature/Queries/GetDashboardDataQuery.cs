using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using EDA.APPLICATION.Wrappers;
using MediatR;

namespace EDA.APPLICATION.Features.DashboardFeature.Queries
{
    public class GetDashboardDataQuery : IRequest<Result<DashboardData>>
    {
    }

    public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, Result<DashboardData>>
    {
        private readonly IDashboardService _dashboardService;

        public GetDashboardDataQueryHandler(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<Result<DashboardData>> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync(cancellationToken);
            return new Result<DashboardData>(dashboardData);
        }
    }
}
