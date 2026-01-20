using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.UserSpecifications;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.UserFeature.Commands.UpdateUserCommand
{
    public class UpdateUserCommand : IRequest<Result<User>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Password { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<User>>
    {
        private readonly IRepositoryAsync<User> _repositoryAsync;

        public UpdateUserCommandHandler(IRepositoryAsync<User> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<User>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repositoryAsync.GetByIdAsync(request.Id, cancellationToken);

            if (user == null)
            {
                return new Result<User>("Usuario no encontrado.");
            }

            // Verificar que el nombre no exista en otro usuario
            var existingUser = await _repositoryAsync.FirstOrDefaultAsync(
                new GetUserUserNameSpecification(request.Name),
                cancellationToken);

            if (existingUser != null && existingUser.Id != request.Id)
            {
                return new Result<User>("Ya existe otro usuario con este nombre.");
            }

            user.Name = request.Name;
            user.LastName = request.LastName;
            user.RoleId = request.RoleId;

            // Solo actualizar contraseña si se proporciona una nueva
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = request.Password;
            }

            await _repositoryAsync.UpdateAsync(user, cancellationToken);
            await _repositoryAsync.SaveChangesAsync(cancellationToken);

            return new Result<User>(user);
        }
    }
}
