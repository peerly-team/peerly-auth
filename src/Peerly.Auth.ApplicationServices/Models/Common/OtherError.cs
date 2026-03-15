using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Models.Common;

public sealed record OtherError
{
    private OtherError(ErrorType type, ErrorMessage? message)
    {
        Type = type;
        Message = message;
    }

    public ErrorType Type { get; }
    public ErrorMessage? Message { get; }

    public static OtherError PermissionDenied(ErrorMessage? errorMessage = null)
    {
        return new OtherError(ErrorType.PermissionDenied, errorMessage);
    }

    public static OtherError NotFound(ErrorMessage? errorMessage = null)
    {
        return new OtherError(ErrorType.NotFound, errorMessage);
    }

    public static OtherError Conflict(ErrorMessage? errorMessage = null)
    {
        return new OtherError(ErrorType.Conflict, errorMessage);
    }
}
