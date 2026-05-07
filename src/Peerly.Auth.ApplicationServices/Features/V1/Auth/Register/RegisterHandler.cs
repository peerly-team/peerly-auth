using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.V1.Auth.Register.Abstractions;
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
    private readonly ICommandValidator<RegisterCommand, RegisterCommandResponse> _validator;

    public RegisterHandler(
        ICommonUnitOfWorkFactory unitOfWorkFactory,
        ITokenService tokenService,
        IHashService hashService,
        IRegisterHandlerMapper mapper,
        ICommandValidator<RegisterCommand, RegisterCommandResponse> validator)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _tokenService = tokenService;
        _hashService = hashService;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CommandResponse<RegisterCommandResponse>> ExecuteAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

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
}
