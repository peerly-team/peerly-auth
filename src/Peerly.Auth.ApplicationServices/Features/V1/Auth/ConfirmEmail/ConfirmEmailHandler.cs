using System.Threading;
using System.Threading.Tasks;
using OneOf.Types;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Models.Common;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.ConfirmEmail;

internal sealed class ConfirmEmailHandler : ICommandHandler<ConfirmEmailCommand, Success>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ICommandValidator<ConfirmEmailCommand, Success> _validator;

    public ConfirmEmailHandler(ICommonUnitOfWorkFactory unitOfWorkFactory, ICommandValidator<ConfirmEmailCommand, Success> validator)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _validator = validator;
    }

    public async Task<CommandResponse<Success>> ExecuteAsync(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.TryPickError(out var error))
        {
            return error;
        }

        await using var unitOfWork = await _unitOfWorkFactory.CreateAsync(cancellationToken);

        var userId = await unitOfWork.EmailVerificationRepository.GetUserIdByTokenAsync(command.Token, cancellationToken);
        await unitOfWork.UserRepository.UpdateAsync(
            userId!.Value,
            builder => builder.Set(item => item.IsConfirmed, true),
            cancellationToken);

        return new Success();
    }
}
