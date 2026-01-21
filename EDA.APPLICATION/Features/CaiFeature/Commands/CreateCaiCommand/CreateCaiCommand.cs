using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CaiSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CaiFeature.Commands.CreateCaiCommand
{
    public class CreateCaiCommand : IRequest<Result<Cai>>
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int InitialCorrelative { get; set; }
        public int FinalCorrelative { get; set; }
        public string Prefix { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class CreateCaiCommandHandler : IRequestHandler<CreateCaiCommand, Result<Cai>>
    {
        private readonly IRepositoryAsync<Cai> _repositoryAsync;

        public CreateCaiCommandHandler(IRepositoryAsync<Cai> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Cai>> Handle(CreateCaiCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el código no exista
            var existingCai = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCaiByCodeSpecification(request.Code),
                cancellationToken);

            if (existingCai != null)
            {
                return new Result<Cai>("Ya existe un CAI con este código.");
            }

            // Verificar que el prefix no exista
            var existingPrefix = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCaiByPrefixSpecification(request.Prefix),
                cancellationToken);

            if (existingPrefix != null)
            {
                return new Result<Cai>("Ya existe un CAI con este prefijo.");
            }

            // Calcular PendingInvoices
            int pendingInvoices = request.FinalCorrelative - request.InitialCorrelative + 1;

            var newCai = new Cai
            {
                Name = request.Name,
                Code = request.Code,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                InitialCorrelative = request.InitialCorrelative,
                FinalCorrelative = request.FinalCorrelative,
                CurrentCorrelative = request.InitialCorrelative, // Inicia en el correlativo inicial
                PendingInvoices = pendingInvoices,
                Prefix = request.Prefix,
                IsActive = request.IsActive
            };

            await _repositoryAsync.AddAsync(newCai, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Cai>(newCai);
        }
    }
}
