using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Exceptions;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.RefreshAccessToken;

internal sealed class RefreshHandler : ICommandHandler<RefreshCommand, RefreshCommandResponse>
{
    public required ICommonUnitOfWorkFactory _unitOfWorkFactory;
    public required IHashService _hashService;
    public required ITokenService _tokenService;

    public RefreshHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService, ITokenService tokenService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
        _tokenService = tokenService;
    }

    public async Task<CommandResponse<RefreshCommandResponse>> ExecuteAsync(
        RefreshCommand command,
        CancellationToken cancellationToken)
    {
        var readOnlyUnitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var session = await readOnlyUnitOfWork.ReadOnlySessionRepository.GetAsync(command.UserId, cancellationToken);
        if (session is null)
        {
            return ValidationError.From(SessionErrors.ActiveSessionForUserNotFound(command.UserId));
        }

        if (!_hashService.Verify(command.RefreshToken, session.RefreshTokenHash))
        {
            return ValidationError.From(SessionErrors.RefreshTokenForUserNotFound(command.RefreshToken, command.UserId));
        }

        var userRole = await readOnlyUnitOfWork.ReadOnlyUserRepository.GetUserRoleAsync(command.UserId, cancellationToken);
        if (userRole is null)
        {
            throw new NotFoundException();
        }

        var accessToken = _tokenService.CreateAccessToken(command.UserId, userRole.Value);

        return new RefreshCommandResponse
        {
            AuthToken = new AuthToken
            {
                AccessToken = accessToken,
                RefreshToken = command.RefreshToken
            }
        };
    }
}
