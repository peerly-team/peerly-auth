using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;

namespace Peerly.Auth.Api.Exceptions;

[Serializable]
internal sealed class FormatValidationException : ValidationException
{
    public FormatValidationException(string message)
        : base(message)
    {
    }

    public FormatValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message, errors)
    {
    }

    public FormatValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(message, errors, appendDefaultMessage)
    {
    }

    public FormatValidationException(IEnumerable<ValidationFailure> errors)
        : base(errors)
    {
    }
}
