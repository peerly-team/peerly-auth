using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Login.Abstractions;
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
    private readonly ILoginHandlerMapper _mapper;

    public LoginHandler(
        ITokenService tokenService,
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        IHashService hashService,
        ILoginHandlerMapper mapper)
    {
        _tokenService = tokenService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
        _mapper = mapper;
    }

    public async Task<CommandResponse<LoginCommandResponse>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        if (!EmailValidator.IsValid(command.Email))
        {
            return ValidationError.From(EmailErrors.IncorrectEmailFormat);
        }

        var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var user = await unitOfWork.UserRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return ValidationError.From(EmailErrors.NotFound);
        }

        if (!_hashService.Verify(command.Password, user.PasswordHash))
        {
            return ValidationError.From(PasswordErrors.Incorrect);
        }

        // NOTE: Проверка существования активных сессий - если есть, то обнуляем и создаем новую
        var session = await unitOfWork.SessionRepository.GetAsync(user.Id, cancellationToken);
        if (session is not null)
        {
            var sessionFilter = LoginHandlerMapper.ToSessionFilter(session);
            var sessionUpdateItem = _mapper.ToSessionUpdateItem();
            await unitOfWork.SessionRepository.UpdateAsync(sessionFilter, sessionUpdateItem, cancellationToken);
        }

        var authToken = _tokenService.CreateAuthToken(user.Id, user.Role);
        var refreshTokenHash = await _hashService.HashAsync(authToken.RefreshToken, cancellationToken);
        var sessionAddItem = _mapper.ToSessionAddItem(user.Id, refreshTokenHash);
        _ = await unitOfWork.SessionRepository.AddAsync(sessionAddItem, cancellationToken);

        return new LoginCommandResponse
        {
            UserId = user.Id,
            AuthToken = authToken
        };
    }
}
