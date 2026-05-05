using FluentValidation;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Validators.Auth;

internal sealed class V1RefreshRequestValidator : AbstractValidator<V1RefreshRequest>
{
    public V1RefreshRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
