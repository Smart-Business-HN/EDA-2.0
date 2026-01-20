using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.FamilySpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.FamilyFeature.Commands.CreateFamilyCommand
{
    public class CreateFamilyCommand : IRequest<Result<Family>>
    {
        public string Name { get; set; } = null!;
    }

    public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<Family>>
    {
        private readonly IRepositoryAsync<Family> _repositoryAsync;

        public CreateFamilyCommandHandler(IRepositoryAsync<Family> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<Family>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre no exista
            var existingFamily = await _repositoryAsync.FirstOrDefaultAsync(
                new GetFamilyByNameSpecification(request.Name),
                cancellationToken);

            if (existingFamily != null)
            {
                return new Result<Family>("Ya existe una familia con este nombre.");
            }

            var newFamily = new Family
            {
                Name = request.Name
            };

            await _repositoryAsync.AddAsync(newFamily, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<Family>(newFamily);
        }
    }
}
