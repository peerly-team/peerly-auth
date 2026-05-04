using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Validation.Errors;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;

internal sealed class ConfirmEmailCommandValidator : ICommandValidator<ConfirmEmailCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public ConfirmEmailCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandValidationResult> ValidateAsync(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var userExpirationTime = await unitOfWork.ReadOnlyEmailVerificationRepository.GetUserExpirationTimeByTokenAsync(command.Token, cancellationToken);
        if (userExpirationTime is null)
        {
            return OtherError.NotFound();
        }

        if (userExpirationTime.ExpirationTime < _clock.GetCurrentTime())
        {
            return ValidationError.From(EmailVerificationErrors.TokenExpired);
        }

        return CommandValidationResult.Ok();
    }
}
