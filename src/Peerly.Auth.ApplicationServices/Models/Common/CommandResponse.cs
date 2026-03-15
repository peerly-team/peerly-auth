using OneOf;
using Peerly.Auth.Abstractions.ApplicationServices;

namespace Peerly.Auth.ApplicationServices.Models.Common;

public sealed class CommandResponse<TSuccess> : OneOfBase<TSuccess, ValidationError, OtherError>, ICommandResponse
{
    private CommandResponse(OneOf<TSuccess, ValidationError, OtherError> input)
        : base(input)
    {
    }

    public static implicit operator CommandResponse<TSuccess>(TSuccess success) => new(success);

    public static implicit operator CommandResponse<TSuccess>(ValidationError validationError) => new(validationError);

    public static implicit operator CommandResponse<TSuccess>(OtherError otherError) => new(otherError);

    public static implicit operator CommandResponse<TSuccess>(OneOf<ValidationError, OtherError> commandError)
    {
        return commandError.Match<CommandResponse<TSuccess>>(
            validationError => validationError,
            otherError => otherError);
    }

    public bool TryPickError(out OneOf<ValidationError, OtherError> error)
    {
        return !TryPickT0(out _, out error);
    }
}
