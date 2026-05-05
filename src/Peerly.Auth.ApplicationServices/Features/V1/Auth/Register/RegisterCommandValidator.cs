using System.Threading;
using System.Threading.Tasks;
using Devolutions.Zxcvbn;
using Peerly.Auth.Abstractions.UnitOfWork;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Features.Validation.Errors;
using Peerly.Auth.ApplicationServices.Models.Common;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.Register;

internal sealed class RegisterCommandValidator : ICommandValidator<RegisterCommand, RegisterCommandResponse>
{
    private readonly ICommonUnitOfWorkFactory _unitOfWorkFactory;

    public RegisterCommandValidator(ICommonUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<CommandValidationResult> ValidateAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var readOnlyUnitOfWork = await _unitOfWorkFactory.CreateReadOnlyAsync(cancellationToken);

        var isEmailExists = await readOnlyUnitOfWork.ReadOnlyUserRepository.ExistsAsync(command.Email, cancellationToken);
        if (isEmailExists)
        {
            return ValidationError.From(EmailErrors.EmailAlreadyUsed);
        }

        if (Zxcvbn.Evaluate(command.Password).Score < 3)
        {
            return ValidationError.From(PasswordErrors.IsTooSimple);
        }

        return CommandValidationResult.Ok();
    }
}
