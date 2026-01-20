using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.FamilyFeature.Commands.DeleteFamilyCommand
{
    public class DeleteFamilyCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand, Result<bool>>
    {
        private readonly IRepositoryAsync<Family> _repositoryAsync;

        public DeleteFamilyCommandHandler(IRepositoryAsync<Family> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<bool>> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
        {
            var family = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (family == null)
            {
                return new Result<bool>("Familia no encontrada.");
            }

            await _repositoryAsync.DeleteAsync(family, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<bool>(true);
        }
    }
}
