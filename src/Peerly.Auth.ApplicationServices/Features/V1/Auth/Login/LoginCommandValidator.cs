using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Services.Abstractions;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Login;

internal sealed class LoginCommandValidator : ICommandValidator<LoginCommand, LoginCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHashService _hashService;

    public LoginCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IHashService hashService)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _hashService = hashService;
    }

    public async Task<CommandValidationResult> ValidateAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var user = await unitOfWork.ReadOnlyUserRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return ValidationError.From(EmailErrors.NotFound);
        }

        if (!_hashService.Verify(command.Password, user.PasswordHash))
        {
            return ValidationError.From(PasswordErrors.Incorrect);
        }

        return CommandValidationResult.Ok();
    }
}
