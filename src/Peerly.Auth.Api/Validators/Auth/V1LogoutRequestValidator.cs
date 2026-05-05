using FluentValidation;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Validators.Auth;

internal sealed class V1LogoutRequestValidator : AbstractValidator<V1LogoutRequest>
{
    public V1LogoutRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
