using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ShiftFeature.Commands.CreateShiftCommand
{
    public class CreateShiftCommand : IRequest<Result<Shift>>
    {
        public int UserId { get; set; }
        public string ShiftType { get; set; } = null!;
        public decimal InitialAmount { get; set; }
    }

    public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Result<Shift>>
    {
        private readonly IRepositoryAsync<Shift> _repositoryAsync;

        public CreateShiftCommandHandler(IRepositoryAsync<Shift> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Shift>> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
        {
            var newShift = new Shift
            {
                UserId = request.UserId,
                ShiftType = request.ShiftType,
                StartTime = DateTime.Now,
                InitialAmount = request.InitialAmount,
                IsOpen = true
            };

            await _repositoryAsync.AddAsync(newShift, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Shift>(newShift);
        }
    }
}
