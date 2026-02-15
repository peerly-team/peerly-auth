using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Validation.Errors;
using Peerly.Auth.Exceptions;
using Peerly.Auth.Models.Sessions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal sealed class LogoutHandler : IQueryHandler<LogoutQuery, LogoutQueryResponse>
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

    public async Task<LogoutQueryResponse> ExecuteAsync(LogoutQuery query, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var session = await unitOfWork.SessionRepository.GetAsync(query.UserId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException(SessionErrors.ActiveSessionForUserNotFound);
        }

        if (!_hashService.Verify(query.RefreshToken, session.RefreshTokenHash))
        {
            throw new NotFoundException(SessionErrors.RefreshTokenForUserNotFound(query.RefreshToken));
        }

        var sessionUpdateItem = new SessionUpdateItem
        {
            Id = session.Id,
            RefreshTokenHash = session.RefreshTokenHash,
            CancellationTime = _clock.GetCurrentTime()
        };
        _ = await unitOfWork.SessionRepository.UpdateAsync(sessionUpdateItem, cancellationToken);

        return new LogoutQueryResponse();
    }
}
