using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal sealed class LogoutHandler : ICommandHandler<LogoutCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHashService _hashService;
    private readonly IClock _clock;

    public LogoutHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var session = await unitOfWork.SessionRepository.GetAsync(command.UserId, cancellationToken);
        if (session is null)
        {
            return ValidationError.From(SessionErrors.ActiveSessionForUserNotFound(command.UserId));
        }

        if (!_hashService.Verify(command.RefreshToken, session.RefreshTokenHash))
        {
            return ValidationError.From(SessionErrors.RefreshTokenForUserNotFound(command.RefreshToken, command.UserId));
        }

        var sessionFilter = session.ToSessionFilter();
        var sessionUpdateItem = LogoutHandlerMapper.ToSessionUpdateItem(_clock.GetCurrentTime());
        _ = await unitOfWork.SessionRepository.UpdateAsync(sessionFilter, sessionUpdateItem, cancellationToken);

        return new Success();
    }
}
