using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Validation.Errors;
using Peerly.Auth.Exceptions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Logout;

internal sealed class LogoutHandler : IQueryHandler<LogoutQuery, LogoutQueryResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHashService _hashService;
    private readonly ILogoutHandlerMapper _mapper;

    public LogoutHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService, ILogoutHandlerMapper mapper)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
        _mapper = mapper;
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

        var sessionFilter = LogoutHandlerMapper.ToSessionFilter(session);
        var sessionUpdateItem = _mapper.ToSessionUpdateItem();
        _ = await unitOfWork.SessionRepository.UpdateAsync(sessionFilter, sessionUpdateItem, cancellationToken);

        return new LogoutQueryResponse();
    }
}
