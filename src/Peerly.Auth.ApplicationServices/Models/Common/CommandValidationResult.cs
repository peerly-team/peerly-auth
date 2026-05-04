using OneOf;
using OneOf.Types;

namespace Peerly.Auth.ApplicationServices.Models.Common;

internal sealed class CommandValidationResult : OneOfBase<Success, ValidationError, OtherError>
{
    private CommandValidationResult(OneOf<Success, ValidationError, OtherError> input)
        : base(input)
    {
    }

    public static CommandValidationResult Ok()
    {
        return new CommandValidationResult(new Success());
    }

    public static implicit operator CommandValidationResult(ValidationError error)
    {
        return new CommandValidationResult(error);
    }

    public static implicit operator CommandValidationResult(OtherError error)
    {
        return new CommandValidationResult(error);
    }

    public bool TryPickError(out OneOf<ValidationError, OtherError> error)
    {
        return !TryPickT0(out _, out error);
    }
}
