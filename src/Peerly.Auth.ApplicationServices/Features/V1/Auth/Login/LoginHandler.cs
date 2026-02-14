using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.ApplicationServices.Validation.Errors;
using Peerly.Auth.ApplicationServices.Validation.Validators;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

internal sealed class LoginHandler : ICommandHandler<LoginCommand, LoginCommandResponse>
{
    private readonly ITokenService _tokenService;
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHashService _hashService;

    public LoginHandler(ITokenService tokenService, ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService)
    {
        _tokenService = tokenService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
    }

    public async Task<CommandResponse<LoginCommandResponse>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        if (!EmailValidator.IsValid(command.Email))
        {
            return ValidationError.From(EmailErrors.IncorrectEmailFormat);
        }

        var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var user = await unitOfWork.ReadOnlyUserRepository.GetAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return ValidationError.From(EmailErrors.NotFound);
        }

        if (!_hashService.Verify(command.Password, user.PasswordHash))
        {
            return ValidationError.From(PasswordErrors.Incorrect);
        }

        var authToken = _tokenService.CreateAuthToken(user);

        return new LoginCommandResponse
        {
            AuthToken = authToken,
            UserId = user.Id
        };
    }
}
