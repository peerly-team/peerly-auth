using FluentValidation;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Validators.Auth;

internal sealed class V1ConfirmEmailRequestValidator : AbstractValidator<V1ConfirmEmailRequest>
{
    public V1ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotNull()
            .NotEmpty();
    }
}
