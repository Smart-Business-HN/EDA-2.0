using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.CaiSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.CaiFeature.Commands.UpdateCaiCommand
{
    public class UpdateCaiCommand : IRequest<Result<Cai>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int InitialCorrelative { get; set; }
        public int FinalCorrelative { get; set; }
        public string Prefix { get; set; } = null!;
        public bool IsActive { get; set; }
        public int? CurrentCorrelative { get; set; }
    }

    public class UpdateCaiCommandHandler : IRequestHandler<UpdateCaiCommand, Result<Cai>>
    {
        private readonly IRepositoryAsync<Cai> _repositoryAsync;
        private readonly IRepositoryAsync<Invoice> _invoiceRepository;

        public UpdateCaiCommandHandler(IRepositoryAsync<Cai> repositoryAsync, IRepositoryAsync<Invoice> invoiceRepository)
        {
            _repositoryAsync = repositoryAsync;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Result<Cai>> Handle(UpdateCaiCommand request, CancellationToken cancellationToken)
        {
            var cai = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (cai == null)
            {
                return new Result<Cai>("CAI no encontrado.");
            }

            // Verificar que el nuevo código no exista (excepto si es el mismo CAI)
            var existingCai = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCaiByCodeSpecification(request.Code),
                cancellationToken);

            if (existingCai != null && existingCai.Id != request.Id)
            {
                return new Result<Cai>("Ya existe un CAI con este código.");
            }

            // Verificar que el nuevo prefix no exista (excepto si es el mismo CAI)
            var existingPrefix = await _repositoryAsync.FirstOrDefaultAsync(
                new GetCaiByPrefixSpecification(request.Prefix),
                cancellationToken);

            if (existingPrefix != null && existingPrefix.Id != request.Id)
            {
                return new Result<Cai>("Ya existe un CAI con este prefijo.");
            }

            // Validar que no se modifiquen los correlativos si ya se han usado facturas
            if (cai.CurrentCorrelative > cai.InitialCorrelative)
            {
                if (request.InitialCorrelative != cai.InitialCorrelative)
                {
                    return new Result<Cai>("No se puede modificar el correlativo inicial porque ya se han emitido facturas con este CAI.");
                }
            }

            // Si se proporciona un nuevo correlativo actual, aplicarlo
            if (request.CurrentCorrelative.HasValue)
            {
                if (request.CurrentCorrelative.Value < request.InitialCorrelative || request.CurrentCorrelative.Value > request.FinalCorrelative)
                {
                    return new Result<Cai>("El correlativo actual debe estar entre el correlativo inicial y final.");
                }

                // Verificar que el número de factura del nuevo correlativo no haya sido emitido ya
                string targetInvoiceNumber = $"{cai.Prefix}{request.CurrentCorrelative.Value:D8}";
                var existingInvoice = await _invoiceRepository.FirstOrDefaultAsync(
                    new Specifications.InvoiceSpecification.GetInvoiceByNumberSpecification(targetInvoiceNumber),
                    cancellationToken);

                if (existingInvoice != null)
                {
                    return new Result<Cai>($"No se puede asignar el correlativo {request.CurrentCorrelative.Value} porque la factura {targetInvoiceNumber} ya fue emitida.");
                }

                cai.CurrentCorrelative = request.CurrentCorrelative.Value;
            }

            // Recalcular PendingInvoices basado en el nuevo FinalCorrelative y CurrentCorrelative
            int pendingInvoices = request.FinalCorrelative - cai.CurrentCorrelative + 1;
            if (pendingInvoices < 0) pendingInvoices = 0;

            cai.Name = request.Name;
            cai.Code = request.Code;
            cai.FromDate = request.FromDate;
            cai.ToDate = request.ToDate;
            cai.InitialCorrelative = request.InitialCorrelative;
            cai.FinalCorrelative = request.FinalCorrelative;
            cai.PendingInvoices = pendingInvoices;
            cai.Prefix = request.Prefix;
            cai.IsActive = request.IsActive;

            await _repositoryAsync.UpdateAsync(cai, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Cai>(cai);
        }
    }
}
