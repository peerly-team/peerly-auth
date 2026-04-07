using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Auth.Abstractions.ApplicationServices;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;
using Peerly.Auth.ApplicationServices.Validation.Errors;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.VerifyEmail;

internal sealed class VerifyEmailHandler : ICommandHandler<VerifyEmailCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;

    public VerifyEmailHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, IClock clock)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var verification = await unitOfWork.EmailVerificationRepository.GetByTokenAsync(command.Token, cancellationToken);

        if (verification is null)
        {
            return OtherError.NotFound();
        }

        if (verification.VerificationTime is not null)
        {
            return new Success();
        }

        if (verification.ExpirationTime < _clock.GetCurrentTime())
        {
            return ValidationError.From(EmailVerificationErrors.TokenExpired);
        }

        await unitOfWork.EmailVerificationRepository.UpdateAsync(
            verification.Id,
            builder => builder.Set(item => item.VerificationTime, _clock.GetCurrentTime()),
            cancellationToken);

        return new Success();
    }
}
