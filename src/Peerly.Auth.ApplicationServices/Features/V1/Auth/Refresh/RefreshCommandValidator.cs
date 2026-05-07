using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Refresh;

internal sealed class RefreshCommandValidator : ICommandValidator<RefreshCommand, RefreshCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHashService _hashService;

    public RefreshCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
    }

    public async Task<CommandValidationResult> ValidateAsync(RefreshCommand command, CancellationToken cancellationToken)
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
            return OtherError.NotFound();
        }

        return CommandValidationResult.Ok();
    }
}
