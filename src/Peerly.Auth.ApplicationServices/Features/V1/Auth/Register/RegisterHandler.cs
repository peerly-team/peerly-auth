using System.Threading;
using System.Threading.Tasks;
using Devolutions.Zxcvbn;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;
using Peerly.Auth.ApplicationServices.Services.Tokens.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

internal sealed class RegisterHandler : ICommandHandler<RegisterCommand, RegisterCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ITokenService _tokenService;
    private readonly IHashService _hashService;
    private readonly IRegisterHandlerMapper _mapper;

    public RegisterHandler(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        ITokenService tokenService,
        IHashService hashService,
        IRegisterHandlerMapper mapper)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _tokenService = tokenService;
        _hashService = hashService;
        _mapper = mapper;
    }

    public async Task<CommandResponse<RegisterCommandResponse>> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var (isError, error) = await CommandValidateAsync(command, unitOfWork, cancellationToken);
        if (isError)
        {
            return error!;
        }

        // NOTE: высчитываем хеш пароля перед открытием транзакции к БД, поскольку очень тяжелая операция
        var emailVerificationToken = _tokenService.CreateRefreshToken();
        var passwordHash = await _hashService.HashAsync(command.Password, cancellationToken);

        var setOperations = await unitOfWork.StartOperationSet(cancellationToken);

        var userAddItem = _mapper.ToUserAddItem(command, passwordHash);
        var userId = await unitOfWork.UserRepository.AddAsync(userAddItem, cancellationToken);

        var emailVerificationAddItem = _mapper.ToEmailVerificationAddItem(userId, emailVerificationToken);
        _ = await unitOfWork.EmailVerificationRepository.AddAsync(emailVerificationAddItem, cancellationToken);

        var authToken = _tokenService.CreateAuthToken(userId, command.Role);

        var refreshTokenHash = await _hashService.HashAsync(authToken.RefreshToken, cancellationToken);
        var sessionAddItem = _mapper.ToSessionAddItem(userId, refreshTokenHash);
        _ = await unitOfWork.SessionRepository.AddAsync(sessionAddItem, cancellationToken);

        var outboxMessage = _mapper.ToOutboxMessage(userId, command);
        _ = await unitOfWork.OutboxRepository.AddAsync(outboxMessage, cancellationToken);

        await setOperations.Complete(cancellationToken);

        return new RegisterCommandResponse
        {
            UserId = userId,
            AuthToken = authToken
        };
    }

    // todo: добавить валидатор для V1RegisterRequest, чтобы там проверять формат почты
    private static async Task<(bool, ValidationError?)> CommandValidateAsync(
        RegisterCommand command,
        ICommonUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var email = command.Email;
        var isEmailExists = await unitOfWork.UserRepository.ExistsAsync(email, cancellationToken);
        if (isEmailExists)
        {
            return (true, ValidationError.From(EmailErrors.EmailAlreadyUsed));
        }

        if (Zxcvbn.Evaluate(command.Password).Score < 3)
        {
            return (true, ValidationError.From(PasswordErrors.IsTooSimple));
        }

        return (false, null);
    }
}
