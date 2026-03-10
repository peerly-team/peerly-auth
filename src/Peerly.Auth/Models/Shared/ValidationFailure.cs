using System.Collections.Generic;

namespace Peerly.Auth.Models.Shared;

public sealed class ValidationFailure
{
    public ValidationFailure(IReadOnlyList<ErrorMessage> errorMessages)
    {
        ErrorMessages = errorMessages;
    }

    public ValidationFailure(ErrorMessage errorMessage)
    {
        ErrorMessages = new[] { errorMessage };
    }

    public IReadOnlyList<ErrorMessage> ErrorMessages { get; }
}
