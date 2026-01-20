using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.UserSpecifications;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.UserFeature.Commands.CreateUserCommand
{
    public class CreateUserCommand : IRequest<Result<User>>
    {
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<User>>
    {
        private readonly IRepositoryAsync<User> _repositoryAsync;

        public CreateUserCommandHandler(IRepositoryAsync<User> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el nombre de usuario no exista
            var existingUser = await _repositoryAsync.FirstOrDefaultAsync(
                new GetUserUserNameSpecification(request.Name),
                cancellationToken);

            if (existingUser != null)
            {
                return new Result<User>("Ya existe un usuario con este nombre.");
            }

            var newUser = new User
            {
                Name = request.Name,
                LastName = request.LastName,
                Password = request.Password,
                RoleId = request.RoleId
            };

            await _repositoryAsync.AddAsync(newUser, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<User>(newUser);
        }
    }
}
