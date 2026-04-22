using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Validation.Errors;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;

internal sealed class ConfirmEmailHandler : ICommandHandler<ConfirmEmailCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public ConfirmEmailHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var verification = await unitOfWork.EmailVerificationRepository.GetUserExpirationTimeByTokenAsync(command.Token, cancellationToken);
        if (verification is null)
        {
            return OtherError.NotFound();
        }

        var isConfirmed = await unitOfWork.UserRepository.IsEmailConfirmedAsync(verification.UserId, cancellationToken);
        if (isConfirmed)
        {
            return new Success();
        }

        if (verification.ExpirationTime < _clock.GetCurrentTime())
        {
            return ValidationError.From(EmailVerificationErrors.TokenExpired);
        }

        await unitOfWork.UserRepository.UpdateAsync(
            verification.UserId,
            builder => builder.Set(item => item.IsConfirmed, true),
            cancellationToken);

        return new Success();
    }
}
