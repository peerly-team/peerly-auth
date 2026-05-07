using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;

internal sealed class RefreshHandler : ICommandHandler<RefreshCommand, RefreshCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ITokenService _tokenService;
    private readonly ICommandValidator<RefreshCommand, RefreshCommandResponse> _validator;

    public RefreshHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, ITokenService tokenService, ICommandValidator<RefreshCommand, RefreshCommandResponse> validator)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _tokenService = tokenService;
        _validator = validator;
    }

    public async Task<CommandResponse<RefreshCommandResponse>> ExecuteAsync(
        RefreshCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        var readOnlyUnitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var userRole = await readOnlyUnitOfWork.ReadOnlyUserRepository.GetUserRoleAsync(command.UserId, cancellationToken);
        var accessToken = _tokenService.CreateAccessToken(command.UserId, userRole!.Value);

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
