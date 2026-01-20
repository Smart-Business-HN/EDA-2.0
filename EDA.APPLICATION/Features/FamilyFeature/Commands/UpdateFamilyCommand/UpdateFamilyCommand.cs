using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.FamilySpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.FamilyFeature.Commands.UpdateFamilyCommand
{
    public class UpdateFamilyCommand : IRequest<Result<Family>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, Result<Family>>
    {
        private readonly IRepositoryAsync<Family> _repositoryAsync;

        public UpdateFamilyCommandHandler(IRepositoryAsync<Family> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Family>> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
        {
            var family = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (family == null)
            {
                return new Result<Family>("Familia no encontrada.");
            }

            // Verificar que el nombre no exista en otra familia
            var existingFamily = await _repositoryAsync.FirstOrDefaultAsync(
                new GetFamilyByNameSpecification(request.Name),
                cancellationToken);

            if (existingFamily != null && existingFamily.Id != request.Id)
            {
                return new Result<Family>("Ya existe otra familia con este nombre.");
            }

            family.Name = request.Name;

            await _repositoryAsync.UpdateAsync(family, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Family>(family);
        }
    }
}
